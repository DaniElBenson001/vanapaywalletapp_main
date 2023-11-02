using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly VanapayDbContext _context;
        public PaymentController(IPaymentService paymentService, VanapayDbContext context)
        {
            _paymentService = paymentService;
            _context = context;

        }

        [HttpPost("initializePayment"), Authorize]
        public async Task<IActionResult> InitializePayment(DepositDto deposit)
        {
            var res = await _paymentService.InitializePayment(deposit);
            return Ok(res);
        }
    }
}
