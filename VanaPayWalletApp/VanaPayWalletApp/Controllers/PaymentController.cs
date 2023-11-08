using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.DtoModels.Webhook;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly VanapayDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, VanapayDbContext context, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("initializePayment"), Authorize]
        public async Task<IActionResult> InitializePayment(DepositDto deposit)
        {
            var res = await _paymentService.InitializePayment(deposit);
            return Ok(res);
        }

        [HttpPost("paystackWebHook")]
        public async Task<IActionResult> PaystackWebHook(object obj)
        {
            var webHookEvent = JsonConvert.DeserializeObject<WebHookDto>(obj.ToString()!);

            var secKey = _configuration.GetSection("PaystackPayment:Secret_Key").Value!;
            String result = "";

            var reqHeader = HttpContext.Request.Headers;

            var reqBody = new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secKey);

            byte[] inputBytes = Encoding.UTF8.GetBytes(obj.ToString()!);
            using(var hmac = new HMACSHA512(secretKeyBytes))
            {
                //Signature Validation
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                result = BitConverter.ToString(hashValue).Replace("-", string.Empty);

                Console.WriteLine(result);

                reqHeader.TryGetValue("x-paystack-signature", out StringValues xpaystackSignature);

                if (!result.ToLower().Equals(xpaystackSignature))
                {
                    return BadRequest();
                }

                await _paymentService.PaymentWebHook(webHookEvent!);
                return Ok();
            }
        }
    }
}
