using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.DataContext;
using VanaPayWalletApp.Models.Entities;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.ViewModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Services.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly VanapayDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionService> _logger;
        private readonly IConfiguration _configuration;

        public TransactionService(VanapayDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<TransactionService> logger, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        //Method to Make a Transfer Transaction
        public async Task<DataResponse<string>> MakeTransactionTransfer(TransactionDto transfer)
        {
            UserDataEntity userData = new();
            DataResponse<string> transferResponse = new();

            try
            {
                int userID;

                //If the userID logged in is null
                if (_httpContextAccessor.HttpContext == null)
                {
                    transferResponse.status = false;
                    transferResponse.statusMessage = "USER NOT FOUND";
                    return transferResponse;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                var SenderAccountData = await _context.Accounts.Where(u => u.AccountNumber == transfer.SenderAcctNo).FirstAsync();
                //If the Sender Account is null
                if (SenderAccountData == null)
                {
                    transferResponse.status = false;
                    transferResponse.statusMessage = "Invalid Account Number";
                    return transferResponse;
                }

                var ReceiverAccountData = await _context.Accounts.Include("UserDataEntity").Where(u => u.AccountNumber == transfer.ReceiverAcctNo).FirstAsync();

                //If the Receiever Account is null and Invalid
                if (ReceiverAccountData == null)
                {
                    transferResponse.status = false;
                    transferResponse.statusMessage = "Invalid Account Number";
                    return transferResponse;
                }

                //If the Receiver Account Number is less than 10 digits
                if (ReceiverAccountData.AccountNumber.Length < 10)
                {
                    transferResponse.status = false;
                    transferResponse.statusMessage = "Your Given Account Number must 10-Digits";
                    return transferResponse;
                }

                //If the Account User wants to act dumb and send to Himself/Herself when there is a Deposit Function for Him >:(
                if (SenderAccountData!.UserId == ReceiverAccountData!.UserId)
                {
                    transferResponse.status = false;
                    transferResponse.statusMessage = "Please Do not send Funds to yourself, Kindly Deposit";
                    return transferResponse;
                }

                //The Real official Transfer Transaction
                TransactionDataEntity newTransfer = new()
                {
                    SenderAccountNo = SenderAccountData.AccountNumber,
                    ReceiverAccountNo = ReceiverAccountData.AccountNumber,
                    Amount = transfer.Amount,
                    Reference = ReferenceGenerator(),
                    DateOfTxn = DateTime.Now,
                    SenderUserId = SenderAccountData.UserId,
                    ReceiverUserId = ReceiverAccountData.UserId
                };

                //If there is Insufficient funds, where the Account User's Mouth is as Dry as the Sands of the Desert, therein He/she can cry, "THERE IS NO MONEY ON GROUND"!! 
                if (transfer.Amount > SenderAccountData.Balance)
                {
                    transferResponse.status = false;
                    transferResponse.statusMessage = "Insufficient Funds; No fear, More is Coming!";
                    return transferResponse;
                }

                //If the Account User decides to act dumb yet again and send Value less than or equals to NGN 0.00 >:( [Omoo, some people are funny o, is it that they want to be transferring their debts ni?!!!]
                if (transfer.Amount <= 0)
                {
                    transferResponse.status = false;
                    transferResponse.statusMessage = "You cannot send amount less than or equals to NGN 0.00, Guy Own up, stop sending debts";
                    return transferResponse;
                }

                SenderAccountData.Balance -= transfer.Amount;
                ReceiverAccountData.Balance += transfer.Amount;
                
                await _context.Transactions.AddAsync(newTransfer);
                await _context.SaveChangesAsync();

                transferResponse.status = true;
                transferResponse.statusMessage = "Transfer Successful";

                return transferResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.Now.ToLongTimeString()}, {DateTime.Now.ToLongDateString()}");
            }
            return transferResponse;
        }

        //Method to get the Transactions History of the Logged in User
        public async Task<List<AdminTransactionViewModel>> GetTransactionHistoryAsAdmin()
        {
            //Initializes the Instance of the Model Class TransactionViewModel, setting it in a Generic Class called List<>
            List<AdminTransactionViewModel> transactionHistory = new();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return transactionHistory;
                }

                //Having the ID of the Logged in encoded in the JSON Web Token Initialized upon Log in, it decodes the Name ID and converts it to an Integer for Use in the Method Logic
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                //Compares the userID integer with the Account ID 
                var userTransactions = await _context.Transactions.Include("SenderUser").Include("ReceiverUser")
                    .Where(txn => txn.SenderUserId == userID || txn.ReceiverUserId == userID).ToListAsync();
 
                var user = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();

                //A foreach loop is created to spool a List of transactions in an Array<JSON> Format
                foreach (var txn in userTransactions)
                {
                    //Checks if the Reciever is the userID in question and the Sender is another ID yielding a CREDIT
                    if (txn.SenderUserId != null && txn.ReceiverUserId == userID)
                    {
                        transactionHistory.Add(new AdminTransactionViewModel
                        {
                            senderName = $"{txn.SenderUser.FirstName} {txn.SenderUser.LastName}",
                            receiverName = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}",

                            senderAcctNo = $"{txn.SenderAccountNo}",
                            receiverAcctNo = $"{txn.ReceiverAccountNo}",

                            senderUsername = $"{txn.SenderUser.UserName}",
                            receiverUsername = $"{txn.ReceiverUser.UserName}",

                            reference = $"{ReferenceGenerator()}",
                            transacType = "<span class=\"cr\">CREDIT</span>",
                            amount = txn.Amount,
                            date = txn.DateOfTxn
                        });
                    }

                    //Checks if the Sender is the userID in question and the Receiver is another ID yielding a DEBIT
                    if (txn.SenderUserId == userID && txn.ReceiverUserId != null)
                    {
                        transactionHistory.Add(new AdminTransactionViewModel
                        {
                            senderName = $"{txn.SenderUser.FirstName} {txn.SenderUser.LastName}",
                            receiverName = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}",
                            
                            senderAcctNo = $"{txn.SenderAccountNo}",
                            receiverAcctNo = $"{txn.ReceiverAccountNo}",
                            
                            transacType = "<span class=\"dr\">DEBIT</span>",
                            amount = txn.Amount,
                            date = txn.DateOfTxn
                        });
                    }

                }
                return transactionHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.Now.ToLongTimeString()}, {DateTime.Now.ToLongDateString()}");
            }
            return transactionHistory;
        }

        //Method to get the Transaction History as a User
        public async Task<List<UserTransactionViewModel>> GetTransactionHistoryAsUser()
        {
            //Initializes the Instance of the Model Class TransactionViewModel, setting it in a Generic Class called List<>
            List<UserTransactionViewModel> getTransactions = new();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return getTransactions;
                }

                //Having the ID of the Logged in encoded in the JSON Web Token Initialized upon Log in, it decodes the Name ID and converts it to an Integer for Use in the Method Logic
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                //Compares the userID integer with the Account ID 
                var userTransactions = await _context.Transactions.Include("SenderUser").Include("ReceiverUser")
                    .Where(txn => txn.SenderUserId == userID || txn.ReceiverUserId == userID).OrderByDescending(x => x.DateOfTxn).ToListAsync();

                var user = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();

                //A foreach loop is created to spool a List of transactions in an Array<JSON> Format
                foreach (var txn in userTransactions)
                {
                    //Checks if the Reciever is the userID in question and the Sender is another ID yielding a CREDIT
                    if (txn.SenderUserId != null && txn.ReceiverUserId == userID)
                    {
                        getTransactions.Add(new UserTransactionViewModel
                        {
                            fullname = $"{txn.SenderUser.FirstName} {txn.SenderUser.LastName}",
                            username = $"{txn.SenderUser.UserName}",
                            accNo = $"{txn.SenderAccountNo}",

                            amount = txn.Amount,
                            transacType = "<div id=\"txn-type-credit\">credit</div>",
                            date = txn.DateOfTxn
                        });
                    }

                    //Checks if the Sender is the userID in question and the Receiver is another ID yielding a DEBIT
                    if (txn.SenderUserId == userID && txn.ReceiverUserId != null)
                    {
                        getTransactions.Add(new UserTransactionViewModel
                        {
                            fullname = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}",
                            username = $"{txn.ReceiverUser.UserName}",
                            accNo = $"{txn.ReceiverAccountNo}",

                            amount = txn.Amount,
                            transacType = "<div id=\"txn-type-debit\">debit</div>",
                            date = txn.DateOfTxn
                        });
                    }

                }
                return getTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.Now.ToLongTimeString()}, {DateTime.Now.ToLongDateString()}");
            }
            return getTransactions;
        }

        //Method to get the three most recent transactions
        public async Task<DataResponse<List<UserTransactionViewModel>>> GetThreeMostRecentTransactions()
        {
            //Initializes the Instance of the Model Class TransactionViewModel, setting it in a Generic Class called List<>
            DataResponse<List<UserTransactionViewModel>> getTransactions = new();

            try
            {
                getTransactions.status = true;
                getTransactions.statusMessage = "Transaction History Received";

                var transactionData = await GetTransactionHistoryAsUser();
                getTransactions.data = transactionData.OrderByDescending(x => x.date).Take(3).ToList();
                return getTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.Now.ToLongTimeString()}, {DateTime.Now.ToLongDateString()}");
            }
            return getTransactions;
        }

        //Method to get the Transaction History by Date Range
        public async Task<DataResponse<List<UserTransactionViewModel>>> GetTransacHistoryByDate(DateDto date)
        {
            DataResponse<List<UserTransactionViewModel>> getTransactions = new();

            try
            {
                getTransactions.status = true;
                getTransactions.statusMessage = "Transaction History Received";


                //Remember, the Start Date is the PAST, the End Date is the PRESENT
                var startDate = date.startDate.Date;
                var endDate = date.endDate.Date;
                

                var transactionData = await GetTransactionHistoryAsUser();
                getTransactions.data = transactionData.Where(x => x.date.Date >= startDate  && x.date.Date <= endDate).ToList();
                return getTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.Now.ToLongTimeString()}, {DateTime.Now.ToLongDateString()}");
            }
            return getTransactions;
        }

        //Method to get the Transaction History by Day
        public async Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryToday()
        {
            DataResponse<List<UserTransactionViewModel>> getTransactions = new();

            try
            {
                getTransactions.status = true;
                getTransactions.statusMessage = "Transaction History Received";

                var startDate = DateTime.Now.Date;
                var endDate = DateTime.Now.Date;

                var transactionData = await GetTransactionHistoryAsUser();
                getTransactions.data = transactionData.Where(x => x.date.Date >= startDate && x.date.Date <= endDate).ToList();
                return getTransactions;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
            }
            return getTransactions;
        }

        //Method to get the Transaction History Yesterday
        public async Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryYesterday()
        {
            DataResponse<List<UserTransactionViewModel>> getTransactions = new();
            try
            {
                getTransactions.status = true;
                getTransactions.statusMessage = "Transaction History Received";

                var startDate = DateTime.Now.Date.AddDays(-1);
                var endDate = DateTime.Now.Date;

                var transactionData = await GetTransactionHistoryAsUser();
                getTransactions.data = transactionData.Where(x => x.date.Date >= startDate && x.date.Date <= endDate).ToList();
                return getTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
            }
            return getTransactions;
        }

        //Method to get the Transaction History 3 days ago
        public async Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryThreeDaysAgo()
        {
            DataResponse<List<UserTransactionViewModel>> getTransactions = new();

            try
            {
                getTransactions.status = true;
                getTransactions.statusMessage = "Transaction History Received";

                var startDate = DateTime.Now.Date.AddDays(-3);
                var endDate = DateTime.Now.Date;

                var transactionData = await GetTransactionHistoryAsUser();
                getTransactions.data = transactionData.Where(x => x.date.Date >= startDate && x.date.Date <= endDate).ToList();
                return getTransactions;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
            }
            return getTransactions;
        }

        //Method to get the Transaction History 7 days ago
        public async Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistorySevenDaysAgo()
        {
            DataResponse<List<UserTransactionViewModel>> getTransactions = new();

            try
            {
                getTransactions.status = true;
                getTransactions.statusMessage = "Transaction History Received";

                var startDate = DateTime.Now.Date.AddDays(-7);
                var endDate = DateTime.Now.Date;

                var transactionData = await GetTransactionHistoryAsUser();
                getTransactions.data = transactionData.Where(x => x.date.Date >= startDate && x.date.Date <= endDate).ToList();
                return getTransactions;

            }
            catch (Exception ex)
            {
                _logger.LogError( $"{ex.Message} ||| {ex.StackTrace}");
            }
            return getTransactions;
        }

        //Method to get Transaction History One Month ago
        public async Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryOneMonthAgo()
        {
            DataResponse<List<UserTransactionViewModel>> getTransactions = new();
            try
            {
                getTransactions.status = true;
                getTransactions.statusMessage = "Transaction History Received";

                var startDate = DateTime.Now.Date.AddMonths(-1);
                var endDate = DateTime.Now.Date;

                var transactionData = await GetTransactionHistoryAsUser();
                getTransactions.data = transactionData.Where(x => x.date.Date >= startDate && x.date.Date <= endDate).ToList();
                return getTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
            }
            return getTransactions;
        }

        //Method that gets every single Transaction for the Admin Side
        public async Task<List<TransactionsListingDto>> GetAllTransactions()
        {
            List<TransactionsListingDto> getAllTransactions = new();

            try
            {
                var allTransactions = await _context.Transactions.Include("SenderUser").Include("ReceiverUser").ToListAsync();

                foreach(var txn in allTransactions)
                {
                    getAllTransactions.Add(new TransactionsListingDto()
                    {
                        Reference = txn.Reference,
                        Amount = txn.Amount,
                        SenderAccountNo = txn.SenderAccountNo!,
                        ReceiverAccountNo = txn.ReceiverAccountNo!,
                        DateOfTxn = txn.DateOfTxn
                    });  
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.Now.ToLongTimeString()}, {DateTime.Now.ToLongDateString()}");
            }
            return getAllTransactions;
        }

        //Method to Generate a string Value for Transaction Referencing
        public string ReferenceGenerator()
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
            string ReferenceString = $"VNPY-Txn-{StrBuild}";
            return ReferenceString.ToString();
        }
    }
}
