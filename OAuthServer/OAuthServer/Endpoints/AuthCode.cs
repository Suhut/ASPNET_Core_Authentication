namespace OAuthServer.Endpoints;

public class AuthCode
{
    public string ClientId { get; set; }
    public string CodeChallenge { get; set; }
    public string CodeChallegeMethod { get; set; }
    public string RedirectUri { get; set; }
    public DateTime Expiry { get; set; } 
}
