using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class TokenDto
    {
        public string? VerificationToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
        public string? RefreshTokenExpiryTime { get; set; } = string.Empty;
        public string? UserToken { get; set; } = string.Empty;
    }
}
