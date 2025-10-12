using System;
using Xunit;
using Csp.Api.Services;

namespace Csp.Api.Tests
{
    public class FineCalculationServiceTests
    {
        [Theory]
        [InlineData("2024-04-01", "2024-04-01", 0.25, 0)]
        [InlineData("2024-04-01", "2024-04-02", 0.25, 0.25)]
        [InlineData("2024-04-01", "2024-04-05", 0.25, 1.0)]
        [InlineData("2024-04-01", "2024-03-31", 0.25, 0)]
        [InlineData("2024-04-01", "2024-04-10", 1.0, 9.0)]
        public void ComputeFineAmount_CorrectlyCalculatesFine(string dueDateStr, string nowStr, decimal dailyRate, decimal expected)
        {
            var dueDate = DateTime.Parse(dueDateStr).ToUniversalTime();
            var now = DateTime.Parse(nowStr).ToUniversalTime();
            var fine = IFineCalculationService.ComputeFineAmount(dueDate, now, dailyRate);
            Assert.Equal(expected, fine);
        }
    }
}
