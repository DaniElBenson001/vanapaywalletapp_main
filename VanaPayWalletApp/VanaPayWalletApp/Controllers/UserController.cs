using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        private readonly VanaPayDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserService _user;


        public UserController(VanaPayDbContext context, IConfiguration configuration, IUserService user)
        {
            _configuration = configuration;
            _context = context;
            _user = user;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            var res = await _user.Register(request);
            return Ok(res);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var res = await _user.Login(request);
            return Ok(res);

        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var res = await _user.DeleteUser(id);
            return Ok(res);
        }

        
    }
}
