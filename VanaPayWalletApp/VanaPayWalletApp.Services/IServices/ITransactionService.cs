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
        Task<List<TransactionViewModel>> GetTransactionHistory();
        Task<List<TransactionsListingDto>> GetAllTransactions();

    };
}
