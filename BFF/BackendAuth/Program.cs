using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpClient()
    .AddDataProtection()
    ;

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(tCtx =>
    {
        tCtx.AddRequestTransform(async rc =>
        {
            if ((rc.HttpContext.User.Identity?.IsAuthenticated ?? false) &&
                rc.DestinationPrefix == "http://192.168.8.109:7211")
            {
                var accessToken = await rc.HttpContext.GetTokenAsync("access_token");
                rc.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        });
    })
    ;

builder.Services.AddAuthentication("auth-cookie")
    .AddCookie("auth-cookie")
    .AddOAuth("youtube", o =>
    {
        //https://github.com/Suhut/OpenIdDictIdentityServer/blob/nextjs_x_openiddict_x_bff/OpenIdDictIdentityServer/Data/Seed/Worker.cs
        o.SignInScheme = "auth-cookie";
        o.ClientId = "BffClient";
        o.ClientSecret = "Bff-Secret";
        o.SaveTokens = true;

        o.UsePkce = true;

        o.Scope.Clear();
        o.Scope.Add("email");
        o.Scope.Add("offline_access");

        o.AuthorizationEndpoint = "http://192.168.8.109:7211/connect/authorize";
        o.TokenEndpoint = "http://192.168.8.109:7211/connect/token";
        o.UserInformationEndpoint = "http://192.168.8.109:7211/connect/userinfo";
        o.CallbackPath = "/oauth/yt-cb";
    }
    )
    ;

var app = builder.Build();



app.MapGet("/login", () => Results.Challenge(
    new AuthenticationProperties()
    {
        RedirectUri = "/"
    },
    authenticationSchemes: new List<string>() { "youtube" }
    ));

app.MapGet("/api-yt", async (
        IHttpClientFactory clientFactory,
        HttpContext ctx
    ) =>
{
    var accessToken = await ctx.GetTokenAsync("access_token");
    var client = clientFactory.CreateClient();

    using var req = new HttpRequestMessage(HttpMethod.Get, "http://192.168.8.109:7211/connect/userinfo");

    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    using var response = await client.SendAsync(req);
    return await response.Content.ReadAsStringAsync();
}).RequireAuthorization();

app.MapReverseProxy();
app.Run();
