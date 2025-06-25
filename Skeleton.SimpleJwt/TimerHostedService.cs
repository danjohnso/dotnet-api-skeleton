using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Skeleton.SimpleJwt
{
    //TODO: These will need to move to webjobs if we scale horizontally
    public abstract class TimerHostedService(ILogger<TimerHostedService> logger, IServiceScopeFactory serviceScopeFactory, TimeSpan occurrence) : IHostedService, IDisposable
    {
        protected readonly ILogger Logger = logger;
        protected readonly IServiceScopeFactory ServiceScopeFactory = serviceScopeFactory;

        private Timer? _timer;
        private readonly TimeSpan _timespan = occurrence;
        private Task? _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("{TaskName} is starting", GetType().Name);

            _timer = new Timer(ExecuteTask, null, _timespan, TimeSpan.FromMilliseconds(-1));

            return Task.CompletedTask;
        }

        private void ExecuteTask(object? state)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _executingTask = ExecuteTaskAsync(_stoppingCts.Token);
        }

        private async Task ExecuteTaskAsync(CancellationToken stoppingToken)
        {
            await RunJobAsync(stoppingToken);
            _timer?.Change(_timespan, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// This method is called when the <see cref="IHostedService"/> starts. The implementation should return a task 
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)"/> is called.</param>
        /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
        protected abstract Task RunJobAsync(CancellationToken stoppingToken);

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("{TaskName} is stopping", GetType().Name);
            _timer?.Change(Timeout.Infinite, 0);

            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
