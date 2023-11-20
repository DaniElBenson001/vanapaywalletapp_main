using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;
using VanaPayWalletApp.DataContext;
using VanaPayWalletApp.Models.Entities;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.ViewModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Services.Services
{
    public class UserService : IUserService
    {
        public static UserDataEntity user = new UserDataEntity();
        private readonly VanapayDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        //private readonly IAuthService _authService;
        //private readonly IMailService _mailService;

        public UserService(VanapayDbContext context, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        //The Register Method
        public async Task<ResponseViewModel> Register(UserRegisterRequest request)
        {
            ResponseViewModel registerResponse = new ResponseViewModel();

            try
            {
                CreatePasswordHash(request.password,
                    out byte[] passwordHash,
                    out byte[] passwordSalt);

                var userData = new UserDataEntity
                {
                    FirstName = request.firstName,
                    LastName = request.lastName,
                    Email = request.email,
                    UserName = request.username,
                    Address = request.address,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedAt = DateTime.Now
                };

                var userEmail = await _context.Users.AnyAsync(u => u.Email == request.email);
                if (userEmail)
                {
                    registerResponse.Status = false;
                    registerResponse.StatusMessage = "User Already Exists";
                    return registerResponse;
                }

                //Add New User and Save it to DB
                await _context.Users.AddAsync(userData);
                await _context.SaveChangesAsync();

                var userAccount = new AccountDataEntity
                {
                    AccountNumber = await AccountNumGen(),
                    Balance = 10000,
                    Currency = "NGN",
                    UserId = userData.Id,

                };

                //Add new Account and save it to DB
                await _context.Accounts.AddAsync(userAccount);
                await _context.SaveChangesAsync();

                registerResponse.Status = true;
                registerResponse.StatusMessage = "User Successfully Created";
                return registerResponse;
            }

            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message} ||| {ex.StackTrace}");
                return registerResponse;

            }
        }


        public async Task<DataResponse<string>> ChangePassword(PasswordChangeDto password)
        {
            var passwordResponse = new DataResponse<string>();

            try
            {
                int userID;

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Where(p => p.Id == userID).FirstAsync();

                if (user == null)
                {
                    passwordResponse.status = false;
                    passwordResponse.statusMessage = "USER NOT FOUND";
                    return passwordResponse;
                }

                if (!VerifyPasswordHash(password.oldPassword, user.PasswordHash!, user.PasswordSalt!))
                {
                    passwordResponse.status = false;
                    passwordResponse.statusMessage = "Your Old Password is not Correct";
                    return passwordResponse;
                }


                CreatePasswordHash(password.newPassword,
                 out byte[] passwordHash,
                 out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.PasswordModifiedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                passwordResponse.status = true;
                passwordResponse.statusMessage = "Password Successfully Updated";
            }
            catch (Exception ex)
            {
                _logger.LogError($" {ex.Message} ||| {ex.StackTrace} ");
                passwordResponse.status = false;
                passwordResponse.statusMessage = ex.Message;
                return passwordResponse;
            }
            return passwordResponse;
        }

        //Delete User
        public async Task<UserDataEntity?> DeleteUser(int id)
        {
            var response = new UserDataEntity();
            var accountResponse = new AccountDataEntity();

            if (_context.Users == null)
            {
                return null;
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            var acct = await _context.Accounts.FindAsync(id);
            if (acct == null)
            {
                return null;
            }

            _context.Users.Remove(user);
            _context.Accounts.Remove(acct);
            await _context.SaveChangesAsync();
            return response;
        }

        //Palindrome Code
        private static string PalindromeCode()
        {
            var firstFigure = new Random().Next(1, 9);
            var secondFigure = new Random().Next(0, 9);
            var palindromeNum = $"{firstFigure}{secondFigure}{firstFigure}";
            return palindromeNum;
        }

        public string DateCode()
        {
            var dateCode = "";
            var date = DateTime.Now.ToString("yyyyMdd");
            dateCode += date;
            return dateCode;
        }

        //Generates a sequential random Number to be the newly created Account Number of the user
        private async Task<string> AccountNumGen()
        {
            var AcctNum = $"{PalindromeCode()}{DateCode()}";
            try
            {
                var searchAccNum = _context.Accounts.Any(x => x.AccountNumber == AcctNum);
                while (searchAccNum)
                {
                    AcctNum = $"{PalindromeCode()}{DateCode}";
                }

                return AcctNum;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message} ||| {ex.StackTrace}");
            }
            return AcctNum;

        }

        //Generate an Email Token
        public string GenerateEmailToken()
        {
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return token;
        }

        //Method to Hash the password of a User for Security Purposes
        public void CreatePasswordHash(string password,
            out byte[] passwordHash,
            out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password,
            byte[] passwordHash,
            byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }   
    }
}
