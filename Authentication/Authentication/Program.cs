using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

const string AuthSchema = "cookie";
const string AuthSchema2 = "cookie2";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(AuthSchema)
    .AddCookie(AuthSchema)
    .AddCookie(AuthSchema2)
    ;

builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("eu policy", pd =>
    {
        pd.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthSchema)
            .AddRequirements(new MyRequirement())
            .RequireClaim("passport_type", "eur");
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//app.Use((ctx, next) =>
//{
//    if(ctx.Request.Path.StartsWithSegments("/login"))
//    {
//        return next();
//    }

//    if(!ctx.User.Identities.Any(x=>x.AuthenticationType==AuthSchema))
//    {
//        ctx.Response.StatusCode = 401;
//        return Task.CompletedTask;
//    }

//    if (!ctx.User.HasClaim("passport_type", "eur"))
//    {
//        ctx.Response.StatusCode = 403;
//        return Task.CompletedTask;
//    }

//    return next();
//});

app.MapGet("/unsecure", (HttpContext ctx) =>
{

    return ctx.User.FindFirst("usr")?.Value ?? "empty";

});

app.MapGet("/sweden", (HttpContext ctx) =>
{
    //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthSchema))
    //{
    //    ctx.Response.StatusCode = 401;
    //    return "";
    //}

    //if (!ctx.User.HasClaim("passport_type", "eur"))
    //{
    //    ctx.Response.StatusCode = 403;
    //    return "";
    //} 

    return "allowed";

}).RequireAuthorization("eu policy");

app.MapGet("/norway", (HttpContext ctx) =>
{
    //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthSchema))
    //{
    //    ctx.Response.StatusCode = 401;
    //    return "";
    //}

    //if (!ctx.User.HasClaim("passport_type", "nor"))
    //{
    //    ctx.Response.StatusCode = 403;
    //    return "";
    //}

    return "allowed";

}).RequireAuthorization("au policy");
 
app.MapGet("/denmark", (HttpContext ctx) =>
{
    //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthSchema2))
    //{
    //    ctx.Response.StatusCode = 401;
    //    return "";
    //}

    //if (!ctx.User.HasClaim("passport_type", "den"))
    //{
    //    ctx.Response.StatusCode = 403;
    //    return "";
    //}

    return "allowed";

}).RequireAuthorization("au policy");

app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "suhut"));
    claims.Add(new Claim("passport_type", "eur"));
    var identity = new ClaimsIdentity(claims, AuthSchema);
    var user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(AuthSchema, user);
    return "ok";
}).AllowAnonymous();

app.Run();


//https://stackoverflow.com/questions/49389970/asp-net-core-authorization-combining-or-requirements
//https://coderethinked.com/policy-based-authorization-in-asp-net-core/
public class MyRequirement : IAuthorizationRequirement { }

public class MyRequirementHandler : AuthorizationHandler<MyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MyRequirement requirement)
    {
        //context.User
        //context.Succeed(new MyRequirement());
        return Task.CompletedTask;
    }
}
