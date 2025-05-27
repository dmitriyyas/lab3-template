using AuthService.Dto;
using AuthService.Models;
using AuthService.Services;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("/")]
public class AuthController(UsersContext context) : Controller
{
    private readonly UsersContext _context = context;
    
    [HttpGet("api/v1/authorize")]
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

    [HttpPost("api/v1/login")]
    public async Task<IActionResult> Login([FromBody] AuthDataDto authData)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == authData.Username && u.PasswordHash == authData.Password);
        if (user is null)
            return Unauthorized();

        var code = Guid.NewGuid().ToString("N");
        CodeStore.Save(code, user, authData.ClientId, authData.Scope);

        Console.WriteLine(code);

        var redirectUrl = $"{authData.RedirectUri}?code={code}&state={authData.State}";
        //Response.Headers.Location = redirectUrl;

        //return StatusCode(302);
        return Ok(redirectUrl);
    }
    
    [HttpPost("api/v1/users")]
    //[Authorize]
    public async Task<IActionResult> Register([FromBody] UserCreateDto userCreate)
    {
        //if (!HttpContext.IsAdminRequest())
        //    return Forbid();

        var existedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userCreate.Username || u.Phone == userCreate.Phone || u.Email == userCreate.Email);

        if (existedUser is not null)
            return Conflict("User with such username or phone or email already exists.");

        var user = new User
        {
            Username = userCreate.Username,
            PasswordHash = userCreate.Password,
            Name = userCreate.Name,
            Surname = userCreate.Surname,
            Fathername = userCreate.Fathername,
            Phone = userCreate.Phone,
            Email = userCreate.Email,
            Role = "USER"
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("api/v1/me")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        var username = HttpContext.GetUsername();

        var user = await _context.Users.FirstAsync(u => u.Username == username);

        return Ok(ToDto(user));
    }

    [HttpPost("api/v1/token")]
    public IActionResult GetToken([FromBody] TokenRequestDto tokenRequest)
    {
        var data = CodeStore.Get(tokenRequest.Code);
        if (data is null) 
            return Unauthorized();

        var scope = data.Scope;
        var user = data.User;

        var handler = new JwtSecurityTokenHandler();
        var rsaKey = KeyStore.GetKey();
        var creds = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("Username", user.Username),
            new("Role", user.Role)
        };

        if (scope.Contains("profile"))
            claims.Add(new(JwtRegisteredClaimNames.Name, user.Name));
        if (scope.Contains("email"))
            claims.Add(new(JwtRegisteredClaimNames.Email, user.Email));

        var token = new JwtSecurityToken(
            issuer: "Issuer",
            audience: tokenRequest.ClientId,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(30),
            signingCredentials: creds
        );
        token.Header["kid"] = KeyStore.KeyId;

        return Ok(handler.WriteToken(token));
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto(id: user.Id,
            username: user.Username,
            name: user.Name,
            surname: user.Surname,
            fathername: user.Fathername,
            phone: user.Phone,
            email: user.Email,
            role: user.Role);
    }
}