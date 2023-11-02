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

        public PaymentService(VanapayDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<PaymentService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<DataResponse<PaystackRequestView>> InitializePayment(DepositDto deposit)
        {
            var PaymentResponse = new DataResponse<PaystackRequestView>();
            try
            {
                int userID;

                //Condition to check if the HttpContextAccessor does not contain any tangible value, sending the appropriate pin response
                if (_httpContextAccessor.HttpContext == null)
                {
                    PaymentResponse.Status = false;
                    PaymentResponse.StatusMessage = $"User does not Exist";
                    return PaymentResponse;
                }

                //Handpicks the user Id embedded in the Claims and converts the value to Integers
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    
                //Variable for User 
                var user = await _context.Users.Where(v => v.Id == userID).FirstOrDefaultAsync();
                var account = await _context.Accounts.Where(x => x.AccountId == userID).FirstOrDefaultAsync();

                if(deposit.Amount <= 0)
                {
                    PaymentResponse.Status = false;
                    PaymentResponse.StatusMessage = "You cannot Send Funds less than or equals to 0";
                    return PaymentResponse;
                }

                var amountinKobo =  (deposit.Amount * 100 );

                var PaymentData = new PaystackRequestDto
                {
                    email = user!.Email,
                    amount = amountinKobo.ToString(),
                    currency = "NGN",
                    reference = TxnReferenceGenerator().ToString()
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
                    PaymentResponse.Status = false;
                    PaymentResponse.StatusMessage = "Status Code is not Code 200";
                    return PaymentResponse;
                }

                var respData = JsonConvert.DeserializeObject<PaystackRequestDto>(ResponseBody);

                if(respData != null)
                {
                    var newResponse = new DepositDataEntity()
                    {
                        DepositId = account.AccountId,
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

                PaymentResponse.Status = true;
                PaymentResponse.StatusMessage = "Payment Initializtion Successful";
                return PaymentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
                PaymentResponse.Status = false;
                PaymentResponse.StatusMessage = ex.Message;
                return PaymentResponse;
            }
        }


        //public async Task<DataResponse<>>


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
