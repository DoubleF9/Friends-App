using ApiApp.Models;
using ApiApp.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthenticationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            try
            {
                var user = new User
                {
                    Email = userDto.Email,
                    Password = userDto.Password,
                };
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return BadRequest("Email already exists.");
                }

                user.Password = HashPassword(user.Password);

                if (user.Profile == null)
                {
                    user.Profile = new Profile
                    {
                        FirstName = "Default",
                        LastName = "User",
                        BirthDate = DateTime.Now,
                        PhotoUrl = "",
                        User = user
                    };
                }
                else
                {
                    user.Profile.User = user;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok("User and profile registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
            {
                return Unauthorized("Invalid email or password.");
            }


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvxyz123456789");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddSeconds(10000),
                Issuer = "App",
                Audience = "App",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(tokenString);
        }



        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[16];
            rng.GetBytes(saltBytes);

            var salt = Convert.ToBase64String(saltBytes);
            var saltedPassword = password + salt;

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            return salt + Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var salt = storedHash.Substring(0, 24);
            var hash = storedHash.Substring(24);

            var saltedPassword = inputPassword + salt;

            using var sha256 = SHA256.Create();
            var inputHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            return Convert.ToBase64String(inputHash) == hash;
        }
    }
}
