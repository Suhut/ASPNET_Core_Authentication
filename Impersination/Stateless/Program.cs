using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddCookie("temp")
    ;

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", (ClaimsPrincipal user) => user.Claims.Select(x => $"{x.Type}: {x.Value}"));

app.MapGet("/login", () => Results.SignIn(
    new ClaimsPrincipal(
        new ClaimsIdentity(
            new[]
            {
                new Claim("user_id", Guid.NewGuid().ToString()),
                new Claim("name", "original")
            },
            "cookie"
            )
        ),
    authenticationScheme: "cookie"
    ));

app.MapGet("/imp", async (string name, HttpContext ctx) =>
{
    var user = ctx.User;

    await ctx.SignInAsync(
        "temp",
        user
    );

    await ctx.SignInAsync(
        "cookie",
        new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                new Claim("user_id", Guid.NewGuid().ToString()),
                new Claim("name", name),
                new Claim("imp", "true")
                },
                "cookie"
                )
            )
    );
}
).RequireAuthorization();

app.MapGet("/logout", async (HttpContext ctx) =>
    {
        if (ctx.User.HasClaim("imp", "true"))
        {
            var original = await ctx.AuthenticateAsync("temp");

            await ctx.SignInAsync(
                "cookie",
                original.Principal
                );

            await ctx.SignOutAsync("temp");

            return Results.Ok();
        }

        return Results.SignOut(authenticationSchemes: new List<string>() { "cookie", "temp" });
    }
    ).RequireAuthorization();

app.Run();

