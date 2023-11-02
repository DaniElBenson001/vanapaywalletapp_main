using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class PaystackRequestDto
    {
        public string email {  get; set; } = string.Empty;
        public string amount { get; set; }
        public string currency { get; set; } = string.Empty;
        public string reference { get; set; } = string.Empty;
    }
}
