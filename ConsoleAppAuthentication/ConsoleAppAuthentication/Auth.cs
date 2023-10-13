using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Encodings.Web;

namespace ConsoleAppAuthentication;
public class Auth
{
    public static async Task Handler()
    {
        var sem = new SemaphoreSlim(0);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("https://localhost:5005");
        builder.Logging.ClearProviders();
        builder.Services.AddTransient<PersistedAccessToken>();

        builder.Services.AddDataProtection()
            .SetApplicationName("my_cli_app")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cli_encryption_key"
                )))
            ;
        builder.Services.AddAuthentication()
            .AddOAuth
            <OAuthOptionsWithoutSecret, OAuthHandlerWithoutSecret>
            ("default", o =>
            {
               //https://github.com/Suhut/OpenIdDictIdentityServer/blob/nextjs_x_openiddict_x_bff/OpenIdDictIdentityServer/Data/Seed/Worker.cs

                o.ClientId = "ConsoleClient"; 
                o.AuthorizationEndpoint = "http://192.168.8.109:7211/connect/authorize";
                o.TokenEndpoint = "http://192.168.8.109:7211/connect/token";
                o.CallbackPath = "/auth-cb";

                //o.Scope.Add("User.Read");
                //o.Scope.Add("openid");
                o.Scope.Add("email");
                //o.Scope.Add("profile"); 


                o.UsePkce = true;

                o.SaveTokens = true;

                o.Events.OnCreatingTicket = async ctx =>
                {
                    var pat = ctx.HttpContext.RequestServices.GetRequiredService<PersistedAccessToken>();
                    await pat.SaveAsync(ctx.AccessToken);
                    ctx.HttpContext.Response.Redirect("/success");
                };
            })
            ;

        var app = builder.Build();
        app.UseAuthentication();
        app.MapGet("/", () => Results.Challenge(new(), new List<string>() { "default" }));
        app.MapGet("/success", () =>
        {
            sem.Release();
            return "success!";
        });
        _ = app.StartAsync();

        Process.Start(new ProcessStartInfo()
        {
            FileName = "https://localhost:5005",
            UseShellExecute = true
        });

        await sem.WaitAsync();
        await app.StopAsync();
    }
}

public class OAuthHandlerWithoutSecret : OAuthHandler<OAuthOptionsWithoutSecret>
{
    public OAuthHandlerWithoutSecret(IOptionsMonitor<OAuthOptionsWithoutSecret> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
        ) : base(options, logger, encoder, clock)
    {

    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        await base.HandleRemoteAuthenticateAsync();
        return HandleRequestResult.Handle();
    }
}

public class OAuthOptionsWithoutSecret : OAuthOptions
{
    public override void Validate()
    {

    }
}