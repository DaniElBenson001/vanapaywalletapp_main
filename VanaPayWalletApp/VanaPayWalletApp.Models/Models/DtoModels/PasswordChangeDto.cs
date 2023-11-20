using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class PasswordChangeDto
    {
        public string oldPassword {  get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
    }

    public class PasswordDto
    {
        public string password { get; set;}
    }
}
