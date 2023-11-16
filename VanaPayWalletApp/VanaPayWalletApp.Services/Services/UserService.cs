using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        //private readonly IMailService _mailService;

        //Constructor for the User Service
        public UserService(VanapayDbContext context, IConfiguration configuration, ILogger<UserService> logger)
        {
            _context = context;                 
            _configuration = configuration;     
            _logger = logger;                   
        }

        //The Register Method
        public async Task<ResponseViewModel> Register(UserRegisterRequest request)
        {
            ResponseViewModel registerResponse = new ResponseViewModel();

            try
            {
                CreatePasswordHash(request.Password,
                    out byte[] passwordHash,
                    out byte[] passwordSalt);

                var userData = new UserDataEntity
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    UserName = request.UserName,
                    Address = request.Address,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedAt = DateTime.Now
                };

                var userEmail = await _context.Users.AnyAsync(u => u.Email == request.Email);
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

        //Method to generate Security Question for a User
        //public async Task<DataResponse<string>> GetSecurityQuestion()
        //{
        //    DataResponse<string> securQuestionResponse = new();

        //    try
        //    {
        //        Random random = new Random();
        //        var securityQuestions = new[]
        //        {
        //            "What is your Mother's Maiden Name",
        //            "What was the Name of your First Pet?",
        //            "In which City were you Born?",
        //            "What is your Favorite Book?",
        //            "What is your Favorite Movie?",
        //            "Who was your Childhood Best Friend?",
        //            "What is the Name of your Favorite Teacher?",
        //            "What is the Model of your First Car?",
        //            "What is your Favorite Sports team?",
        //            "What is your Favorite Color?",
        //            "What is the Name of the Street you grew up on?",
        //            "What is your Favorite Food?",
        //            "What is the Name of your First School?",
        //            "Who is your Favorite Historical Figure?",
        //            "What is your Favorite Vacation Spot?",
        //            "What is the Make of your First Computer?",
        //            "What is your Favorite Music Band or Artist?",
        //            "What is your Father's Middle Name?",
        //            "What is your Favorite Childhood Game?",
        //            "What is the Name of your Significant Other?"
        //        };

        //        string questionString = "";
        //        int n = random.Next(1, securityQuestions.Length);
        //        questionString +=
        //    }
        //    Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"AN ERROR OCCURED.... => {ex.Message} ||| {ex.StackTrace}");
        //        return securQuestionResponse;
        //    }
        //    return securQuestionResponse;
        //}


        //A Miscellaneous Method to Delete a User that is not Needed, will be reburbished to be Lock/Unlocked Users for the Administrators to use
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

        private static string DateCode()
        {
            var dateCode = "";
            var date = DateTime.Now.ToString("yyyymdd");
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
    }
}
