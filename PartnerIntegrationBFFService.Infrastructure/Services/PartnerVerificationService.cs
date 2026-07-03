using PartnerIntegrationBFFService.Application.Contracts.Services;

namespace PartnerIntegrationBFFService.Infrastructure.Services
{
    public class PartnerVerificationService(HttpClient httpClient) : IPartnerVerificationService
    {
        public async Task<bool> VerifyPartnerAsync(string partnerId)
        {
            var response = await httpClient.GetAsync($"/api/mock/PartnerVerification/{partnerId}");

            return response.IsSuccessStatusCode;
        }
    }
}
