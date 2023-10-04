using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddScheme<CookieAuthenticationOptions, visitorAuthHandler>("visitor", o =>
    {

    })
    //.AddCookie("visitor")
    .AddCookie("local")
    .AddCookie("patreon")
    //.AddOAuth("external-patreon", o =>
    //{
    //    o.SignInScheme = "xxx";
    //    o.ClientId = "xxx";
    //    o.ClientSecret = "xxx";
    //    o.AuthorizationEndpoint = "xxx";
    //    o.TokenEndpoint = "xxx";
    //    o.UserInformationEndpoint = "xxx";

    //    o.CallbackPath = "/cb-patreon";

    //    o.Scope.Add("profile");
    //    o.SaveTokens = true;  
    //})

    //https://github.com/Suhut/OpenIdDictIdentityServer/blob/nextjs_x_openiddict_x_bff/OpenIdDictIdentityServer/Data/Seed/Worker.cs
    .AddOpenIdConnect("external-patreon", o =>
    {
        o.SignInScheme = "patreon";
        o.ClientId = "AuthenticationSchemaClient";
        o.ClientSecret = "AuthenticationSchema-Secret";

        o.Authority = "http://192.168.8.109:7211";
        o.RequireHttpsMetadata = false;

        o.ResponseType = OpenIdConnectResponseType.Code; 

        o.CallbackPath = "/cb-patreon";

        o.Scope.Add("profile");
        o.SaveTokens = true;
    })
    ;

builder.Services.AddAuthorization(b =>
{
    b.AddPolicy("customer", p =>
    {
        p.AddAuthenticationSchemes("patreon", "local", "visitor")
            .RequireAuthenticatedUser();
    });

    b.AddPolicy("user", p =>
    {
        p.AddAuthenticationSchemes("local")
            .RequireAuthenticatedUser();
    });
}); 

var app = builder.Build(); 
 
app.UseAuthentication();
app.UseAuthorization();
 

app.MapGet("/", ctx => Task.FromResult("Hallo World")).RequireAuthorization("customer");

app.MapGet("/login-local", async (ctx) =>
{
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "suhut"));
    var identity = new ClaimsIdentity(claims, "local");
    var user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("local", user); 
});


app.MapGet("/login-patreon", async (ctx) =>
{
    await ctx.ChallengeAsync("external-patreon", new AuthenticationProperties()
    {
        RedirectUri = "/"
    }) ;
}).RequireAuthorization("user");

app.Run();

public class visitorAuthHandler : CookieAuthenticationHandler
{
    public visitorAuthHandler(IOptionsMonitor<CookieAuthenticationOptions> options, 
                            ILoggerFactory logger, 
                            UrlEncoder encoder, 
                            ISystemClock clock) : base(options, logger, encoder, clock)
    {

    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        var result = await base.HandleAuthenticateAsync();
        if(result.Succeeded)
        {
            return result;
        }

        var claims = new List<Claim>();
        claims.Add(new Claim("usr", "suhut"));
        var identity = new ClaimsIdentity(claims, "visitor");
        var user = new ClaimsPrincipal(identity);

        await Context.SignInAsync("visitor", user);

        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));

    }
}