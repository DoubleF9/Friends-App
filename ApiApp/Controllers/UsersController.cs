using ApiApp.Models;
using ApiApp.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;


        public UsersController(AppDbContext context)
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

        [HttpGet("getprofile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            return Ok(new
            {
                profile.FirstName,
                profile.LastName,
                profile.BirthDate,
                profile.PhotoUrl
            });
        }

        // Save profile data
        [Authorize]
        [HttpPost("saveprofile")]
        public async Task<IActionResult> SaveProfile([FromBody] ProfileDto updatedProfile)
        {
            // Extract the user ID from the JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Invalid token. User ID not found.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            DateTime currentTime = DateTime.UtcNow;
            Claim? exp = User.Claims.FirstOrDefault(c => c.Type == "exp");
            //DateTime expireTime = new DateTime(exp.Value.ToString());
            DateTimeOffset timeOffset = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp?.Value));
            if (DateTime.Now > timeOffset.ToLocalTime())
            {
                return Unauthorized("Unauthorized user");
            }

            // Get the logged-in user
            var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the profile exists
            if (user.Profile == null)
            {
                return NotFound("Profile not found.");
            }

            // Update the profile
            user.Profile.FirstName = updatedProfile.FirstName;
            user.Profile.LastName = updatedProfile.LastName;
            user.Profile.BirthDate = updatedProfile.BirthDate;
            user.Profile.PhotoUrl = updatedProfile.PhotoUrl;

            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }

        private string HashPassword(string password)
        {
            // Generate a random salt of fixed length (16 bytes)
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[16]; // 16 bytes = 128 bits
            rng.GetBytes(saltBytes);

            // Convert the salt to a Base64 string
            var salt = Convert.ToBase64String(saltBytes);

            // Combine the password and salt
            var saltedPassword = password + salt;

            // Hash the salted password
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            // Combine the salt and hash into a single string (salt is always 16 bytes)
            return salt + Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            // Extract the salt (first 24 characters, since Base64-encoded 16 bytes = 24 characters)
            var salt = storedHash.Substring(0, 24);

            // Extract the hash (remaining part of the stored hash)
            var hash = storedHash.Substring(24);

            // Combine the input password with the extracted salt
            var saltedPassword = inputPassword + salt;

            // Hash the salted password
            using var sha256 = SHA256.Create();
            var inputHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            // Compare the computed hash with the stored hash
            return Convert.ToBase64String(inputHash) == hash;
        }

    }
}
