using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stateful;
using System.Security.Claims;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddMyCookie("cookie", null, null!) 
    ;

builder.Services.AddSingleton<Database>();
builder.Services.AddAuthorization();

var app = builder.Build(); 

app.MapGet("/", (ClaimsPrincipal user) => user.Claims.Select(x => $"{x.Type}: {x.Value}"));

app.MapGet("/login", () => Results.SignIn(
    new ClaimsPrincipal(
        new ClaimsIdentity(
            new[]
            {
                new Claim("user_id", "one"), 
            },
            "cookie"
            )
        ),
    authenticationScheme: "cookie"
    ));

app.MapGet("/imp", async (
    Database database, 
    ClaimsPrincipal user
    ) =>
    {
        database[user.FindFirstValue("user_id")].Impersonating = "two";
    }
    ).RequireAuthorization();

app.MapGet("/logout", async (
    Database database,
    ClaimsPrincipal user
    ) =>
{
    var originalUser = user.FindFirstValue("original");

    if (!string.IsNullOrWhiteSpace(originalUser))
    {
        database[originalUser].Impersonating = null;
        return Results.Ok();
    }

    return Results.SignOut(authenticationSchemes: new List<string>() { "cookie" });
}
    ).RequireAuthorization();

app.Run();

public class CookieAuthenticationHandlerExt : CookieAuthenticationHandler
{
    private readonly Database _database;
    public CookieAuthenticationHandlerExt(
        IOptionsMonitor<CookieAuthenticationOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        Database database
        ) : base(options, logger, encoder, clock)
    {
        _database = database;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await base.HandleAuthenticateAsync();

        if(!result.Succeeded)
        {
            return result;
        }

        var userId = result.Principal.FindFirstValue("user_id");
        var user = _database[userId];

        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(user.Impersonating))
        {
            claims.Add(new Claim("user_id", user.Impersonating));
            claims.Add(new Claim("original", userId));

            user = _database[user.Impersonating];  
        }
        else
        {
            claims.Add(new Claim("user_id", userId));
        }

        claims.Add(new Claim("name", user.Name));


        return AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(
                new ClaimsIdentity( claims, "cookie")
                ),
                result.Properties,
                Scheme.Name
            ));
    }
}