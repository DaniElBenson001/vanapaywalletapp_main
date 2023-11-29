using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Models.ViewModels;

namespace VanaPayWalletApp.Models.Models.DtoModels.Webhook
{
    public class WebHookDto
    {
        public string @event { get; set; } = string.Empty;
        public Webhook.Data  data { get; set; }
    }
}
