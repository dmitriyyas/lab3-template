using AuthService.Dto;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
public class AuthController : Controller
{
    [HttpGet("/authorize")]
    public IActionResult Authorize(string response_type, string client_id, string redirect_uri, string scope, string state)
    {
        if (response_type != "code")
            return BadRequest();

        return Content($@"
            <html>
            <body>
                <form method='post' action='/login'>
                    <input type='hidden' name='redirect_uri' value='{redirect_uri}' />
                    <input type='hidden' name='client_id' value='{client_id}' />
                    <input type='hidden' name='state' value='{state}' />
                    <input type='hidden' name='scope' value='{scope}' />
                    <label>Username: <input name='username' /></label><br/>
                    <label>Password: <input name='password' type='password' /></label><br/>
                    <button type='submit'>Login</button>
                </form>
            </body>
            </html>", "text/html"); // поменять на редирект на страницу логина
    }

    [HttpPost("/login")]
    public IActionResult Login([FromBody] AuthDataDto authData)
    {
        var user = UserStore.Validate(username, password);
        if (user == null)
            return Unauthorized();

        var code = Guid.NewGuid().ToString("N");
        CodeStore.Save(code, user, client_id, scope);

        return Redirect($"{redirect_uri}?code={code}&state={state}");
    }

    [HttpPost("/token")]
    public IActionResult Token([FromForm] string code, [FromForm] string client_id)
    {
        var data = CodeStore.Get(code);
        if (data == null) return Unauthorized();

        var scope = data.Scope;
        var user = data.User;

        var handler = new JwtSecurityTokenHandler();
        var rsaKey = KeyStore.GetKey();
        var creds = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id)
        };

        if (scope.Contains("profile"))
            claims.Add(new(JwtRegisteredClaimNames.Name, user.Name));
        if (scope.Contains("email"))
            claims.Add(new(JwtRegisteredClaimNames.Email, user.Email));

        var token = new JwtSecurityToken(
            issuer: "https://localhost:5001",
            audience: client_id,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );
        token.Header["kid"] = KeyStore.KeyId;

        return Ok(new
        {
            id_token = handler.WriteToken(token),
            token_type = "Bearer"
        });
    }

    [HttpGet("/userinfo")]
    public IActionResult UserInfo([FromHeader(Name = "Authorization")] string authHeader)
    {
        if (authHeader == null || !authHeader.StartsWith("Bearer ")) return Unauthorized();

        var token = authHeader["Bearer ".Length..];
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(new
        {
            sub = claims.GetValueOrDefault("sub"),
            name = claims.GetValueOrDefault("name"),
            email = claims.GetValueOrDefault("email")
        });
    }
}