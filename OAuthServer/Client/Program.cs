using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddOAuth("custom", o =>
    {
        o.SignInScheme = "cookie";

        o.ClientId = "x";
        o.ClientSecret = "x";

        o.AuthorizationEndpoint = "https://localhost:5005/oauth/authorize";
        o.TokenEndpoint = "https://localhost:5005/oauth/token";
        o.CallbackPath = "/oauth/custom-cb";

        o.UsePkce = true; 

        o.ClaimActions.MapJsonKey("sub", "sub"); 
        o.ClaimActions.MapJsonKey("custom 33", "custom"); 

        o.Events.OnCreatingTicket = async ctx =>
        {
            var payloadBase64 = ctx.AccessToken.Split('.')[1];
            var payloadJson = Base64UrlTextEncoder.Decode(payloadBase64);
            var payload = JsonDocument.Parse(payloadJson);
            ctx.RunClaimActions(payload.RootElement);
        };

    });



var app = builder.Build();

app.MapGet("/", async (HttpContext ctx) =>
{ 
    return ctx.User.Claims.Select(x => new { x.Type, x.Value }).ToList();
});

app.MapGet("/login", (HttpContext ctx) =>
{
    return Results.Challenge(
        new AuthenticationProperties()
        {
            RedirectUri = "https://localhost:5004/",

        },
        authenticationSchemes: new List<string>() { "custom" }
        );
});

app.Run();
