using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.Auth;
using System.IdentityModel.Tokens.Jwt;
using backend.Services;
using backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Authorize]
    public class AuthController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public AuthController(IConfiguration configuration, AppDbContext context, IMediator mediator) : base(mediator)
        {
            _configuration = configuration;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model.Username == "demo" && model.Password == "demo")
            {
                var tokenService = new TokenService(_configuration);
                var accessToken = tokenService.GenerateAccessToken(model.Username);
                var refreshToken = tokenService.GenerateRefreshToken(model.Username);
                var oldTokens = _context.RefreshTokens
                    .Where(r => r.Username == model.Username && !r.IsRevoked && !r.IsExpired);
                _context.RefreshTokens.RemoveRange(oldTokens);
                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();
                SetRefreshTokenCookie(refreshToken.Token);
                return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken) });
            }
            return Unauthorized("Invalid credentials.");
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var token = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized("No refresh token found.");
            var oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
            if (oldToken == null || !oldToken.IsActive)
                return Unauthorized("Invalid or expired refresh token.");
            oldToken.IsRevoked = true;
            var tokenService = new TokenService(_configuration);
            var newAccessToken = tokenService.GenerateAccessToken(oldToken.Username);
            var newRefreshToken = tokenService.GenerateRefreshToken(oldToken.Username);
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();
            SetRefreshTokenCookie(newRefreshToken.Token);
            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken) });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var existing = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
                if (existing != null)
                {
                    existing.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }
                Response.Cookies.Delete("refreshToken");
            }
            return Ok(new { message = "Logged out" });
        }

        private void SetRefreshTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}