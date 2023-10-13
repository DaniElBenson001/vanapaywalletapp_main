using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        //private readonly IMailService _mailService;

        //Constructor for the User Service
        public UserService(VanapayDbContext context, IConfiguration configuration, ILogger<UserService> logger)
        {
            _context = context;                 //Parameter that grant access to data in the Database Server
            _configuration = configuration;     //Parameter for Configuration Settings
            _logger = logger;                   //Parameter for Application Logging Operations
            //_mailService = mailService;
        }

        //Method to Register a New User
        public async Task<RegisterViewModel> Register(UserRegisterRequest request)
        {
            //Instance Of the RegisterViewModel Class - Basically an Object
            RegisterViewModel registerResponse = new RegisterViewModel();
            try
            {
                //Initializes the Password Hash of the Password Given by the User
                CreatePasswordHash(request.Password,
                    out byte[] passwordHash,
                    out byte[] passwordSalt);
                
                //Variable initialized to create an instance of the UserDataEntity - Use the Peek Definition to check the UserDataEntity Model Class
                var user = new UserDataEntity
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    UserName = request.UserName,
                    Address = request.Address,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                };

                //
                var data = await _context.Users.AnyAsync(u => u.Email == request.Email);
                if (data)
                {
                    registerResponse.Status = false;
                    registerResponse.StatusMessage = "User Already Exists";
                    return registerResponse;
                }

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var userAccount = new AccountDataEntity
                {
                    AccountNumber = AccountNumGen(),
                    Balance = 10000,
                    Currency = "NGN",
                    AccountId = user.Id,

                };

                var AccountData = await _context.Accounts.AnyAsync(u => u.AccountNumber == userAccount.AccountNumber);
                if (AccountData)
                {
                    registerResponse.Status = false;
                    registerResponse.StatusMessage = "Account User Already Exists";
                    return registerResponse;
                }

                await _context.Accounts.AddAsync(userAccount);
                await _context.SaveChangesAsync();

                registerResponse.Status = true;
                registerResponse.StatusMessage = "User Successfully Created";
                return registerResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                return registerResponse;
                
            }
        }

        public async Task<DataResponse<LoginViewModel>> Login(UserLoginRequest request)
        {
            var response = new DataResponse<LoginViewModel>();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName || u.Email == request.Email);

                if (user == null)
                {
                    throw new Exception("User Not Found");
                }

                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    throw new Exception("Username/Password is Incorrect!");
                }

                string token = CreateToken(user);
                user.VerificationToken = token;
                await _context.SaveChangesAsync();

                var loginData = new LoginViewModel()
                {
                    UserName = request.UserName,
                    VerificationToken = token,
                };
                response.Data = loginData;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                return response;
            }

            return response;
        }

        public async Task<UserDataEntity?> DeleteUser(int id)
        {
            var response = new UserDataEntity();
            var accountResponse = new AccountDataEntity();

            if(_context.Users == null)
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

        //A Palindrome number of three figures will be added to the beginning of the 
        private static string PalindromeCode()
        {
            var firstFigure = new Random().Next(1, 9);
            var secondFigure = new Random().Next(0, 9);
            var palindromeNum = $"{firstFigure}{secondFigure}{firstFigure}";
            return palindromeNum;
        }

        //Generates a sequential random Number to be the newly created Account Number of the user
        private static string AccountNumGen()
        {
            var AcctNum= $"{PalindromeCode()}{new Random().Next(1111, 9999)}{PalindromeCode()}";
            return AcctNum.ToString();
        }

        //Generate an Email Token
        public string GenerateEmailToken()
        {
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return token;
        }

        private string CreateToken(UserDataEntity user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        //public async Task<DataResponse<string>> VerifyEmail(string verifyEmail)
        //{
        //    var response = new DataResponse<string>();
        //    var emailUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == verifyEmail);
        //    try
        //    {
        //        //Strengthening the Token Value by three [As taught by a Senior Colleague]
        //        var token = GenerateEmailToken();
        //        var encodedToken = Encoding.UTF8.GetBytes(token);
        //        var validToken = WebEncoders.Base64UrlEncode(encodedToken);

        //        //Writing the content in the mail and also redirecting the user back to the Login after Verification
        //        string url = $"{_configuration.GetSection("Links:LoginUrl").Value!}?token={validToken}";
        //        await _mailService.VerifyEmailMessage(verifyEmail, "EMAIL VERIFICATION", "<h1> Your Email has been Verified </h1>",
        //            $"<p><a href = {url}> Proceed to Log In </a></p>" +
        //            "<br><b> DO HAVE A VANAFUL DAY! </b>");

        //        emailUser!.VerificationToken = validToken;
        //        emailUser.VerifiedAt = DateTime.Now;



        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
        //        _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");

        //        response.Status = false;
        //        response.StatusMessage = ex.Message;
        //    }
        //    return response;
        //}

        /*[HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return BadRequest("Invalid Token");
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("User Verified");
        }*/

        /*private static string CreateAccountNumber()
        {
            var accountNum = "";

        }*/

        /*private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }*/

        private void CreatePasswordHash(string password,
            out byte[] passwordHash,
            out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password,
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
