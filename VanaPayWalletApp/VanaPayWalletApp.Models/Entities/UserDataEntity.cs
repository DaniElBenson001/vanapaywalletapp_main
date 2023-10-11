/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;
using VanaPayWalletApp.Models.Models.DtoModels;

namespace VanaPayWalletApp.Models.Entities
{
    
    public record UserDataEntity
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public string? VerificationToken { get; set; } = string.Empty;

        //public string? RefreshToken { get; set; } = string.Empty;
        //public string? RefreshTokenExpiryTime { get; set; } = string.Empty;
        //public string? UserToken { get; set; } = string.Empty;
        //public DateTime? VerifiedAt { get; set; }
        //public string? PasswordResetToken { get; set; }
        //public DateTime? ResetTokenExpires { get; set; }

        //internal virtual ICollection<UserAccountDetails> UserAccountDetails { get; set; }

        public List<AccountDataEntity> Account { get; set; }
        public virtual List<TransactionDataEntity> SenderTransaction { get; set; }
        public virtual List<TransactionDataEntity> ReceiverTransaction { get; set; }

    }
}