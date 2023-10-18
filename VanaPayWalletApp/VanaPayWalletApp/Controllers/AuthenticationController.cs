using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly VanapayDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthenticationController(VanapayDbContext context, IConfiguration configuration, IAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var res = await _authService.Login(request);
            return Ok(res);

        }

        [HttpPost("createPin"), Authorize]
        public async Task<IActionResult> CreatePin(PinCreationDto pin)
        {
            var res = await _authService.CreatePin(pin);
            return Ok(res);
        }

        [HttpPost("verifyPin"), Authorize]
        public async Task<IActionResult> VerifyPin(PinVerificationDto pin)
        {
            var res = await _authService.VerifyPin(pin);
            return Ok(res);
        }

        [HttpPut("changePin"), Authorize]
        public async Task<IActionResult> ChangePin(PinChangeDto pin)
        {
            var res = await _authService.ChangePin(pin);
            return Ok(res);
        }

        [HttpGet("validateUser"), Authorize]
        public async Task<IActionResult> ValidateUser()
        {
            var res = await _authService.ValidateUser();
            return Ok(res);
        }
    }
}
