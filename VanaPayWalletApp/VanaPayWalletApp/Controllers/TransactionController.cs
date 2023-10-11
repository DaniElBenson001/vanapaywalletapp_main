using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly VanaPayDbContext _context;
        private readonly ITransactionService _transactionService;

        public TransactionController(VanaPayDbContext context, ITransactionService transactionService)
        {
            _transactionService = transactionService;
            _context = context;
        }

        [HttpGet("getTransactionHistory"), Authorize]
        public async Task<IActionResult> GetTransactionHistory()
        {
            var res = await _transactionService.GetTransactionHistory();
            return Ok(res);
        }

        [HttpPut("transfer"), Authorize]
        public async Task<IActionResult> MakeTransactionTransfer(TransactionDto transfer)
        {
            var res = await _transactionService.MakeTransactionTransfer(transfer);
            return Ok(res);
        }

        [HttpGet("getAllTransactions")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var res = await _transactionService.GetAllTransactions();
            return Ok(res);
        }
    }
}
