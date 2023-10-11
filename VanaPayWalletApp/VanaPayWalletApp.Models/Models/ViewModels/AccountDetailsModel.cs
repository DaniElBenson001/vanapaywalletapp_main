using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.ViewModels
{
    public class AccountDetailsModel
    {
        public decimal Balance { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Currency { get; set ;} = string.Empty;
    }
}
