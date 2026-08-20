using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERP.API.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ERP.API.Infrastructure.Security;

public class JwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, IEnumerable<string> permissionCodes, IEnumerable<string> roleNames)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "480");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("CompanyID", user.CompanyID.ToString()),
            new("FullName", user.FullName)
        };

        claims.AddRange(roleNames.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissionCodes.Select(p => new Claim("Permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
