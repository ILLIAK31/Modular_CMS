using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.Auth;
using System.IdentityModel.Tokens.Jwt;
using backend.Services;

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
                var tokenService = new TokenService(_configuration);
                var token = tokenService.GenerateAccessToken(model.Username);
                return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return Unauthorized("Invalid credentials");
        }
    }
}