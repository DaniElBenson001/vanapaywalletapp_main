using Microsoft.Identity.Client;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Entities;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class DashboardDto
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string AccountNumber { get; set; }
        public decimal? Balance { get; set; }

        //[ForeignKey("AccountDataEntity")]
        //public int UserId { get; set; }
    }
}
