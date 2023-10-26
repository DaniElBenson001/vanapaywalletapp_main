using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Models.DtoModels;

namespace VanaPayWalletApp.Services.IServices
{
    public interface ISearchService
    {
        Task<DataResponse<SearchOutputDto>> SearchUserByAccNo(SearchInputDto acc);
        Task<DataResponse<SearchOutputDto>> SearchUser(RegularSearchDto search);
    }
}
