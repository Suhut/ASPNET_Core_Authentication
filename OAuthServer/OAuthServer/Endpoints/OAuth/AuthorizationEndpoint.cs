using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Web;

namespace OAuthServer.Endpoints.OAuth;
public static class AuthorizationEndpoint
{
    public static IResult Handle(
        HttpRequest request,
        IDataProtectionProvider dataProtectionProvider
        )
    {
        var iss = HttpUtility.UrlEncode("https://localhost:5005");
        request.Query.TryGetValue("state", out var state);

        if(!request.Query.TryGetValue("response_type", out var responseType))
        {
            return Results.BadRequest(new
            {
                error = "invalid_request",
                state,
                iss
            });
        }

        //request.Query.TryGetValue("response_type", out var responseType);
        request.Query.TryGetValue("client_id", out var clientId);
        request.Query.TryGetValue("code_challenge", out var codeChallenge);
        request.Query.TryGetValue("code_challenge_method", out var codeChallengeMethod);
        request.Query.TryGetValue("redirect_uri", out var redirectUri);
        request.Query.TryGetValue("scope", out var scope);
        //request.Query.TryGetValue("state", out var state);

        var protector = dataProtectionProvider.CreateProtector("oauth");
        var code = new AuthCode()
        {
            ClientId=clientId,
            CodeChallenge=codeChallenge,
            CodeChallegeMethod=codeChallengeMethod,
            RedirectUri=redirectUri,
            Expiry=DateTime.Now.AddMinutes(5)
        };
        var codeString = protector.Protect(JsonSerializer.Serialize(code));

        return Results.Redirect($"{redirectUri}?code={codeString}&state={state}&iss={HttpUtility.UrlEncode("https://localhost:5005")}");
    }
}