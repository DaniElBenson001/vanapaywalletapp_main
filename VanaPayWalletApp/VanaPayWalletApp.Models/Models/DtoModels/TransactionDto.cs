using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class TransactionDto
    {
        public string? SenderAcctNo { get; set; } = string.Empty;
        public string? ReceiverAcctNo { get; set;} = string.Empty;
        public decimal Amount { get; set; }

    }
}
