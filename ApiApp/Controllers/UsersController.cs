using ApiApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        // Static variable to hold the ID of the currently logged-in user
        private static int? _currentUserId = null;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
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
        public async Task<IActionResult> Login([FromBody] User loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            user.IsLogged = true;
            await _context.SaveChangesAsync();

            _currentUserId = user.Id;

            return Ok("Login successful.");
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
        [HttpPost("saveprofile")]
        public async Task<IActionResult> SaveProfile([FromBody] Profile updatedProfile)
        {
            // Check if a user is logged in
            if (_currentUserId == null)
            {
                return Unauthorized("No user is currently logged in.");
            }

            // Get the logged-in user
            var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == _currentUserId);
            if (user == null)
            {
                return Unauthorized("You are not authorized to update this profile.");
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
