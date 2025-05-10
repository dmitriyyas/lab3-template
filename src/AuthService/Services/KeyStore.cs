using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthService.Services;

public static class KeyStore
{
    public static RSA Rsa = RSA.Create(2048);
    public static string KeyId = Guid.NewGuid().ToString("N");

    public static RsaSecurityKey GetKey() => new RsaSecurityKey(Rsa) { KeyId = KeyId };
}