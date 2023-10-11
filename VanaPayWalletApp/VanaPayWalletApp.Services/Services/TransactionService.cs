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
        private readonly VanaPayDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(VanaPayDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<TransactionService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        //Method to Make a Transfer Transaction
        public async Task<RegisterViewModel> MakeTransactionTransfer(TransactionDto transfer)
        {
            UserDataEntity userData = new UserDataEntity();
            RegisterViewModel transferMessage = new RegisterViewModel();

            try
            {
                int userID;

                //If the userID logged in is null
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new RegisterViewModel();
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

                var ReceiverAccountData = await _context.Accounts.Where(u => u.AccountNumber == transfer.ReceiverAcctNo).FirstAsync();
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
                TransactionDataEntity newTransfer = new TransactionDataEntity()
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

                SenderAccountData.Balance = SenderAccountData.Balance - transfer.Amount;
                ReceiverAccountData.Balance = ReceiverAccountData.Balance + transfer.Amount;
                
                await _context.Transactions.AddAsync(newTransfer);
                await _context.SaveChangesAsync();

                transferMessage.Status = true;
                transferMessage.StatusMessage = "Transfer Successful";
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
            List<TransactionViewModel> transactionHistory = new List<TransactionViewModel>();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return transactionHistory;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                var loggedInUser = await _context.Accounts.Include("UserDataEntity").Where(snderID => snderID.AccountId == userID).FirstOrDefaultAsync();
                var userTransactions = await _context.Transactions.Include("SenderUser").Include("ReceiverUser")
                    .Where(txn => txn.SenderUserId == userID || txn.ReceiverUserId == userID).ToListAsync();

                foreach (var txn in userTransactions)
                {
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
            List<TransactionsListingDto> getAllTransactions = new List<TransactionsListingDto>();

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
            Random RNG = new Random();
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
