using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using VanaPayWalletApp.DataContext;
using VanaPayWalletApp.Models.Entities;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.DtoModels.Webhook;
using VanaPayWalletApp.Models.Models.ViewModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly VanapayDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient client = new HttpClient();
        private readonly ITransactionService _transactionService;

        public PaymentService(VanapayDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<PaymentService> logger, ITransactionService transactionService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
            _transactionService = transactionService;
        }

        public async Task<DataResponse<PaystackRequestView>> InitializePayment(DepositDto deposit)
        {
            var paymentResponse = new DataResponse<PaystackRequestView>();
            try
            {
                int userID;

                //Condition to check if the HttpContextAccessor does not contain any tangible value, sending the appropriate pin response
                if (_httpContextAccessor.HttpContext == null)
                {
                    paymentResponse.status = false;
                    paymentResponse.statusMessage = $"User does not Exist";
                    return paymentResponse;
                }

                //Handpicks the user Id embedded in the Claims and converts the value to Integers
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    
                //Variable for User 
                var user = await _context.Users.Where(v => v.Id == userID).FirstOrDefaultAsync();
                var account = await _context.Accounts.Where(x => x.UserId == userID).FirstOrDefaultAsync();

                if(deposit.Amount <= 0)
                {
                    paymentResponse.status = false;
                    paymentResponse.statusMessage = "You cannot Send Funds less than or equals to 0";
                    return paymentResponse;
                }

                var amountinKobo =  (deposit.Amount * 100 );

                var PaymentData = new PaystackRequestDto
                {
                    email = user!.Email,
                    amount = amountinKobo.ToString()!,
                    currency = "NGN",
                    reference = _transactionService.ReferenceGenerator()
                };

                var secKey = _configuration.GetSection("PaystackPayment:Secret_Key").Value;
                var uri = _configuration.GetSection("PaystackPayment:URI").Value;

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secKey);
                var PaymentContent = new StringContent(JsonConvert.SerializeObject(PaymentData), Encoding.UTF8);
                using HttpResponseMessage PaymentRequest = await client.PostAsync(uri, PaymentContent);

                string ResponseBody = await PaymentRequest.Content.ReadAsStringAsync();
                PaymentRequest.EnsureSuccessStatusCode();

                if(PaymentRequest.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    paymentResponse.status = false;
                    paymentResponse.statusMessage = "Status Code is not Code 200";
                    return paymentResponse;
                }

                var respData = JsonConvert.DeserializeObject<PaystackRequestView>(ResponseBody);

                if(respData != null)
                {
                    var newResponse = new DepositDataEntity()
                    {
                        UserId = account!.UserId,
                        UserName = user.UserName,
                        Amount = deposit.Amount,
                        TxnReference = PaymentData.reference,
                        Email = PaymentData.email,
                        CreatedAt = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    await _context.Deposits.AddAsync(newResponse);
                    await _context.SaveChangesAsync();
                    
                }

                paymentResponse.status = true;
                paymentResponse.statusMessage = "Payment Initializtion Successful";
                paymentResponse.data = respData;
                return paymentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
                paymentResponse.status = false;
                paymentResponse.statusMessage = ex.Message;
                return paymentResponse;
            }
        }


        public async Task<DataResponse<WebHookDto>> PaymentWebHook(WebHookDto eventData)
        {
            var paymentResponse = new DataResponse<WebHookDto>();
            WebHookDto webhookResponse = new WebHookDto();

            try
            {
                var paymentInfo = await _context.Deposits.Where(pi => pi.TxnReference == eventData.data.reference).FirstOrDefaultAsync();
                var payerAccount = await _context.Users.Where(pa => pa.Id == paymentInfo!.UserId).FirstOrDefaultAsync();
                var userAccount = await _context.Accounts.Where(pa => pa.Id == paymentInfo!.UserId).FirstOrDefaultAsync();

                if(payerAccount == null)
                {
                    paymentResponse.status = false;
                    paymentResponse.statusMessage = "Error";
                    return paymentResponse;
                }

                if(paymentInfo!.Status != "Successful")
                {
                    paymentResponse.status = false;
                    paymentResponse.statusMessage = "Error";
                    return paymentResponse;
                }

                if(!(eventData.@event == "charge.success") || !(eventData.@event == paymentInfo.TxnReference))
                {
                    paymentInfo.Status = "Failed";
                    paymentInfo.CreatedAt = DateTime.Now;
                }

                paymentResponse.data = webhookResponse;

                paymentInfo.Status = "Successful";
                paymentInfo.Bank = eventData.data.authorization.bank;
                paymentInfo.CardType = eventData.data.authorization.card_type;
                paymentInfo.Channels = eventData.data.channel;
                paymentInfo.CustomerCode = eventData.data.customer.customer_code;
                paymentInfo.CreatedAt = DateTime.Now;

                userAccount!.Balance = userAccount.Balance + paymentInfo.Amount;

                await _context.SaveChangesAsync();

                var txnDeposit = new TransactionDataEntity()
                {
                    ReceiverUserId = payerAccount.Id,
                    ReceiverAccountNo = userAccount.AccountNumber,
                    Reference = eventData.data.reference,
                    Amount = paymentInfo.Amount,
                    DateOfTxn = paymentInfo.CreatedAt
                };

                await _context.Transactions.AddAsync(txnDeposit);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
                paymentResponse.status = false;
                paymentResponse.statusMessage = ex.Message;
                return paymentResponse;
            }

            return paymentResponse;
        }


        //Method to Generate a string Value for Transaction Funding Referencing
        private static string TxnReferenceGenerator()
        {
            Random RNG = new();
            const string refChars = "abcdefghijklmnopqrstuvwxyzABCDEFHIJKLMNOPQRSTUVWXYZ1234567890";
            int size = 24;
            var StrBuild = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var c = refChars[RNG.Next(0, refChars.Length)];
                StrBuild.Append(c);
            }
            string ReferenceString = $"VNPY-Txn-FUND-{StrBuild}";
            return ReferenceString.ToString();
        }
    }
}
