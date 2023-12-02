using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
        //private readonly IMailService _mailService;

        public UserService(VanapayDbContext context, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

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

        public async Task<DataResponse<string>> UpdateUserDetails(UserDetailsDto userInfo)
        {
            var userResponse = new DataResponse<string>();

            int userID;

            userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.Where(p => p.Id == userID).FirstAsync();

            try
            {
                if(user == null)
                {
                    userResponse.status = false;
                    userResponse.statusMessage = "USER NOT FOUND";
                    return userResponse;
                }

                if (userInfo.firstName == string.Empty)
                {
                    userInfo.firstName = user.FirstName;
                }

                if(userInfo.lastName == string.Empty)
                {
                    userInfo.lastName = user.LastName;
                }

                if(userInfo.address == string.Empty)
                {
                    userInfo.address = user.Address;
                }

                user.FirstName = userInfo.firstName;
                user.LastName = userInfo.lastName;
                user.Address = userInfo.address;
                user.UserModifiedAt = DateTime.Now;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                userResponse.status = true;
                userResponse.statusMessage = "Updated Successfully";
            }
            catch(Exception ex)
            {
                _logger.LogError($" {ex.Message} ||| {ex.StackTrace} ");
                userResponse.status = false;
                userResponse.statusMessage = ex.Message;
                return userResponse;
            }
            return userResponse;
        }

        public async Task<DataResponse<string>> CreatePin(PinCreationDto pin)
        {
            //Creating an instance of the Generic Class "DataResponse" holding a data type string
            var pinResponse = new DataResponse<string>();

            try
            {
                int userID;

                //Condition to check if the HttpContextAccessor does not contain any tangible value, sending the appropriate pin response
                if (_httpContextAccessor == null)
                {
                    pinResponse.status = false;
                    pinResponse.statusMessage = $"User does not Exist";
                    return pinResponse;
                }

                //Handpicks the user Id embedded in the Claims and converts the value to Integers
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                //Finds and Handpicks the first or default value that matches the comparism in value with the userID
                var user = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();

                //Generates a Hashed and Salted PIN for the PIN provided
                CreatePinHash(pin.UserPin,
                    out byte[] pinSalt,
                    out byte[] pinHash);

                //Fills in the Fields with the Appropriate Database using the user variable which uses the DbContext asynchronously, saving the changes therein
                user!.PinHash = pinHash;
                user.PinSalt = pinSalt;
                user.PinCreatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                //Returns the Pin Response for the API to send upon request
                pinResponse.status = true;
                pinResponse.statusMessage = "Pin Successfully Created";

                return pinResponse;
            }
            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} ||| {ex.StackTrace}");
                pinResponse.status = false;
                pinResponse.statusMessage = ex.Message;
                return pinResponse;
            }
        }

        public async Task<DataResponse<string>> ChangePin(PinChangeDto pin)
        {
            var pinResponse = new DataResponse<string>();
            try
            {
                int userID;

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Where(p => p.Id == userID).FirstAsync();

                if (user == null)
                {
                    pinResponse.status = false;
                    pinResponse.statusMessage = "USER NOT FOUND";
                    return pinResponse;
                }

                if (!VerifyPinHash(pin.OldPin, user.PinHash!, user.PinSalt!))
                {
                    pinResponse.status = false;
                    pinResponse.statusMessage = "Your Old PIN is not Correct";
                    return pinResponse;
                }

                CreatePinHash(pin.NewPin,
                out byte[] pinSalt,
                out byte[] pinHash);

                user.PinHash = pinHash;
                user.PinSalt = pinSalt;
                user.PinModifiedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                pinResponse.status = true;
                pinResponse.statusMessage = "Pin Successfully Updated";
            }
            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($" {ex.Message} ||| {ex.StackTrace} ");
                pinResponse.status = false;
                pinResponse.statusMessage = ex.Message;
                return pinResponse;
            }
            return pinResponse;
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
                user.PasswordModifiedAt = DateTime.Now;

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

        public async Task<DataResponse<string>> AddSecurityQuestion(SecurityQuestionDto result)
        {
            var securQuestionResponse = new DataResponse<string>();
            try
            {
                int userID;

                if (_httpContextAccessor == null)
                {
                    securQuestionResponse.status = false;
                    securQuestionResponse.statusMessage = $"User does not Exist";
                    return securQuestionResponse;
                }

                if(result.answer ==  null || result.answer == "")
                {
                    securQuestionResponse.status = true;
                    securQuestionResponse.statusMessage = "Kindly send in your Answers";
                    return securQuestionResponse;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                var user = await _context.Users.Where(s => s.Id == userID).FirstOrDefaultAsync();

                CreateSecurityQuestionHash(result.answer!,
                    out byte[] answerHash,
                    out byte[] answerSalt);

                var data = new SecurityQuestionDataEntity()
                {
                    Question = result.question,
                    Answer = answerSalt,
                    UserId = userID
                };

                //if (data.UserId == user!.Id)
                //{
                //    securQuestionResponse.status = false;
                //    securQuestionResponse.statusMessage = "Security Question Added Already";
                //    return securQuestionResponse;
                //}

                await _context.SecurityQuestions.AddAsync(data);
                await _context.SaveChangesAsync();

                securQuestionResponse.status = true;
                securQuestionResponse.statusMessage = "Information Added Successfully";
            }

            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($" {ex.Message}  |||  {ex.StackTrace} " +
                    $"");
                securQuestionResponse.status = false;
                securQuestionResponse.statusMessage = ex.Message;
                return securQuestionResponse;
            }
            return securQuestionResponse;
        }

        public async Task<DataResponse<string>> SecurityQuestionAvailability()
        {
            var availabilityResponse = new DataResponse<string>();
            
            try
            {
                int userID;

                if(_httpContextAccessor.HttpContext == null)
                {
                    availabilityResponse.status = false;
                    availabilityResponse.statusMessage = "User Not Found";
                    return availabilityResponse;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                var userQuestion = await _context.SecurityQuestions.Where(x => x.UserId == userID).FirstOrDefaultAsync();

                if(userQuestion!.UserId == null)
                {
                    availabilityResponse.status = false;
                    availabilityResponse.statusMessage = "No Question added yet";
                    return availabilityResponse;
                }
                if(userQuestion.UserId != null)
                {
                    availabilityResponse.status = true;
                    availabilityResponse.statusMessage = "Question provided already";
                    return availabilityResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" {ex.Message} ||| {ex.StackTrace} ");
                availabilityResponse.status = false;
                availabilityResponse.statusMessage = ex.Message;
                return availabilityResponse;
            }
            return availabilityResponse;
        }

        public async Task<DataResponse<string>> GetSecurityQuestion()
        {
            string question = SecurityQuestionRandomizer();
            var questionResponse = new DataResponse<string>();
            try
            {
                questionResponse.status = true;
                questionResponse.statusMessage = "Question Gotten Successfully";
                questionResponse.data = question;
                return questionResponse;
            }
            catch(Exception ex)
            {
                _logger.LogError($" {ex.Message} ||| {ex.StackTrace} ");
                questionResponse.status = false;
                questionResponse.statusMessage = ex.Message;
                return questionResponse;
            }
           
        }

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

        public static string SecurityQuestionRandomizer()
        {
            Random random = new Random();
            var securityQuestions = new[]
            {
                "What is your Mother's Maiden Name",
                "What was the Name of your First Pet?",
                "In which City were you Born?",
                "What is your Favorite Book?",
                "What is your Favorite Movie?",
                "Who was your Childhood Best Friend?",
                "What is the Name of your Favorite Teacher?",
                "What is the Model of your First Car?",
                "What is your Favorite Sports team?",
                "What is your Favorite Color?",
                "What is the Name of the Street you grew up on?",
                "What is your Favorite Food?",
                "What is the Name of your First School?",
                "Who is your Favorite Historical Figure?",
                "What is your Favorite Vacation Spot?",
                "What is the Make of your First Computer?",
                "What is your Favorite Music Band or Artist?",
                "What is your Father's Middle Name?",
                "What is your Favorite Childhood Game?",
                "What is the Name of your Significant Other?",
                "What is the name of your first pet?",
                "Which city were you born in?",
                "What is your oldest sibling's middle name?",
                "What was the make and model of your first car?",
                "In what city did you meet your significant other?",
                "What is the name of your favorite childhood teacher?",
                "What is the first name of your oldest cousin?",
                "What street did you grow up on?",
                "What was the name of your childhood best friend?",
                "Where did you go for your first international trip?",
                "What is the name of your favorite fictional character?",
                "What was the name of your first school?",
                "What is your maternal grandmother's maiden name?",
                "What was the name of the company of your first job?",
                "What is the name of your favorite childhood book?",
                "What is your favorite movie of all time?",
                "What was the model of your first mobile phone?",
                "In which city were you when you had your first kiss?",
                "What was the name of the street you lived on in third grade?",
                "What is your favorite dish to cook?" 
            };

            string questionString = "";
            int n = random.Next(1, securityQuestions.Length);

            questionString += securityQuestions[n];
            return questionString;
        }

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


        public string GenerateEmailToken()
        {
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return token;
        }

        //Method to Hash the Transaction Pin of the Account User
        private void CreatePinHash(string pin,
            out byte[] pinSalt,
            out byte[] pinHash)
        {
            using (var hmac = new HMACSHA512())
            {
                pinSalt = hmac.Key;
                pinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin.ToString()!));
            }
        }

        public bool VerifyPinHash(string pin,
            byte[] pinHash,
            byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512(pinSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
                return computedHash.SequenceEqual(pinHash);
            }
        }

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

        public void CreateSecurityQuestionHash(string answer,
            out byte[] answerHash,
            out byte[] answerSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                answerSalt = hmac.Key;
                answerHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(answer));
            }
        }

        public bool VerifySecurityQuestionHash(string answer,
            byte[] answerHash,
            byte[] answerSalt)
        {
            using (var hmac = new HMACSHA512(answerSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(answer));
                return computedHash.SequenceEqual(answerHash);
            }
        }
    }
}
