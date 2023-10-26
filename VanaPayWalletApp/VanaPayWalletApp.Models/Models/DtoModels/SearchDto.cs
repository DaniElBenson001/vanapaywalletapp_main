using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Entities;

namespace VanaPayWalletApp.Models.Models.DtoModels
{

    public record SearchOutputDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string AccNumber { get; set; } = string.Empty;
    }

    public record SearchInputDto
    {
        public string Acc { get; set; } = string.Empty;
    }

    public record RegularSearchDto()
    {
        public string search { get; set; } = string.Empty;
    }
}
