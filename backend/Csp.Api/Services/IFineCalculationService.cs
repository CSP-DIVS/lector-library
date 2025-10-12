using System.Threading.Tasks;

namespace Csp.Api.Services
{
    /// <summary>
    /// Defines operations for calculating and updating overdue fines.
    /// </summary>
    public interface IFineCalculationService
    {
        /// <summary>
        /// Calculates and updates overdue fines for all active lendings.
        /// Idempotent: running multiple times per day results in the same values.
        /// </summary>
        Task<int> CalculateOverdueFinesAsync();

        /// <summary>
        /// Helper to compute fine amount given due date, current date, and rate.
        /// Public to enable unit testing of date math.
        /// </summary>
        /// <param name="dueDate">The due date (in UTC).</param>
        /// <param name="nowUtc">The current time (UTC).</param>
        /// <param name="dailyRate">The daily fine rate.</param>
        /// <returns>The computed fine amount. Zero if not overdue.</returns>
        public static decimal ComputeFineAmount(System.DateTime dueDate, System.DateTime nowUtc, decimal dailyRate)
        {
            var daysOverdue = (nowUtc.Date - dueDate.Date).Days;
            if (daysOverdue <= 0) return 0m;
            return daysOverdue * dailyRate;
        }
    }
}
