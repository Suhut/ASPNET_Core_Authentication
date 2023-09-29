using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
        //.PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect("xxx"))
        .PersistKeysToFileSystem(new DirectoryInfo("C:\\SUHUT\\DOTNET\\ASP.NET Core Authentication\\CookieSingleSignOn\\TempData"))
        .SetApplicationName("unique")
        ;

builder.Services.AddAuthorization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hallo World! identity");
app.MapGet("/protected", () => "Secret!").RequireAuthorization();  
app.Run();
