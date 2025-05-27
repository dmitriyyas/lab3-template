using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Common;

public static class HttpContextExtensions
{
    public static string GetAccessToken(this HttpContext context)
    {
        return context.Request.Headers["Authorization"].ToString(); 
    }

    private static string GetTrimmedAccessToken(this HttpContext context)
    {
        var authorizationHeader = context.GetAccessToken();
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            return authorizationHeader.Substring("Bearer ".Length).Trim();

        throw new InvalidOperationException("Access token is invalid");
    }

    private static string GetClaim(this HttpContext context, string claimType)
    {
        var token = context.GetTrimmedAccessToken();

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
            throw new InvalidOperationException("Access token is invalid");
        
        var jwtToken = handler.ReadJwtToken(token);
        var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);

        if (claim is null)
            throw new InvalidOperationException($"Claim with type '{claimType}' wasn't found");

        return claim.Value;
    }

    public static string GetUsername(this HttpContext context)
    {
        return GetClaim(context, "Username");
    }

    public static bool IsAdminRequest(this HttpContext context)
    {
        var claim = GetClaim(context, "Role");

        return claim == "ADMIN";
    }
}
