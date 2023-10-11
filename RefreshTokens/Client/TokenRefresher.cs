using client;

namespace Client;

public class TokenRefresher : BackgroundService
{
    public readonly ILogger<TokenRefresher> _logger;
    public readonly IServiceProvider _serviceProvider; 

    public TokenRefresher(ILogger<TokenRefresher> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TokenDatabase>();
                var refreshTokenContext = scope.ServiceProvider.GetRequiredService<RefreshTokenContext>();
                var tokens = db.Record;

                foreach ( var (patreonId, tokenInfo) in tokens )
                {
                    _logger.LogInformation($"refreshing token for {patreonId}");
                    if(tokenInfo.Expires.Subtract(DateTime.UtcNow)<TimeSpan.FromDays(1))
                    {
                        var result = await refreshTokenContext.RefreshTokenAsync(tokenInfo, stoppingToken);
                        db.Save(patreonId, new TokenInfo(
                                result.AccessToken,
                                result.RefreshToken,
                                DateTime.UtcNow.AddSeconds(int.Parse(result.ExpiresIn))
                                ) );
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
