using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Csp.Api.Services
{
    /// <summary>
    /// Background hosted service to calculate overdue fines on a schedule.
    /// </summary>
    public class FineCalculationHostedService : BackgroundService
    {
        private readonly ILogger<FineCalculationHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public FineCalculationHostedService(
            ILogger<FineCalculationHostedService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run once on startup (optional):
            await SafeRunOnce(stoppingToken);

            // Then run periodically every 24h (configurable)
            var intervalHours = _configuration.GetValue<int>("Fines:IntervalHours", 24);
            var delay = TimeSpan.FromHours(intervalHours <= 0 ? 24 : intervalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                await SafeRunOnce(stoppingToken);
            }
        }

        private async Task SafeRunOnce(CancellationToken token)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var fineService = scope.ServiceProvider.GetRequiredService<IFineCalculationService>();
                var affected = await fineService.CalculateOverdueFinesAsync();
                _logger.LogInformation("Fine calculation completed. Rows affected: {Affected}", affected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fine calculation failed");
            }
        }
    }
}
