﻿using FluentValidation;
using FluentValidation.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.ViewModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public static UserDataEntity user = new UserDataEntity();
        private readonly VanapayDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;


        public UserController(VanapayDbContext context, IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _context = context;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            UserRegisterRequest request, 
            [FromServices] IValidator<UserRegisterRequest> validator)
        {
            FluentValidation.Results.ValidationResult validationResult = validator.Validate(request);

            if(!validationResult.IsValid)
            {
                var modelStateDictionary = new ModelStateDictionary();

                foreach (FluentValidation.Results.ValidationFailure failure in validationResult.Errors)
                {
                    modelStateDictionary.AddModelError(
                        failure.PropertyName,
                        failure.ErrorMessage);
                }

                return ValidationProblem(modelStateDictionary);
            }

            var res = await _userService.Register(request);
            return Ok(res);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var res = await _userService.DeleteUser(id);
            return Ok(res);
        }

        
    }
}
