using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegrationBFFService.API.Models
{
    public class CustomValidationProbemDetails : ProblemDetails
    {
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}

