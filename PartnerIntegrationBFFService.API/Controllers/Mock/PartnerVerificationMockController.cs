using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegrationBFFService.API.Controllers.Mock
{
    [ApiController]
    [Route("api/mock/[controller]")]
    public class PartnerVerificationController : ControllerBase
    {
        private static readonly Random _random = new();

        [HttpGet("{partnerId}")]
        public async Task<IActionResult> Verify(string partnerId)
        {
            if (_random.NextDouble() < 0.3)
                throw new TimeoutException($"Simulated timeout for partnerId: {partnerId}");

            return Ok(new { PartnerId = partnerId, IsVerified = true });
        }
    }
}
