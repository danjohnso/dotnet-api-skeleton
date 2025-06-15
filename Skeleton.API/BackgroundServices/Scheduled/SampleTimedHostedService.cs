using Skeleton.Data;

namespace Skeleton.API.BackgroundServices.Scheduled
{
    public class SampleTimedHostedService(ILogger<SampleTimedHostedService> logger, IServiceScopeFactory serviceScopeFactory) 
        : TimerHostedService(logger, serviceScopeFactory, TimeSpan.FromHours(24))
    {
        protected override async Task RunJobAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation($"{nameof(SampleTimedHostedService)} is starting to work");

            try
            {
                using IServiceScope scope = ServiceScopeFactory.CreateScope();

                DataContext dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                
                //can do whatever, nice to have these in app

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Unable to do {nameof(SampleTimedHostedService)} work");
            }

            Logger.LogInformation($"{nameof(SampleTimedHostedService)} is finished working");
        }
    }
}
