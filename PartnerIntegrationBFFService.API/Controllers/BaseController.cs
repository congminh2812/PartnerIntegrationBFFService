using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegrationBFFService.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
    }
}
