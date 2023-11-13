using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Entities;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.ViewModels;

namespace VanaPayWalletApp.Services.IServices
{
    public interface ITransactionService
    {
        Task<DataResponse<string>> MakeTransactionTransfer(TransactionDto transfer);
        Task<List<AdminTransactionViewModel>> GetTransactionHistoryAsAdmin();
        Task<List<UserTransactionViewModel>> GetTransactionHistoryAsUser();
        Task<List<TransactionsListingDto>> GetAllTransactions();
        Task<DataResponse<List<UserTransactionViewModel>>> GetThreeMostRecentTransactions();
        Task<DataResponse<List<UserTransactionViewModel>>> GetTransacHistoryByDate(DateDto date);
        Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryToday();
        Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistorySevenDaysAgo();
        Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryThreeDaysAgo();
        Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryYesterday();
        Task<DataResponse<List<UserTransactionViewModel>>> GetTxnHistoryOneMonthAgo();
    };
}
