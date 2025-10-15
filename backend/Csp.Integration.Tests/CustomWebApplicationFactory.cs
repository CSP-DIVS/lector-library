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
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                userService.InitializeDatabaseAsync().GetAwaiter().GetResult();
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


