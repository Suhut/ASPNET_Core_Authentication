using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    ;

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

var api = app.MapGroup("api");
api.MapGet("/user", (ClaimsPrincipal user) => user.Claims.ToDictionary(x => x.Type, x => x.Value));
api.MapPost("/login", () => Results.SignIn(
    new ClaimsPrincipal(
        new ClaimsIdentity(
            new[] {new Claim("id", Guid.NewGuid().ToString()) },
            "cookie"
            )
        ),
    authenticationScheme : "cookie"
    ));

app.MapFallbackToFile("index.html");

app.Run();
