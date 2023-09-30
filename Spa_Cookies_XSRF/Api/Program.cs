using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.AddControllers();

builder.Services.AddAuthorization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
    {
        o.Cookie.Domain = ".company.local";
        //o.Cookie.SecurePolicy = CookieSecurePolicy.None;
        //o.Cookie.SameSite = SameSiteMode.None;
    });


var app = builder.Build();

app.UseCors(x =>

    //x.WithOrigins("https://app.company.local")
    //.AllowAnyMethod()
    //.AllowAnyHeader()
    //.AllowCredentials()

    //x.AllowAnyOrigin()
    //.AllowAnyMethod()
    //.AllowAnyHeader()
    //.AllowCredentials()

    x.WithOrigins("https://app.company.local", "https://evil.local")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()

    );

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Guid.NewGuid().ToString());

app.MapGet("/protected", (HttpContext ctx) => ctx.User.FindFirst(ClaimTypes.Name)?.Value).RequireAuthorization();

app.MapPost("/data", () =>  "welp we edited something....").RequireAuthorization();

app.MapPost("/login", (LoginForm form, HttpContext ctx) =>
{
    ctx.SignInAsync(new ClaimsPrincipal(new[]
    {
        new ClaimsIdentity(new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, form.Username??""),
        },
        CookieAuthenticationDefaults.AuthenticationScheme
        )
    }
    ));
    return "ok";
});


app.MapDefaultControllerRoute();

app.Run();


public class LoginForm
{
    public string? Username { get; set;}
    public string? Password { get; set;}
}
