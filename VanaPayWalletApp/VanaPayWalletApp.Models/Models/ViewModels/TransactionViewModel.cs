using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.ViewModels
{
    public class AdminTransactionViewModel
    {
        public string senderName { get; set; } = string.Empty;
        public string receiverName { get; set; } = string.Empty;
        public string? senderAcctNo { get; set; } = string.Empty;
        public string? receiverAcctNo { get; set; } = string.Empty;
        public string? senderUsername {  get; set; } = string.Empty;
        public string? receiverUsername {  get; set; } = string.Empty;
        public string? reference {  get; set; } = string.Empty;
        public decimal? amount { get; set; }
        public DateTime date { get; set; }
        public string transacType { get; set; }
        //public string Currency { get; set; }

    }

    public class UserTransactionViewModel
    {
        public string fullname { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string accNo { get; set; } = string.Empty;
        public decimal? amount { get; set; }
        public DateTime date { get; set; }
        public string transacType { get; set;}
    }
}
