using ApiApp.Models;
using ApiApp.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
    }
}
