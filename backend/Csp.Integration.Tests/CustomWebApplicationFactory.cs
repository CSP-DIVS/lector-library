using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MySql; //docker containers for testing
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Csp.Api.Services;

namespace Csp.Integration.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MySqlContainer _mysqlContainer;
        private bool _useContainer;

        public CustomWebApplicationFactory()
        {
            try
            {
                // If Docker is unavailable, this Build() will throw during validation
                _mysqlContainer = new MySqlBuilder()
                    .WithImage("mysql:8.4")
                    .WithDatabase("csp_test")
                    .WithUsername("test")
                    .WithPassword("testpwd")
                    .Build();
                _useContainer = true;
            }
            catch
            {
                // Fall back to existing connection string (e.g., local dev DB)
                _useContainer = false;
                _mysqlContainer = null!;
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                if (_useContainer)
                {
                    var cs = _mysqlContainer.GetConnectionString();
                    configBuilder.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", cs)
                    });
                }
            });

            builder.ConfigureTestServices(services =>
            {
                // Ensure our IUserService is available and DB init will run
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                // Kick database initialization on host startup
                userService.InitializeDatabaseAsync().GetAwaiter().GetResult();
            });
        }

        public async Task InitializeAsync()
        {
            if (_useContainer)
            {
                await _mysqlContainer.StartAsync();
            }
        }

        public new async Task DisposeAsync()
        {
            if (_useContainer && _mysqlContainer is not null)
            {
                await _mysqlContainer.DisposeAsync();
            }
        }
    }
}


