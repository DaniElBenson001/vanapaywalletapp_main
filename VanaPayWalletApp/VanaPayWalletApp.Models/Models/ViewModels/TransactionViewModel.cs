using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.ViewModels
{
    public class TransactionViewModel
    {
        public string SenderInfo { get; set; } = string.Empty;
        public string ReceiverInfo { get; set; } = string.Empty;
        public string? SenderAcctNo { get; set; } = string.Empty;
        public string? ReceiverAcctNo { get; set; } = string.Empty;
        //public int? SenderUserId { get; set; }
        //public int? ReceiverUserId { get; set; }
        public decimal Amount { get; set; }
        //public string Reference { get; set; } = string.Empty;
        //public string Status { get; set; } = string.Empty;
        public string Date { get; set; }
        public string TransacType { get; set; }
        //public string Currency { get; set; }

    }
}
