using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;

namespace PartnerIntegrationBFFService.API.Controllers.V1
{
    [ApiVersion("1.0")]
    public class PartnerController(ISender sender) : BaseController
    {
        [HttpPost("transactions")]
        public async Task<IActionResult> Transactions([FromBody] PartnerTransactionsCommand command)
        {
            var result = await sender.Send(command);
            return Ok(result);
        }
    }
}
