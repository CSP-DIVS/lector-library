using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MySql;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Csp.Api.Services;
using MySql.Data.MySqlClient;

namespace Csp.Integration.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MySqlContainer _mysqlContainer;

        public CustomWebApplicationFactory()
        {
            try
            {
                _mysqlContainer = new MySqlBuilder()
                    .WithImage("mysql:8.4")
                    .WithDatabase("csp_test")
                    .WithUsername("test")
                    .WithPassword("testpwd")
                    .Build();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Docker is required to run integration tests. Please ensure Docker Desktop is installed and running.\n" +
                    "Download from: https://www.docker.com/products/docker-desktop", ex);
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var cs = _mysqlContainer.GetConnectionString();
                configBuilder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", cs)
                });
            });

            builder.ConfigureTestServices(services =>
            {
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();

                // Ensure core tables exist and are seeded
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                userService.InitializeDatabaseAsync().GetAwaiter().GetResult();

                // Ensure lending tables are present
                var lendingService = scope.ServiceProvider.GetRequiredService<ILendingService>();
                lendingService.InitializeLendingTablesAsync().GetAwaiter().GetResult();

                // Ensure payments table exists for payment-related tests
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    using var conn = new MySqlConnection(connectionString);
                    conn.Open();

                    // Try to load the SQL from the API's Data folder if available, otherwise use a minimal fallback
                    string sql;
                    try
                    {
                        var sqlPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Data", "Payments", "CreatePaymentsTable.sql");
                        sql = System.IO.File.Exists(sqlPath)
                            ? System.IO.File.ReadAllText(sqlPath)
                            : @"CREATE TABLE IF NOT EXISTS payments (
                                    Id INT AUTO_INCREMENT PRIMARY KEY,
                                    LendingId INT NOT NULL,
                                    MemberId INT NOT NULL,
                                    Amount DECIMAL(10,2) NOT NULL,
                                    PaymentMethod VARCHAR(50) NOT NULL DEFAULT 'Cash',
                                    PaymentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                    RecordedBy INT NOT NULL,
                                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                                    INDEX idx_payments_lending (LendingId),
                                    INDEX idx_payments_member (MemberId),
                                    INDEX idx_payments_date (PaymentDate),
                                    INDEX idx_payments_recorded_by (RecordedBy)
                                );";
                    }
                    catch
                    {
                        sql = @"CREATE TABLE IF NOT EXISTS payments (
                                    Id INT AUTO_INCREMENT PRIMARY KEY,
                                    LendingId INT NOT NULL,
                                    MemberId INT NOT NULL,
                                    Amount DECIMAL(10,2) NOT NULL,
                                    PaymentMethod VARCHAR(50) NOT NULL DEFAULT 'Cash',
                                    PaymentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                    RecordedBy INT NOT NULL,
                                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                                    INDEX idx_payments_lending (LendingId),
                                    INDEX idx_payments_member (MemberId),
                                    INDEX idx_payments_date (PaymentDate),
                                    INDEX idx_payments_recorded_by (RecordedBy)
                                );";
                    }

                    using var cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public async Task InitializeAsync()
        {
            await _mysqlContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _mysqlContainer.DisposeAsync();
        }
    }
}