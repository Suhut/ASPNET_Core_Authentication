using Microsoft.AspNetCore.DataProtection;
using System.Net.Http.Headers;

namespace ConsoleAppAuthentication;
public class Call
{
    public static async Task Handler()
    {
        var container = new ServiceCollection();
        container.AddDataProtection()
            .SetApplicationName("my_cli_app")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cli_encryption_key"
                )))
            ;
        container.AddTransient<PersistedAccessToken>();

        var pat = container.BuildServiceProvider().GetRequiredService<PersistedAccessToken>();
         
        var client = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", await pat.LoadAsync())
            }

        };

        var result = await client.GetAsync("http://192.168.8.109:7211/connect/userinfo");
        Console.WriteLine(await result.Content.ReadAsStringAsync());
    }
}