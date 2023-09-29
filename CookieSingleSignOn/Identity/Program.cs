using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
        //.PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect("xxx"))
        .PersistKeysToFileSystem(new DirectoryInfo("C:\\SUHUT\\DOTNET\\ASP.NET Core Authentication\\CookieSingleSignOn\\TempData"))
        .SetApplicationName("unique")
        ;


builder.Services.AddAuthorization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
    {
        o.Cookie.Domain = ".company.local";
    });

var app = builder.Build();
  
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hallo World! identity");
app.MapGet("/protected", () => "Secret!").RequireAuthorization();
app.MapGet("/login", (HttpContext ctx) =>
{
    ctx.SignInAsync(new ClaimsPrincipal(new[]
    {
        new ClaimsIdentity(new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        },
        CookieAuthenticationDefaults.AuthenticationScheme
        )
    }
    ));
    return "ok";
});

app.Run();
 