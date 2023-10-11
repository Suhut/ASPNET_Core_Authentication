using client;
using Client;
using Microsoft.AspNetCore.Authentication; 
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TokenDatabase>()
    .AddHostedService<TokenRefresher>()
    .AddTransient<RefreshTokenContext>()
    .AddHttpClient()
    .AddHttpClient<PatreonClient>()
    .AddPolicyHandler(PatreonClient.HandleUnAuthorized)
    ;

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddOAuth("patreon", o =>
    {
        //https://github.com/Suhut/OpenIdDictIdentityServer/blob/nextjs_x_openiddict_x_bff/OpenIdDictIdentityServer/Data/Seed/Worker.cs

        o.SignInScheme = "cookie";

        o.ClientId = PatreonOAuthConfig.ClientId;
        o.ClientSecret = PatreonOAuthConfig.ClientSecret;

        o.AuthorizationEndpoint = PatreonOAuthConfig.AuthorizationEndpoint;
        o.TokenEndpoint = PatreonOAuthConfig.TokenEndpoint;
        o.UserInformationEndpoint = PatreonOAuthConfig.UserInformationEndpoint;
        o.CallbackPath = "/oauth/patreon-cb";
        o.SaveTokens = false;

        o.UsePkce = true;

        o.Scope.Clear();
        //o.Scope.Add("identity");
        o.Scope.Add("email"); 
        o.Scope.Add("offline_access"); 

        o.Events.OnCreatingTicket = async ctx =>
        {
            var database = ctx.HttpContext.RequestServices.GetRequiredService<TokenDatabase>();

            using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

            using var result = await ctx.Backchannel.SendAsync(request);
            var content = await result.Content.ReadAsStringAsync();
            var userJson = JsonDocument.Parse(content).RootElement;

            //var patreonId = userJson.GetProperty("data").GetProperty("id").GetString();
            var patreonId = userJson.GetProperty("sub").GetString();
            database.Save(patreonId, new TokenInfo(
                    ctx.AccessToken,
                    ctx.RefreshToken,
                    DateTime.UtcNow.AddSeconds(int.Parse(ctx.TokenResponse.ExpiresIn))
                ));

            ctx.Identity.AddClaim(new Claim("patreonId", patreonId));
        };


    });

builder.Services.AddAuthorization();

var app = builder.Build(); 

app.MapGet("/", (
    ClaimsPrincipal user
    ) => user.Claims.Select(x=>new {x.Type, x.Value}));

app.MapGet("/login", () => Results.Challenge(
    new AuthenticationProperties()
    {
        RedirectUri = "https://localhost:5005/"
    },
     authenticationSchemes: new List<string>() { "patreon" }
    )) ;

app.MapGet("/info", (ClaimsPrincipal user ,PatreonClient patreonClient) =>
{
    var patreonId = user.FindFirstValue("patreonId");
    return patreonClient.GetInfo(patreonId);
});
app.Run();
 