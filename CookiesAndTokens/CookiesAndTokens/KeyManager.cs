using System.Security.Cryptography;

namespace CookiesAndTokens;
public class KeyManager
{
    public KeyManager()
    {
        RsaKey = RSA.Create();
        if(File.Exists("key"))
        {
            RsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
        }
        else
        {
            var privateKey = RsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes("key", privateKey);
        }
    }

    public RSA RsaKey { get; }
}