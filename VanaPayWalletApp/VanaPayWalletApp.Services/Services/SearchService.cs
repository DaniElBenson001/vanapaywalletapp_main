﻿using MailKit.Search;
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

                if(AccInfo != null)
                {
                    searchResult.Status = true;
                    searchResult.StatusMessage = "User Found";
                    searchResult.Data = new SearchOutputDto()
                    {
                        FirstName = AccInfo.UserDataEntity.FirstName,
                        LastName = AccInfo.UserDataEntity.LastName,
                        UserName = AccInfo.UserDataEntity.UserName,
                        AccNumber = AccInfo.AccountNumber
                    };
                }
                else
                {
                    searchResult.Status = false;
                    searchResult.StatusMessage = "Account Does not Exist";
                }

                if (acc.Acc == "")
                {
                    searchResult.Status = false;
                    searchResult.StatusMessage = "Please Input Value";
                }
            }


            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                searchResult.Status = false;
                searchResult.StatusMessage = ex.Message;
               
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

                if(UserInfo == null)
                {
                    searchResult.Status = false;
                    searchResult.StatusMessage = "User Not Found";
                }

                if (search.search == "")
                {
                    searchResult.Status = false;
                    searchResult.StatusMessage = "Please Input Value";
                }

                if(UserInfo != null)
                {
                    searchResult.Status = true;
                    searchResult.StatusMessage = "User Found";
                    searchResult.Data = new SearchOutputDto()
                    {
                        FirstName = UserInfo.FirstName,
                        LastName = UserInfo.LastName,
                        UserName = UserInfo.UserName
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                searchResult.Status = false;
                searchResult.StatusMessage = ex.Message;

                return searchResult;
            }

            return searchResult;
        }
    }
}