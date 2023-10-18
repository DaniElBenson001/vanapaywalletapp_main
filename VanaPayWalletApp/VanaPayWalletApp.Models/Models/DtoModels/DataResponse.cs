using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class DataResponse<T>
    {
        public DataResponse()
        {
            Status = true;
            StatusMessage = "You are Logged in and ready to go";
        }
        public bool Status { get; set; }
        public string? StatusMessage { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
