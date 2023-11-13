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
        private readonly VanapayDbContext _context;
        private readonly ITransactionService _transactionService;

        public TransactionController(VanapayDbContext context, ITransactionService transactionService)
        {
            _transactionService = transactionService;
            _context = context;
        }

        [HttpGet("getTransactionHistoryAsAdmin"), Authorize]
        public async Task<IActionResult> GetTransactionHistoryAsAdmin()
        {
            var res = await _transactionService.GetTransactionHistoryAsAdmin();
            return Ok(res);
        }

        [HttpGet("getTransactionHistoryAsUser"), Authorize]
        public async Task<IActionResult> GetTransactionHistoryAsUser()
        {
            var res = await _transactionService.GetTransactionHistoryAsUser();
            return Ok(res);
        }

        [HttpGet("getRecentTransactions"), Authorize]
        public async Task<IActionResult> GetRecentTransactions()
        {
            var res = await _transactionService.GetThreeMostRecentTransactions();
            return Ok(res);
        }

        [HttpPost("getTransacHistoryByDate"), Authorize]
        public async Task<IActionResult> GetTransacHistoryByDate(DateDto date)
        {
            var res = await _transactionService.GetTransacHistoryByDate(date);
            return Ok(res);
        }

        [HttpGet("getTransacHistoryToday"), Authorize]
        public async Task<IActionResult> GetTransacHistoryByDay()
        {
            var res = await _transactionService.GetTxnHistoryToday();
            return Ok(res);
        }

        [HttpGet("getTransacHistoryYesterday"), Authorize]
        public async Task<IActionResult> GetTxnHistoryYesterday()
        {
            var res = await _transactionService.GetTxnHistoryYesterday();
            return Ok(res);
        }

        [HttpGet("getTransacHistoryThreeDaysAgo"), Authorize]
        public async Task<IActionResult> GetTxnHistoryThreeDaysAgo()
        {
            var res = await _transactionService.GetTxnHistoryThreeDaysAgo();
            return Ok(res);
        }

        [HttpGet("getTransacHistorySevenDaysAgo"), Authorize]
        public async Task<IActionResult> GetTxnHistorySevenDaysAgo()
        {
            var res = await _transactionService.GetTxnHistorySevenDaysAgo();
            return Ok(res);
        }

        [HttpGet("getTransacHistoryOneMonthAgo"), Authorize]
        public async Task<IActionResult> GetTxnHistoryOneMonthAgo()
        {
            var res = await _transactionService.GetTxnHistoryOneMonthAgo();
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
