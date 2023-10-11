using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class TransactionsListingDto
    {
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string SenderAccountNo { get; set; } = string.Empty;
        public string ReceiverAccountNo { get; set; } = string.Empty;
        public string DateOfTxn { get; set; }
    }
}
