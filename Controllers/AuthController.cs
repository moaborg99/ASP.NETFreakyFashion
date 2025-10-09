using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FreakyFashion.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FreakyFashion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public AuthController(IConfiguration config) => _config = config;

    // POST /api/auth/login
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest dto)
    {
        if (!IsValidUser(dto.username, dto.password))
            return Unauthorized(new { error = "Invalid username or password." });

        var token = CreateToken(dto.username);
        var expires = int.Parse(_config["Jwt:ExpiresInSeconds"] ?? "3600");
        return Ok(new LoginResponse(token, "Bearer", expires));
    }

    private bool IsValidUser(string username, string password)
        => username == "admin" && password == "password123"; // minimal för kursen

    private string CreateToken(string username)
    {
        var cfg = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username)
            // ev. roller senare: new Claim(ClaimTypes.Role, "Admin")
        };

        var expires = DateTime.UtcNow.AddSeconds(int.Parse(cfg["ExpiresInSeconds"] ?? "3600"));

        var token = new JwtSecurityToken(
            issuer: cfg["Issuer"],
            audience: cfg["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
