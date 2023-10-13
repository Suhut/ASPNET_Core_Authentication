using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace ConsoleAppAuthentication;

public class PersistedAccessToken
{
    private readonly IDataProtectionProvider _provider;
    public PersistedAccessToken(IDataProtectionProvider provider)
    {
        _provider = provider;
    }

    public string AccessTokenPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".my_access_token"
        );

    public Task SaveAsync(string accessToken)
    {
        var protector = _provider.CreateProtector("cli-access-token");
        return File.WriteAllTextAsync(AccessTokenPath, protector.Protect(accessToken));
    }

    public async Task<string> LoadAsync()
    {
        var protector = _provider.CreateProtector("cli-access-token");
        var content = await File.ReadAllTextAsync(AccessTokenPath);
        return protector.Unprotect(content);
    }
}