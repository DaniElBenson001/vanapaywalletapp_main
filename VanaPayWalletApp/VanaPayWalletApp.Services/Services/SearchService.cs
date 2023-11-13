using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.DataContext;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.ViewModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Services.Services
{
    public class SearchService : ISearchService
    {
        private readonly VanapayDbContext _context;
        private readonly ILogger<SearchService> _logger;

        public SearchService(VanapayDbContext context, ILogger<SearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Method to Search for a User via His Account Number - Primarily used for Transfer
        public async Task<DataResponse<SearchOutputDto>> SearchUserByAccNo(SearchInputDto acc)
        {
            var searchResult = new DataResponse<SearchOutputDto>();

            try
            {

                var AccInfo = await _context.Accounts.Include("UserDataEntity").Where(user => user.AccountNumber == acc.Acc).FirstOrDefaultAsync();

                if (AccInfo == null)
                {
                    searchResult.status = false;
                    searchResult.statusMessage = "User Not Founf";
                }

                if (AccInfo != null)
                {
                    searchResult.status = true;
                    searchResult.statusMessage = "User Found";
                    searchResult.data = new SearchOutputDto()
                    {
                        FirstName = AccInfo.UserDataEntity.FirstName,
                        LastName = AccInfo.UserDataEntity.LastName,
                        UserName = AccInfo.UserDataEntity.UserName,
                        AccNumber = AccInfo.AccountNumber
                    };
                }
                else
                {
                    searchResult.status = false;
                    searchResult.statusMessage = "Account Does not Exist";
                    return searchResult;
                }

                if (acc.Acc == "")
                {
                    searchResult.status = false;
                    searchResult.statusMessage = "Please Input Value";
                    return searchResult;
                }
            }


            catch (Exception ex)
            {
                _logger.LogError($" {ex.Message} ||| {ex.StackTrace}");
                searchResult.status = false;
                searchResult.statusMessage = ex.Message;

                return searchResult;
            }

            return searchResult;
        }

        //Method to search for a User in the Application
        public async Task<DataResponse<SearchOutputDto>> SearchUser(RegularSearchDto search)
        {
            var searchResult = new DataResponse<SearchOutputDto>();

            try
            {
                var UserInfo = await _context.Users.Where(user => user.UserName == search.search || user.FirstName == search.search || user.LastName == search.search).FirstOrDefaultAsync();

                if (UserInfo == null)
                {
                    searchResult.status = false;
                    searchResult.statusMessage = "User Not Found";
                    return searchResult;
                }

                if (search.search == "")
                {
                    searchResult.status = false;
                    searchResult.statusMessage = "Please Input Value";
                    return searchResult;
                }

                searchResult.status = true;
                searchResult.statusMessage = "User Found";
                searchResult.data = new SearchOutputDto()
                {
                    FirstName = UserInfo.FirstName,
                    LastName = UserInfo.LastName,
                    UserName = UserInfo.UserName
                };
            }

            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
                searchResult.status = false;
                searchResult.statusMessage = ex.Message;
                return searchResult;
            }

            return searchResult;
        }
    }
}
       