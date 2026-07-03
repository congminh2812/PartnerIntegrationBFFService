using Microsoft.AspNetCore.Mvc;
using PartnerIntegrationBFFService.API.Controllers.Mock;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class PartnerVerificationMockControllerTests
    {
        [Fact]
        public async Task Verify_AtLeastOneCallReturnsOk_WhenCalledMultipleTimes()
        {
            var controller = new PartnerVerificationController();

            var observedOk = false;
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    var result = await controller.Verify($"p-{i}");
                    if (result is OkObjectResult ok)
                    {
                        observedOk = true;
                        Assert.NotNull(ok.Value);
                        break;
                    }
                }
                catch (TimeoutException)
                {
                    // ignored - controller randomly simulates timeout; continue trying
                }
            }

            Assert.True(observedOk, "Expected at least one successful Ok response from the mock controller within 20 attempts.");
        }

        [Fact]
        public async Task Verify_AtLeastOneCallThrowsTimeout_WhenCalledMultipleTimes()
        {
            var controller = new PartnerVerificationController();

            var observedTimeout = false;
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var _ = await controller.Verify($"p-{i}");
                }
                catch (TimeoutException)
                {
                    observedTimeout = true;
                    break;
                }
            }

            Assert.True(observedTimeout, "Expected the mock controller to throw a TimeoutException at least once within 100 attempts.");
        }
    }
}