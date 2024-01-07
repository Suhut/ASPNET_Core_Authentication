using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Client;

public class CookieAuthenticationStateProvider(HttpClient client) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await GetUserStateAsync();
        return new AuthenticationState(user);
    }

    public async Task Login()
    {
        var result = client.PostAsJsonAsync("/api/login", new { });
        var user = await GetAuthenticationStateAsync();
        base.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task<ClaimsPrincipal> GetUserStateAsync()
    {
        var user = await client.GetFromJsonAsync<Dictionary<string, string>>("api/user");

        return
            new ClaimsPrincipal(
                    new ClaimsIdentity(
                            user.Select(kv => new Claim(kv.Key, kv.Value)),
                             "Somethinng"
                        )
                )
             ;
    }
}
