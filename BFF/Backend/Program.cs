using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Net.Http.Headers;
using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TokenDatabase>()
    .AddHttpClient()
    .AddDataProtection()
    ;


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(tCtx =>
    {
        tCtx.AddRequestTransform(rc =>
        {
            if ((rc.HttpContext.User.Identity?.IsAuthenticated ?? false) &&
                rc.DestinationPrefix == "http://192.168.8.109:7211")
            {
                var tokenDb = rc.HttpContext.RequestServices.GetRequiredService<TokenDatabase>();
                var userId = rc.HttpContext.User.FindFirst("id")?.Value;
                var accessToken = tokenDb.GetToken(userId);

                rc.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return ValueTask.CompletedTask;
        });
    })
    ;

builder.Services.AddAuthentication("auth-cookie")
    .AddCookie("auth-cookie")
    .AddOAuth("youtube", o =>
    {
        //https://github.com/Suhut/OpenIdDictIdentityServer/blob/nextjs_x_openiddict_x_bff/OpenIdDictIdentityServer/Data/Seed/Worker.cs

        o.SignInScheme = "auth-cookie";
        o.ClientId = "BffClient02";
        o.ClientSecret = "Bff-Secret02";
        o.SaveTokens = false;

        o.UsePkce = true;

        o.Scope.Clear();
        o.Scope.Add("email");
        o.Scope.Add("offline_access");

        o.AuthorizationEndpoint = "http://192.168.8.109:7211/connect/authorize";
        o.TokenEndpoint = "http://192.168.8.109:7211/connect/token";
        o.UserInformationEndpoint = "http://192.168.8.109:7211/connect/userinfo";
        o.CallbackPath = "/oauth/yt-cb";



        o.Events.OnCreatingTicket = async ctx =>
        {
            var tokenDatabase = ctx.HttpContext.RequestServices.GetRequiredService<TokenDatabase>();
            var authenticationHandlerProvider = ctx.HttpContext.RequestServices
                    .GetRequiredService<IAuthenticationHandlerProvider>();

            var handler = await authenticationHandlerProvider.GetHandlerAsync(ctx.HttpContext, "auth-cookie");
            var authresult = await handler.AuthenticateAsync();
            if (!authresult.Succeeded)
            {
                ctx.Fail("failed authentication");
                return;
            }

            var cp = authresult.Principal;
            var userId = cp.FindFirstValue("id");
            tokenDatabase.StoreToken(userId!, ctx.AccessToken);

            ctx.Principal = cp.Clone();
            var identity = ctx.Principal.Identities.First(x => x.AuthenticationType == "auth-cookie");
            identity.AddClaim(new Claim("yt-token", "y"));
        };


    }
    )
    ;

builder.Services.AddAuthorization(b =>
{
    b.AddPolicy("youtube-enabled", pb =>
    {
        pb.AddAuthenticationSchemes("auth-cookie")
            .RequireClaim("yt-token", "y")
            .RequireAuthenticatedUser();
    });

});

var app = builder.Build();

app.MapGet("/api/login", (TokenDatabase tDb) =>
{
    var userId = "user_identifier";
    List<Claim> claims = new() { new Claim("id", userId) };
    if (!string.IsNullOrEmpty(tDb.GetToken(userId)))
    {
        claims.Add(new Claim("yt-token", "y"));
    }

    return Results.SignIn(
        new(new ClaimsIdentity(claims, "auth-cookie")),
        authenticationScheme: "auth-cookie"
        );
});


app.MapGet("/api/user", (ClaimsPrincipal user) => new
{
    Id = user.FindFirst("id")?.Value,
    ytEnabled = user.FindFirst("yt-token")?.Value == "y",
}).RequireAuthorization();

app.MapGet("/api/youtube-connect", () => Results.Challenge(
        new AuthenticationProperties()
        {
            RedirectUri = "https://localhost:7200/"
        },
        authenticationSchemes: new List<string>() { "youtube" }
    ))
    ;


app.MapGet("/api-yt", async (
        IHttpClientFactory clientFactory,
        ClaimsPrincipal user,
        TokenDatabase tDb
    ) =>
{
    var userId = user.FindFirst("id")?.Value;
    var token = tDb.GetToken(userId);
    var client = clientFactory.CreateClient();

    using var req = new HttpRequestMessage(HttpMethod.Get,
        "http://192.168.8.109:7211/connect/userinfo"); 

    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(req);
    var result = await response.Content.ReadAsStringAsync();
    return result;
}).RequireAuthorization("youtube-enabled");

app.MapForwarder("/{**rest}", "http://localhost:5173");

app.MapReverseProxy();
app.Run();


public class TokenDatabase
{
    private readonly IDataProtectionProvider _dataProtection;
    public Dictionary<string, string> _database = new();

    public TokenDatabase(IDataProtectionProvider dataProtection)
    {
        _dataProtection = dataProtection;
    }

    public void StoreToken(string userId, string token)
    {
        var protector = _dataProtection.CreateProtector(userId + "token");
        _database[userId] = protector.Protect(token);
    }

    public string? GetToken(string userId)
    {
        if (_database.TryGetValue(userId, out var token))
        {
            var protector = _dataProtection.CreateProtector(userId + "token");
            return protector.Unprotect(token);
        }
        return null;
    }
}