using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Models.DtoModels;

namespace VanaPayWalletApp.Services.IServices
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardInfo();
        //Task<AccountTransactions> GetTransactionHistory();
    }
}
