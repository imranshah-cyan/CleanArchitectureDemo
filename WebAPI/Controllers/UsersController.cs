using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public UsersController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            await _userService.Signup(user);
            return Ok();
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var user = await _userService.Authenticate(request.Username, request.Password, request.IpAddress, request.Device);
            if (user == null)
                return Unauthorized();

            var token = GenerateJwtToken(user);

            return Ok(new AuthenticateResponse(user, token));
        }

        [HttpPost("auth/balance")]
        public async Task<IActionResult> GetBalance([FromBody] BalanceRequest request)
        {
            var username = ValidateJwtToken(request.Token);
            if (username == null)
                return Unauthorized();

            var balance = await _userService.GetBalance(username);
            return Ok(new { Balance = balance });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("username", user.Username) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string ValidateJwtToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var username = jwtToken.Claims.First(x => x.Type == "username").Value;

                return username;
            }
            catch
            {
                return null;
            }
        }
    }

    public class AuthenticateRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string IpAddress { get; set; }
        public string Device { get; set; }
    }

    public class AuthenticateResponse
    {
        public AuthenticateResponse(User user, string token)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            Token = token;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public string Token { get; }
    }

    public class BalanceRequest
    {
        public string Token { get; set; }
    }
}
