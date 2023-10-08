using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("def")
    .AddCookie("def")
    ;

builder.Services.AddAuthorization();

builder.Services.AddCors(o =>
{
    o.AddPolicy("fe", pb => pb.WithOrigins("https://localhost:3000").AllowCredentials().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("fe"); 

app.UseAuthentication();
app.UseAuthorization();  

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
