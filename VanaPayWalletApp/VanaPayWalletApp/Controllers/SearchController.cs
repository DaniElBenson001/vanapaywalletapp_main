using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Services.IServices;

namespace VanaPayWalletApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly VanapayDbContext _context;
        private readonly ISearchService _searchService;

        public SearchController(VanapayDbContext context, ISearchService searchService)
        {
            _searchService = searchService;
            _context = context;
        }

        [HttpPost("SearchUserviaAcc")]
        public async Task<IActionResult> SearchViaAccNo(SearchInputDto acc)
        {
            var res = await _searchService.SearchUserByAccNo(acc);
            return Ok(res);
        }

        [HttpPost("SearchUser")]
        public async Task<IActionResult> SearchUser(RegularSearchDto search)
        {
            var res = await _searchService.SearchUser(search);
            return Ok(res);
        }
    }
}
