using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Entities
{
    public record DepositDataEntity
    {
        [Key]
        public int Id { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? TxnReference { get; set; } = string.Empty;
        public string? Status {  get; set; } = string.Empty;
        public string? Channels {  get; set; } = string.Empty;
        public string? CardType {  get; set; } = string.Empty;
        public string? Bank { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? CustomerCode { get; set; } = string.Empty;
        public DateTime CreatedAt {  get; set; }

        [ForeignKey("UserDataEntity")]
        public int UserId {  get; set; }
        public UserDataEntity Deposit {  get; set; }

    }
}
