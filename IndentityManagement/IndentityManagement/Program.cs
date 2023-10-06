using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddDefaultTokenProviders();

builder.Services.AddDataProtection();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("manager",pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
            .RequireClaim("role", "manager");
    });
});

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

app.UseAuthentication(); 

app.MapGet("/", () => "Hallo World!");
app.MapGet("/protected", () => "Something super secret!").RequireAuthorization("manager");
//app.MapGet("/test", (
//    UserManager<IdentityUser> userMgr,
//    SignInManager<IdentityUser> signMgr
//    ) =>
//{
//    //userMgr.ResetPasswordAsync();
//});

app.MapGet("/register", async (
    string username,
    string password,
    IPasswordHasher<User> hasher,
    Database db,
    HttpContext ctx

    ) =>
{
    var user = new User() { Username = username };
    user.PasswordHash = hasher.HashPassword(user, password);
    await db.PutAsynch(user);

    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user)
        );

    return user;
});

app.MapGet("/login", async (
    string username,
    string password,
    IPasswordHasher<User> hasher,
    Database db,
    HttpContext ctx

    ) =>
{
    var user = await db.GetUserAsync(username);
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

    if (result == PasswordVerificationResult.Failed)
    {
        return "bad credentials";
    } 

    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user)
        );

    return "logged in";
});

app.MapGet("/promote", async (
    string username,  
    Database db  

    ) =>
{
    var user = await db.GetUserAsync(username);
    user.Claims.Add(new UserClaim() { Type = "role", Value = "manager" });
    await db.PutAsynch(user);

    return "promoted";
});

app.MapGet("/start-password-reste", async (
    string username,
    Database db,
    IDataProtectionProvider provider

    ) =>
{
    var protector = provider.CreateProtector("PasswordReset");
    var user = await db.GetUserAsync(username);  

    return protector.Protect(user.Username);
});

app.MapGet("/end-password-reset", async (
    string username,
    string password,
    string hash,
    Database db,
    IPasswordHasher<User> hasher,
    IDataProtectionProvider provider

    ) =>
{
    var protector = provider.CreateProtector("PasswordReset"); 
    var hashUsername = protector.Unprotect(hash);
    if(hashUsername!=username)
    {
        return "bad hash";
    }

    var user = await db.GetUserAsync(username);
    user.PasswordHash = hasher.HashPassword(user, password);
    await db.PutAsynch(user);

    return "password reset";
});

app.Run();

public class UserHelper
{
    public static ClaimsPrincipal Convert(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim("username", user.Username)
        };

        claims.AddRange(user.Claims.Select(x => new Claim(x.Type, x.Value)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}