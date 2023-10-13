using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var rsaKey = RSA.Create();

List<String> backList = new();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("jwt")
    .AddJwtBearer("jwt", o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
        };

        o.Events = new JwtBearerEvents()
        {
            OnMessageReceived = (ctx) =>
            {
                if (ctx.Request.Query.ContainsKey("t"))
                {
                    ctx.Token = ctx.Request.Query["t"]; 

                    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(ctx.Token));
                    var hashString = Convert.ToBase64String(hash);
                    if (backList.Contains(hashString))
                    {
                        ctx.Fail("token invalid");
                    }

                }
                return Task.CompletedTask;
            }
        };

        o.Configuration = new OpenIdConnectConfiguration()
        {
            SigningKeys =
            {
                new RsaSecurityKey(rsaKey)
            }
        };

        o.MapInboundClaims = false;
    }
    );

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", () =>
{
    var handler = new JsonWebTokenHandler();
    var key = new RsaSecurityKey(rsaKey);
    var token = handler.CreateToken(new SecurityTokenDescriptor()
    {
        Issuer = "https://localhost:5000",
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("sub", Guid.NewGuid().ToString()), 
        }),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
    });
    return token;
});

app.MapGet("/user", (ClaimsPrincipal user) => user.Claims.Select(x => new { x.Type, x.Value }));
app.MapGet("/blacklist", (string token) =>
{
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    var hashString = Convert.ToBase64String(hash);
    backList.Add(hashString);
});


app.Run();


