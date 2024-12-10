using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MealPlannerBackend.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest(new { success = false, message = "Email is already in use." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Answer = BCrypt.Net.BCrypt.HashPassword(user.Answer);
            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { success = true, message = "User registered successfully." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login login)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                return Unauthorized(new { success = false, message = "Invalid email or password." });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenExpiration = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes");
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.Now.AddMinutes(tokenExpiration),
                claims: claims,
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);


            return Ok(new { success = true, username=user.Username, token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpPost("forgotpassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            
            var user = _context.Users.FirstOrDefault(
                u => u.Email == request.Email && u.SecretQuestion == request.SecretQuestion
            );

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Answer, user.Answer))
            {
                return BadRequest(new { success = false, message = "Invalid email, secret question, or answer." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Password reset successful." });
        }

    }
}
