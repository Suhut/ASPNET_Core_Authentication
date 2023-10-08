using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var jwkString = @"{
    ""additionalData"": {},
    ""alg"": null,
    ""crv"": null,
    ""d"": null,
    ""dp"": null,
    ""dq"": null,
    ""e"": ""AQAB"",
    ""k"": null,
    ""keyId"": null,
    ""keyOps"": [],
    ""kid"": null,
    ""kty"": ""RSA"",
    ""n"": ""rDxXd2BC2M5dxy-ySvjMuxTtn91cMTv2znKfDOwKG44JY6I09h8AWwHp1c872t5NiVemxGyh_54z2UP6bU6HDhfzJESRjx7dxogMXiSOeR3NGQThZBWK9tYCWP4lVKcfP226ns7ETTjbA_dD_DKb9oB2S8y9KIs7V98waakyTj_xOLuKoeZw3rkEMTPuvCgrafXPbHkvwQKAvRTlY8o4buvnET_qbl7YZdo9ACBxPdSb3ikiMG-1nOEjenykaYniVD5fCyjnyJfu7LxdIrV0AjlJjoPTiIO1WvI1CKWvswLGf5mGrz7ThDvP_tI1T_HDQYQFQf38pKntbKw-SmtMZQ"",
    ""oth"": null,
    ""p"": null,
    ""q"": null,
    ""qi"": null,
    ""use"": null,
    ""x"": null,
    ""x5c"": [],
    ""x5t"": null,
    ""x5tS256"": null,
    ""x5u"": null,
    ""y"": null,
    ""keySize"": 2048,
    ""hasPrivateKey"": false,
    ""cryptoProviderFactory"": {
        ""cryptoProviderCache"": {},
        ""customCryptoProvider"": null,
        ""cacheSignatureProviders"": true,
        ""signatureProviderObjectPoolCacheSize"": 64
    }
}";

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
                }
                return Task.CompletedTask;
            }
        };

        o.Configuration = new OpenIdConnectConfiguration()
        {
            SigningKeys =
            {
               JsonWebKey.Create(jwkString)
            }
        };

        o.MapInboundClaims = false;
    }
    );

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", (HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? "empty"); 

app.Run();
 