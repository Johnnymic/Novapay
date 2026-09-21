using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.Application.Dto.Request;
using Nova.Application.Interfaces;
using NovaPay.Controllers.NovaPay.Controllers;
using System.Diagnostics;

namespace NovaPay.Controllers
{
   

    namespace NovaPay.Controllers
    {
        [ApiController]
        [Route("api/v1/admin/")]
        [Authorize(Roles = "Admin")]
        public class AdminController : BaseController
        {
            private readonly IAdminService _adminService;

            public AdminController(IAdminService adminService)
            {
                _adminService = adminService;
            }

           
            [HttpPost("{walletId:guid}/freeze")]
            public async Task<IActionResult> FreezeWallet(
                Guid walletId,
                [FromBody] FreezeWalletRequest request, [FromHeader(Name = "X-Trace-Id")] string TraceId)
            {
                var response = await _adminService.FreezeWalletAsync(
                    walletId,
                    request,
                    Actor,
                    TraceId);

                return Ok(response);
            }

            [HttpGet("view-customers")]
            public async Task<IActionResult> GetAllCustomer([FromHeader(Name = "X-Trace-Id")] string TraceId)
            {
                var response = await _adminService.GetAllCustomersWithWalletsAsync(TraceId);

                return Ok(response);
            }

            [HttpPost("{walletId:guid}/unfreeze")]
            public async Task<IActionResult> UnfreezeWallet(
                Guid walletId,
                [FromBody] UnfreezeWalletRequest request, [FromHeader(Name = "X-Trace-Id")] string TraceId)
            {
                var response = await _adminService.UnfreezeWalletAsync(
                    walletId,
                    request,
                    Actor,
                    TraceId);

                return Ok(response);
            }


          

         
        }
    }
}
