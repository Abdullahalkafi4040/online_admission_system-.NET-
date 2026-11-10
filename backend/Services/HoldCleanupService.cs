using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolBackend.Services;

namespace SchoolBackend.Services
{
    public class HoldCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HoldCleanupService> _logger;

        public HoldCleanupService(IServiceProvider serviceProvider, ILogger<HoldCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hold Cleanup Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var admissionService = scope.ServiceProvider.GetRequiredService<IAdmissionService>();
                        
                        await admissionService.CheckAndExpireHoldsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired holds.");
                }

                // প্রতি ৫ সেকেন্ড পর পর রান করবে
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}