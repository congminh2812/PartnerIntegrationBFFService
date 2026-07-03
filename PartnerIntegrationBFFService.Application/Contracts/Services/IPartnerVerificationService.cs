namespace PartnerIntegrationBFFService.Application.Contracts.Services
{
    public interface IPartnerVerificationService
    {
        Task<bool> VerifyPartnerAsync(string partnerId);
    }
}
