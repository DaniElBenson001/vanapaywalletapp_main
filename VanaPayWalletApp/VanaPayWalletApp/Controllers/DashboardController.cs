using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VanaPayWalletApp.Services.IServices;


namespace VanaPayWalletApp.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly VanaPayDbContext _context;
        //private readonly ILogger _logger;
        //private readonly IConfiguration _configuration;
        private readonly IDashboardService _dashboard;

        public DashboardController(VanaPayDbContext context, IDashboardService dashboard)
        {
           _context = context;
           _dashboard = dashboard;
        }

        [HttpGet("getDashboardInfo"), Authorize]
        public async Task<IActionResult> GetDashboardInfo()
        {
            var res = await _dashboard.GetDashboardInfo();
            return Ok(res);
        }
    }
}
