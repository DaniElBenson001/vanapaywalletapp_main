using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.ViewModels
{
    public class DataResponse<T>
    {
        public DataResponse()
        {
            status = true;
            statusMessage = "You are Logged in and ready to go";
        }  
        public bool status { get; set; }
        public string? statusMessage { get; set; } = string.Empty;
        public T? data  { get; set; }
    }
}
