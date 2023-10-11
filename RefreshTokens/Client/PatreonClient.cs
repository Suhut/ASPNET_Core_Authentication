using client;
using Polly;
using System.Net;
using System.Net.Http.Headers;

namespace Client;

public class PatreonClient
{
    private readonly HttpClient _http;
    private readonly TokenDatabase _tokenDatabase;

    public PatreonClient(HttpClient http, TokenDatabase tokenDatabase)
    {
        _http = http;
        _tokenDatabase = tokenDatabase;
    }

    public async Task<string> GetInfo(string patreonId )
    {
        var tokenInfo = _tokenDatabase.Get(patreonId );
        using var request = new HttpRequestMessage(HttpMethod.Get, PatreonOAuthConfig.UserInformationEndpoint);
        request.Headers.Add( "patreonId", patreonId );
        request.Headers.Authorization= new AuthenticationHeaderValue("Bearer", tokenInfo.AccessToken);

        using var result = await _http.SendAsync( request );
        return await result.Content.ReadAsStringAsync();
    }

    public static IAsyncPolicy<HttpResponseMessage> HandleUnAuthorized(
        IServiceProvider serviceProvider,
        HttpRequestMessage request
        )
    {
        return  Policy
            .HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.Unauthorized)
            .RetryAsync(1, async (
                e,
                attempt
            ) =>
            {
                using var scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<TokenDatabase>();
                var refreshTokenContext = scope.ServiceProvider.GetRequiredService<RefreshTokenContext>();
                var patreonId = request.Headers.GetValues("patreonId").First();
                var tokenInfo = database.Get(patreonId);
                var result = await refreshTokenContext.RefreshTokenAsync(tokenInfo, CancellationToken.None);

                database.Save(patreonId,
                        new TokenInfo(result.AccessToken, result.RefreshToken, DateTime.UtcNow.AddSeconds(int.Parse(result.ExpiresIn))
                    ));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            });
            
    }
}
