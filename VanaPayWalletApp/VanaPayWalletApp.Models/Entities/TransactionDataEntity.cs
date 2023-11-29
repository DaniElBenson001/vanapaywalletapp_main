using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Entities
{
    public class TransactionDataEntity
    {
        public int Id { get; set; }
        public string? SenderAccountNo { get; set; }
        public string? ReceiverAccountNo { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public DateTime DateOfTxn { get; set; }
        public int? SenderUserId { get; set; }
        public int? ReceiverUserId { get; set; }

        [ForeignKey("SenderUserId")]
        public virtual UserDataEntity SenderUser { get; set; }

        [ForeignKey("ReceiverUserId")]
        public virtual UserDataEntity ReceiverUser { get; set; }
    }
}