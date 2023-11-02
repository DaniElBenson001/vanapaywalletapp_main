using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.ViewModels
{
    public class PaystackRequestView
    {
        public bool status { get; set; }
        public string statusMessage { get; set; } = string.Empty;
        public Data data { get; set; }

    }

    public class Data
    {
        public string Authorization_url { get; set; } = string.Empty;
        public string Access_code { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;

    }


}
