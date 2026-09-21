using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.Application.Dto.Request;
using Nova.Application.Interfaces;
using NovaPay.Controllers.NovaPay.Controllers;

namespace NovaPay.Controllers
{
    [ApiController]
    [Route("api/v1/wallets")]
    [Authorize]
    public class WalletController : BaseController
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }



        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var response = await _walletService
                .GetWalletAsync(CustomerId);

            return Ok(response);
        }

       
        [HttpGet("{walletId:guid}/statement")]
        public async Task<IActionResult> GetWalletStatement(
            Guid walletId,
            [FromQuery] WalletStatementRequest request, [FromHeader(Name = "X-Trace-Id")] string TraceId)
        {
            var response = await _walletService
                .GetWalletStatementAsync(
                    walletId,
                    CustomerId,
                    request);

            return Ok(response);
        }

        
        [HttpGet("{walletId:guid}/daily-limit")]
        public async Task<IActionResult> GetDailyLimitStatus(
            Guid walletId, [FromHeader(Name = "X-Trace-Id")] string TraceId)
        {
            var response = await _walletService
                .GetDailyLimitStatusAsync(
                    walletId,
                    CustomerId);

            return Ok(response);
        }










    }
}
