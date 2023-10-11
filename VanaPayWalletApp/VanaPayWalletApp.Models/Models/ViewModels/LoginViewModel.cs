using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.ViewModels
{
    public class LoginViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string VerificationToken { get; set; } = string.Empty;

    }
}
