﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NPOI.OpenXmlFormats.Dml;
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
    public class AuthService : IAuthService
    {
        private readonly VanapayDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthService(VanapayDbContext context, ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        //Method to Log in a User
        public async Task<DataResponse<LoginViewModel>> Login(UserLoginRequest request)
        {
            //Creating an instance of the Generic Class "DataResponse" holding a generic type parameter "LoginViewModel"
            var response = new DataResponse<LoginViewModel>();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName || u.Email == request.Email);

                //Condition checks if the user trying to Log in exists, else returns "User Not Found"
                if (user == null)
                {
                    throw new Exception("User Not Found");
                }

                //Condition checks if the user trying to Log in is imputting the right password, else returns "Username/Password is Incorrect"
                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    throw new Exception("Your Password or Username is Incorrect");
                }

                //This initializes a Verification Token for the User to Authenticate and Validate the User upon Log in
                string token = CreateToken(user);
                user.VerificationToken = token;
                await _context.SaveChangesAsync();

                //Sets the Value for the LoginViewModel to Hold for Authentication Purposes 
                var loginData = new LoginViewModel()
                {
                    UserName = request.UserName,
                    VerificationToken = token,
                };
                response.Data = loginData;
            }
            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                return response;
            }

            return response;
        }

        public async Task<DataResponse<string>> ValidateUser()
        {
            var validationResponse = new DataResponse<string>();

            try
            {
                int userID;

                //Condition to check if the HttpContextAccessor does not contain any tangible value, sending the appropriate pin response
                if (_httpContextAccessor.HttpContext == null)
                {
                    validationResponse.Status = false;
                    validationResponse.StatusMessage = $"User does not Exist";
                }

                if( _httpContextAccessor.HttpContext != null)
                {
                    //Handpicks the user Id embedded in the Claims and converts the value to Integers
                    userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                    //Finds and Handpicks the first or default value that matches the comparism in value with the userID
                    var user = await _context.Users.Where(v => v.Id == userID).FirstOrDefaultAsync();

                    if (user == null)
                    {
                        validationResponse.Status = false;
                        validationResponse.StatusMessage = "No Pin is Created Yet";
                    }

                    if(user != null)
                    {
                        throw new Exception("Already Has a Pin");
                    }
                }
            }
            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                validationResponse.Status = false;
                validationResponse.StatusMessage = ex.Message;
                validationResponse.Data = $"It happened at {DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}";
                return validationResponse;
            }
            return validationResponse;
        }


        //Method to Create a New Pin
        public async Task<DataResponse<string>> CreatePin(PinCreationDto pin)
        {
            //Creating an instance of the Generic Class "DataResponse" holding a data type string
            var pinResponse = new DataResponse<string>();
                
            try
            {
                int userID;
                
                //Condition to check if the HttpContextAccessor does not contain any tangible value, sending the appropriate pin response
                if(_httpContextAccessor == null)
                {
                    pinResponse.Status = false;
                    pinResponse.StatusMessage = $"User does not Exist";
                }

                //Condition to check if the HttpContextAccessor does contain any tangible value, receiving the pin provided and then Hashing the Pin, finally sending the appropriate pin response
                if (_httpContextAccessor != null)
                {
                    //Handpicks the user Id embedded in the Claims and converts the value to Integers
                    userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                    //Finds and Handpicks the first or default value that matches the comparism in value with the userID
                    var user = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();

                    //Condition to check if the user variable contains tangible Value
                    if (user != null)
                    {
                        //Generates a Hashed and Salted PIN for the PIN provided
                        CreatePinHash(pin.UserPin,
                            out byte[] pinSalt,
                            out byte[] pinHash);

                        //Fills in the Fields with the Appropriate Database using the user variable which uses the DbContext asynchronously, saving the changes therein
                        user.PinHash = pinHash;
                        user.PinSalt = pinSalt;
                        user.PinCreatedAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();

                        //Returns the Pin Response for the API to send upon request
                        pinResponse.Status = true;
                        pinResponse.StatusMessage = "Pin Successfully Created";

                        return pinResponse;
                    }

                }
            return pinResponse;
            }
            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                pinResponse.Status = false;
                pinResponse.StatusMessage = ex.Message;
                pinResponse.Data = $"It happened at {DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}";
                return pinResponse;
            }
        }

        //Method to Verify a Given Pin
        public async Task<DataResponse<string>> VerifyPin(PinVerificationDto pin)
        {
            //Creating an instance of the Generic Class "DataResponse" holding a data type string
            var pinResponse = new DataResponse<string>();
            try
            {
                int userID;

                //Condition to check if the HttpContextAccessor does contain any tangible value, receiving the pin provided and then Hashing the Pin, finally sending the appropriate pin response
                if (_httpContextAccessor == null)
                {
                    pinResponse.Status = false;
                    pinResponse.StatusMessage = "USER NOT FOUND";
                }

                //Condition to check if the HttpContextAccessor does contain any tangible value, receiving the pin provided and compares and validate it with the provided pin in the Database
                if (_httpContextAccessor != null)
                {
                    //Handpicks the user Id embedded in the Claims and converts the value to Integers
                    userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

                    //Finds and Handpicks the first or default value that matches the comparism in value with the userID
                    var user = await _context.Users.Where(p => p.Id == userID).FirstAsync();

                    //Condition to check if the user variable does not contain tangible Value
                    if (user == null)
                    {
                        pinResponse.Status = false;
                        pinResponse.StatusMessage = "USER NOT FOUND";
                    }

                    //Condition to check if the user variable contains tangible Value and verifies the provided PIN for 
                    if (user != null)
                    {
                        if (!VerifyPinHash(pin.pin, user!.PinHash, user.PinSalt))
                        {
                            throw new Exception("Pin is Incorrect");
                        }

                        //Returns the Pin Response for the API to send upon request
                        pinResponse.Status = true;
                        pinResponse.StatusMessage = "Pin Successfully Verified";
                    }
                }
            }
            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                pinResponse.Status = false;
                pinResponse.StatusMessage = ex.Message;
                return pinResponse;
            }
            return pinResponse;

        }

        //Method to edit an Existing Pin
        public async Task<DataResponse<string>> ChangePin(PinChangeDto pin)
        {
            var pinResponse = new DataResponse<string>();
            try
            {
                int userID;

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Where(p => p.Id == userID).FirstAsync();

                if(user == null)
                {
                    pinResponse.Status = false;
                    pinResponse.StatusMessage = "USER NOT FOUND";
                }

                if(user != null)
                {
                    CreatePinHash(pin.NewPin,
                            out byte[] pinSalt,
                            out byte[] pinHash);

                    user.PinHash = pinHash;
                    user.PinSalt = pinSalt;
                    user.PinModifiedAt = DateTime.UtcNow;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    pinResponse.Status = true;
                    pinResponse.StatusMessage = "Pin Successfully Updated";
                }
            }
            //Catchs any unforeseen circumstance and returns an error stating the message the problem backing it and time and date accompanied therein 
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                pinResponse.Status = false;
                pinResponse.StatusMessage = ex.Message;
                return pinResponse;
            }
            return pinResponse;
        }

        //Method to Verify the Password Hashed upon Login
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

        //Method to Generate a Token(A random string of characters) for User Authentication and Verification Login
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



        //Method to Hash the Transaction Pin of the Account User
        private void CreatePinHash(string pin, out byte[] pinSalt, out byte[] pinHash)
            {
                using (var hmac = new HMACSHA512())
                {
                    pinSalt = hmac.Key;
                    pinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin.ToString()!));
                }
            }

        private bool VerifyPinHash(string pin,
            byte[] pinHash,
            byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512(pinSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
                return computedHash.SequenceEqual(pinHash);
            }
        }
    }
}
