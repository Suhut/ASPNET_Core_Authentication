using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddCookie("default", o=>
    {
        o.Cookie.Name = "MyCookie";
        //o.Cookie.Domain = "";
        //o.Cookie.Path = "/test";
        //o.Cookie.HttpOnly = false;
        //o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        //o.Cookie.SameSite = SameSiteMode.Strict;

        //o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        //o.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options=>
{
    options.AddPolicy("MyPolicy", pb =>
    {
        pb.RequireAuthenticatedUser()
        .RequireClaim("doesntexists", "nonse");
    });
});
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hallo World!");

app.MapGet("/test", () => "Hallo World!").RequireAuthorization();

app.MapGet("/test22", async (HttpContext ctx) =>
{
    await ctx.ChallengeAsync("default", 
        new AuthenticationProperties()
        {
            RedirectUri="/anything-that-we-want"
        }
        );
    return "ok";

});

app.MapPost("/login",async (HttpContext ctx) =>
{
    await ctx.SignInAsync("default", new ClaimsPrincipal(
        new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                },"default"
            )
        ),
            new AuthenticationProperties()
            {
                IsPersistent = true,
            }
        );
    return "ok";
});

app.MapPost("/signout",async (HttpContext ctx) =>
{
    await ctx.SignOutAsync("default",  
            new AuthenticationProperties()
            {
                IsPersistent = true,
            }
        );
    return "ok";
});


app.MapDefaultControllerRoute();

app.Run(); 