using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.Auth;
using System.IdentityModel.Tokens.Jwt;
using backend.Services;
using backend.Persistence;
using Microsoft.EntityFrameworkCore;
using backend.Application.Auth.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            using var hmac = new System.Security.Cryptography.HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password));
            if (!computedHash.SequenceEqual(user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var tokenService = new TokenService(_configuration);
            var accessToken = tokenService.GenerateAccessToken(model.Username);
            var refreshToken = tokenService.GenerateRefreshToken(model.Username);
            var now = DateTime.UtcNow;
            var oldTokens = _context.RefreshTokens
                .Where(r => r.Username == user.Username && !r.IsRevoked && r.Expires > now);
            _context.RefreshTokens.RemoveRange(oldTokens);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            SetRefreshTokenCookie(refreshToken.Token);
            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken) });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var token = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized("No refresh token found.");
            var oldToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);
            if (oldToken == null || oldToken.IsRevoked || oldToken.Expires <= DateTime.UtcNow)
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Email already in use.");
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return BadRequest("Username already taken.");

            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password))
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };
            var creds = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync
            (
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(creds),
                new AuthenticationProperties { IsPersistent = true }
            );
            return Ok(new { message = "Registration successful." });
        }
    }
}