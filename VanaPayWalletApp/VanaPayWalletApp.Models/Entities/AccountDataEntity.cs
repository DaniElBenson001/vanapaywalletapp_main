using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Entities
{
    public class AccountDataEntity
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AccountDataEntity()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Balance = 10000;
            Currency = "NGN";
        }

        [Key]
        public int Id { get; set; }

        public string AccountNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Balance { get; set; }
        public string Currency { get; set; }

        [ForeignKey("UserDataEntity")]
        public int UserId { get; set; }
        public UserDataEntity UserDataEntity { get; set; }

    }
}
