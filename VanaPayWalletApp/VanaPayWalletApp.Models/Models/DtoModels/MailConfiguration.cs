using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class MailConfiguration
    {
        public bool BCC { get; }
        public bool CC {  get; }
        public string Subject { get; set; }
        public string? DisplayName { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? UserName {  get; set; }
        public string? Password {  get; set; }
        public string? Host {  get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }
}
