using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend.Controllers
{
    [Authorize]
    public class AuthController : BaseApiController
    {
        private readonly IConfiguration _configuration;

        public AuthController(IMediator mediator, IConfiguration configuration) : base(mediator)
        {
            _configuration = configuration;    
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model is { Username: "demo", Password: "demo" })
            {
                var token = GenerateAccessToken(model.Username);
                return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return Unauthorized("Invalid credentials");
        }

        private JwtSecurityToken GenerateAccessToken(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
            };

            var secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("SecretKey is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds);

            return token;
        }
    }
}