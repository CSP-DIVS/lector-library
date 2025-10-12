using System;
using Xunit;
using Csp.Api.Services;

namespace Csp.Unit.Tests
{
    public class FineCalculationServiceUnitTests
    {
        [Theory]
        [InlineData("2025-10-10", "2025-10-10", 0.25, 0)]
        [InlineData("2025-10-10", "2025-10-11", 0.25, 0.25)]
        [InlineData("2025-10-10", "2025-10-15", 0.25, 1.25)]
        [InlineData("2025-10-10", "2025-10-09", 0.25, 0)]
        [InlineData("2025-10-10", "2025-10-20", 1.0, 10.0)]
        public void ComputeFineAmount_CorrectlyCalculatesFine(string dueDateStr, string nowStr, decimal dailyRate, decimal expected)
        {
            var dueDate = DateTime.Parse(dueDateStr).ToUniversalTime();
            var now = DateTime.Parse(nowStr).ToUniversalTime();
            var fine = IFineCalculationService.ComputeFineAmount(dueDate, now, dailyRate);
            Assert.Equal(expected, fine);
        }
    }
}
