using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

List<String> backList = new();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", o=>
    {
        o.Events.OnValidatePrincipal = ctx =>
        {
            if(backList.Contains(ctx.Principal.FindFirstValue("session")))
            { 
                ctx.RejectPrincipal();
            }
            return Task.CompletedTask;
        };
    })
    ;

var app = builder.Build();

app.MapGet("/login", () => Results.SignIn(
    new ClaimsPrincipal(
        new ClaimsIdentity(
            new[] { 
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), 
                new Claim("session", Guid.NewGuid().ToString()) 
            },
            "cookie"
            )
    ),
    new AuthenticationProperties(),
    "cookie"
    ));

app.MapGet("/user", (ClaimsPrincipal user) => user.Claims.Select(x => new { x.Type, x.Value }));
app.MapGet("/blacklist", (string session) =>
{
    backList.Add(session);
});

app.Run();

