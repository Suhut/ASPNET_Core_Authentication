using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddOpenIdConnect("openiddict", o =>
     {
         //https://github.com/Suhut/OpenIdDictIdentityServer/blob/nextjs_x_openiddict_x_bff/OpenIdDictIdentityServer/Data/Seed/Worker.cs

         o.SignInScheme = "cookie";
         o.ClientId = "OAuthClient";
         o.ClientSecret = "OAuth-Secret";

         o.Authority = "http://192.168.8.109:7211";
         o.RequireHttpsMetadata = false;

         o.ResponseType = OpenIdConnectResponseType.Code;

         o.CallbackPath = "/cb-oauth";

         o.Scope.Add("profile");
         o.SaveTokens = true;


         o.ClaimActions.MapJsonKey("sub", "id");
         o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");

         o.Events.OnTokenValidated = tokenValidatedContext =>
         {
             var handler = new JwtSecurityTokenHandler();
             // get access token
             var jsonToken = handler.ReadJwtToken(tokenValidatedContext.TokenEndpointResponse.AccessToken);

             var claims = new List<Claim>();

             claims.Add(new Claim("customClaimType", "customClaimValue"));

             var appIdentity = new ClaimsIdentity(claims);

             tokenValidatedContext.Principal.AddIdentity(appIdentity);
             return Task.CompletedTask;

         };
          
         o.Events.OnTicketReceived = async ctx =>
         {
             var access_token = ctx.HttpContext.GetTokenAsync("access_token");
             var headers= ctx.HttpContext.Response.Headers;
             //var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
             //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.to);
             //using var result = await ctx.Backchannel.SendAsync(request);
             //var user = await result.Content.ReadFromJsonAsync<JsonElement>(); 

         };
     });

//.AddOAuth("openiddict", o =>
//{
//    o.SignInScheme = "cookie";

//    o.ClientId = "OAuthClient";
//    o.ClientSecret = "OAuth-Secret";

//    o.AuthorizationEndpoint = "http://192.168.8.109:7211/connect/authorize";
//    o.TokenEndpoint = "http://192.168.8.109:7211/connect/token";
//    o.CallbackPath = "/cb-oauth";

//    o.SaveTokens = true;

//    o.UserInformationEndpoint = "http://192.168.8.109:7211/connect/userinfo";

//    o.ClaimActions.MapJsonKey("sub", "id");
//    o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login"); 

//    o.Events.OnCreatingTicket = async ctx =>
//    {
//        var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
//        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
//        using var result = await ctx.Backchannel.SendAsync(request);
//        var user = await result.Content.ReadFromJsonAsync<JsonElement>();

//        ctx.RunClaimActions(user);
//    };

//});



var app = builder.Build();

app.MapGet("/", async (HttpContext ctx) =>
{
    await ctx.GetTokenAsync("access_token");
    return ctx.User.Claims.Select(x => new { x.Type, x.Value }).ToList();
});

app.MapGet("/login", (HttpContext ctx) =>
{
    return Results.Challenge(
        new AuthenticationProperties()
        {
            RedirectUri = "https://localhost:5005/",
            
        },
        authenticationSchemes: new List<string>() { "openiddict" }
        );
});

app.Run();
