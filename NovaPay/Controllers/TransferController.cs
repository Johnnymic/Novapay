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
    public class TransferController : BaseController
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

       

        

      

        [HttpPost("{walletId:guid}/credit")]
        public async Task<IActionResult> CreditWallet(
            Guid walletId,
            [FromBody] CreditWalletRequest request,
            [FromHeader(Name = "Idempotency-Key")]
            string idempotencyKey, [FromHeader(Name = "X-Trace-Id")] string TraceId)
        {


            

            var response = await _transferService.CreditAsync(
                walletId,
                request,
                CustomerId,
                idempotencyKey,
                TraceId);

            return Ok(response);
        }

        [HttpPost("{walletId:guid}/transfer")]
        public async Task<IActionResult> Transfer(
            Guid walletId,
            [FromBody] TransferWalletRequest request,
            [FromHeader(Name = "Idempotency-Key")]
            string idempotencyKey, [FromHeader(Name = "X-Trace-Id")] string TraceId)
        {
          

            var response = await _transferService.TransferAsync(
                walletId,
                request,
                CustomerId,
                idempotencyKey,
                TraceId);

            return Ok(response);
        }

        
      
    }
}
