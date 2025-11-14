

using CafeManagent.Services.Interface.AuthenticationModule;

namespace CafeManagent.BackgroundWorkers
{
    public class CleanTokenBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CleanTokenBackgroundWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
                    await refreshTokenService.Clear(DateTime.Now.AddDays(-10));
                }

                await Task.Delay(TimeSpan.FromDays(10), stoppingToken);
            }
        }
    }
}
