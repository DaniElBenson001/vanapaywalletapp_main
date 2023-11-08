using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.DataContext;
using VanaPayWalletApp.Models.Entities;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly VanapayDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Constructor for the Dashboard Service
        public DashboardService(VanapayDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;                             //Parameter that grant access to data in the Database Table
            _httpContextAccessor = httpContextAccessor;     //Parameter that grant access to data in the HTTP Context, the JWT Claims Identifiers and More

        }

        //Method to retrieve the Dashboard Information about the User
        public async Task<DashboardDto> GetDashboardInfo()
        {
            var response = new DashboardDto();
            var user = new UserDataEntity();

            int userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userData = await _context.Accounts.Include("UserDataEntity")
                .Where(userInfo => userInfo.UserId == userID)
                .Select(userInfo => new DashboardDto
                {
                    FullName = $"{userInfo.UserDataEntity.FirstName} {userInfo.UserDataEntity.LastName}",
                    UserName = userInfo.UserDataEntity.UserName,
                    AccountNumber = userInfo.AccountNumber,
                    Balance = userInfo.Balance
                })
                .FirstOrDefaultAsync();

            
            await _context.SaveChangesAsync();

            return userData!;
        }
    }
}

