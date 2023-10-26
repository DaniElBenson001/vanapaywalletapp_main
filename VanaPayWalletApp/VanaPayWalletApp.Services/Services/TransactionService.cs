using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public TransactionService(VanapayDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<TransactionService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        //Method to Make a Transfer Transaction
        public async Task<DataResponse<DashboardDto>> MakeTransactionTransfer(TransactionDto transfer)
        {
            UserDataEntity userData = new();
            DataResponse<DashboardDto> transferMessage = new();

            try
            {
                int userID;

                //If the userID logged in is null
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new DataResponse<DashboardDto>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                var SenderAccountData = await _context.Accounts.Where(u => u.AccountNumber == transfer.SenderAcctNo).FirstAsync();
                //If the Sender Account is null
                if (SenderAccountData == null)
                {
                    transferMessage.Status = false;
                    transferMessage.StatusMessage = "Invalid Account Number";
                    return transferMessage;
                }

                var ReceiverAccountData = await _context.Accounts.Include("UserDataEntity").Where(u => u.AccountNumber == transfer.ReceiverAcctNo).FirstAsync();

                //If the Receiever Account is null and Invalid
                if (ReceiverAccountData == null)
                {
                    transferMessage.Status = false;
                    transferMessage.StatusMessage = "Invalid Account Number";
                    return transferMessage;
                }

                //If the Receiver Account Number is less than 10 digits
                if (ReceiverAccountData.AccountNumber.Length < 10)
                {
                    transferMessage.Status = false;
                    transferMessage.StatusMessage = "Your Given Account Number must 10-Digits";
                    return transferMessage;
                }

                //If the Account User wants to act dumb and send to Himself/Herself when there is a Deposit Function for Him >:(
                if (SenderAccountData!.AccountId == ReceiverAccountData!.AccountId)
                {
                    transferMessage.Status = false;
                    transferMessage.StatusMessage = "Please Do not send Funds to yourself, Kindly Deposit";
                    return transferMessage;
                }

                //The Real official Transfer Transaction
                TransactionDataEntity newTransfer = new()
                {
                    SenderAccountNo = SenderAccountData.AccountNumber,
                    ReceiverAccountNo = ReceiverAccountData.AccountNumber,
                    Amount = transfer.Amount,
                    Reference = ReferenceGenerator(),
                    DateOfTxn = DateTime.UtcNow.ToString("dd/M/yyyy hh:mm:ss tt"),
                    SenderUserId = SenderAccountData.AccountId,
                    ReceiverUserId = ReceiverAccountData.AccountId
                };

                //If there is Insufficient funds, where the Account User's Mouth is as Dry as the Sands of the Desert, therein He/she can cry, "THERE IS NO MONEY ON GROUND"!! 
                if (transfer.Amount > SenderAccountData.Balance)
                {
                    transferMessage.Status = false;
                    transferMessage.StatusMessage = "Insufficient Funds; No fear, More is Coming!";
                    return transferMessage;
                }

                //If the Account User decides to act dumb yet again and send Value less than or equals to NGN 0.00 >:( [Omoo, some people are funny o, is it that they want to be transferring their debts ni?!!!]
                if (transfer.Amount <= 0)
                {
                    transferMessage.Status = false;
                    transferMessage.StatusMessage = "You cannot send amount less than or equals to NGN 0.00, Guy Own up, stop sending debts";
                    return transferMessage;
                }

                SenderAccountData.Balance -= transfer.Amount;
                ReceiverAccountData.Balance += transfer.Amount;
                
                await _context.Transactions.AddAsync(newTransfer);
                await _context.SaveChangesAsync();

                transferMessage.Status = true;
                transferMessage.StatusMessage = "Transfer Successful";
                transferMessage.Data = new DashboardDto()
                {
                    FullName = $"{ReceiverAccountData.UserDataEntity.FirstName} {ReceiverAccountData.UserDataEntity.LastName}",
                    UserName = $"{ReceiverAccountData.UserDataEntity.UserName}",
                    AccountNumber = ReceiverAccountData.AccountNumber
                    
                };
                return transferMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
            }
            return transferMessage;
        }

        //Method to get the Transactions History of the Logged in User
        public async Task<List<TransactionViewModel>> GetTransactionHistory()
        {
            //Initializes the Instance of the Model Class TransactionViewModel, setting it in a Generic Class called List<>
            List<TransactionViewModel> transactionHistory = new();

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

                //A foreach loop is created to spool a List of transactions in an Array<JSON> Format
                foreach (var txn in userTransactions)
                {
                    //Checks if the Reciever is the userID in question and the Sender is another ID yielding a CREDIT
                    if (txn.SenderUserId != null && txn.ReceiverUserId == userID)
                    {
                        transactionHistory.Add(new TransactionViewModel
                        {
                            Amount = txn.Amount,
                            SenderInfo = $"{txn.SenderUser.FirstName} {txn.SenderUser.LastName}",
                            ReceiverInfo = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}",
                            SenderAcctNo = $"{txn.SenderAccountNo}",
                            ReceiverAcctNo = $"{txn.ReceiverAccountNo}",
                            TransacType = "CREDIT",
                            Date = txn.DateOfTxn
                        });
                    }

                    //Checks if the Sender is the userID in question and the Receiver is another ID yielding a DEBIT
                    if (txn.SenderUserId == userID && txn.ReceiverUserId != null)
                    {
                        transactionHistory.Add(new TransactionViewModel
                        {
                            Amount = txn.Amount,
                            SenderInfo = $"{txn.SenderUser.FirstName} {txn.SenderUser.LastName}",
                            ReceiverInfo = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}",
                            SenderAcctNo = $"{txn.SenderAccountNo}",
                            ReceiverAcctNo = $"{txn.ReceiverAccountNo}",
                            TransacType = "DEBIT",
                            Date = txn.DateOfTxn
                        });
                    }

                }
                return transactionHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
            }
            return transactionHistory;
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
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
            }
            return getAllTransactions;
        }

        //Method to Generate a string Value for Transaction Referencing
        private static string ReferenceGenerator()
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
