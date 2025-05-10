using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers;

[ApiController]
public class DiscoveryController : ControllerBase
{
    [HttpGet("/.well-known/openid-configuration")]
    public IActionResult GetConfig()
    {
        var baseUrl = "https://localhost:5002";
        return Ok(new
        {
            issuer = baseUrl,
            authorization_endpoint = $"{baseUrl}/authorize",
            token_endpoint = $"{baseUrl}/token",
            userinfo_endpoint = $"{baseUrl}/userinfo",
            jwks_uri = $"{baseUrl}/.well-known/jwks.json",
            response_types_supported = new[] { "code" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" },
            scopes_supported = new[] { "openid", "profile", "email" }
        });
    }

    [HttpGet("/.well-known/jwks.json")]
    public IActionResult GetJwks()
    {
        var rsa = KeyStore.Rsa.ExportParameters(false);
        return Ok(new
        {
            keys = new[] {
                new {
                    kty = "RSA",
                    use = "sig",
                    kid = KeyStore.KeyId,
                    e = Base64UrlEncoder.Encode(rsa.Exponent),
                    n = Base64UrlEncoder.Encode(rsa.Modulus),
                    alg = "RS256"
                }
            }
        });
    }
}