using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("def")
    .AddCookie("def")
    ;

builder.Services.AddAuthorization();
 
var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(_ => { });

app.UseSpa(x => x.UseProxyToSpaDevelopmentServer("http://localhost:3000"));

app.MapGet("/api/test", () => "secret").RequireAuthorization();

app.MapPost("/api/login", async ctx =>
{
    await ctx.SignInAsync("def", new ClaimsPrincipal(
        new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            },
            "def"
            )
        ), new AuthenticationProperties()
        {
            IsPersistent = true,
        }
        )
    ;
})
;

app.Run();
 