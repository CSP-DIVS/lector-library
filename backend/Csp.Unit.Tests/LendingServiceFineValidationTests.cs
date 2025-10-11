using Csp.Api.DTOs;
using Csp.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;
using Moq;

namespace Csp.Unit.Tests
{
    public class LendingServiceFineValidationTests
    {
        private static LendingService CreateService()
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=dummy;Uid=root;Pwd=pass;"
            };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            return new LendingService(cfg);
        }

        [Fact]
        public async Task AdjustFine_ReturnsError_WhenInvalidLendingId()
        {
            var svc = CreateService();
            var response = await svc.AdjustFineAsync(0, new AdjustFineRequest { NewAmount = 1, Reason = "r" }, 1);
            Assert.False(response.Success);
            Assert.Equal("Invalid lending id", response.Message);
        }

        [Fact]
        public async Task AdjustFine_ReturnsError_WhenNegativeAmount()
        {
            var svc = CreateService();
            var response = await svc.AdjustFineAsync(1, new AdjustFineRequest { NewAmount = -5, Reason = "r" }, 1);
            Assert.False(response.Success);
            Assert.Equal("New amount must be >= 0", response.Message);
        }

        [Fact]
        public async Task AdjustFine_ReturnsError_WhenReasonMissing()
        {
            var svc = CreateService();
            var response = await svc.AdjustFineAsync(1, new AdjustFineRequest { NewAmount = 0, Reason = "" }, 1);
            Assert.False(response.Success);
            Assert.Equal("A reason is required for all fine adjustments.", response.Message);
        }

        // Note: We don't test success paths here since those require DB integration; covered in integration tests.
    }
}
