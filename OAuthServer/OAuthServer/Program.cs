using OAuthServer;
using OAuthServer.Endpoints;
using OAuthServer.Endpoints.OAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", o =>
    {
        o.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<DevKeys>(); 

var app = builder.Build();

app.MapGet("/login", GetLogin.Handler);
app.MapPost("/login", Login.Handler);
app.MapGet("/oauth/authorize", AuthorizationEndpoint.Handle).RequireAuthorization();
app.MapPost("/oauth/token", TokenEndpoint.Handle);

app.Run();
 