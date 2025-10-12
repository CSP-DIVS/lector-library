using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Csp.Api.Data;

namespace Csp.Api.Services
{
    /// <summary>
    /// ADO.NET based implementation to calculate and update overdue fines daily.
    /// </summary>
    public class FineCalculationService : IFineCalculationService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public FineCalculationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                 ?? _configuration.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Connection string not found");
        }

        /// <summary>
        /// Calculates fines for all active, unreturned lendings with past due dates.
        /// Sets Lending.FineAmount to daysOverdue * DailyFineRate and leaves FinePaid unchanged.
        /// </summary>
        public async Task<int> CalculateOverdueFinesAsync()
        {
            var dailyRate = _configuration.GetValue<decimal>("Fines:DailyFineRate", 0.25m);

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = SqlQueryLoader.LoadQuery("Lendings", "UpdateOverdueFines");
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Today", DateTime.UtcNow.Date);
            cmd.Parameters.AddWithValue("@DailyRate", dailyRate);

            var affected = await cmd.ExecuteNonQueryAsync();
            return affected;
        }
    }
}
