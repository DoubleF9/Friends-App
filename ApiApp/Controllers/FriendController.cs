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
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FriendsController(AppDbContext context)
        {
            _context = context;
        }

        // Get all friends for the authenticated user with pagination
        [HttpGet]
        public async Task<Object> GetFriends([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            int userId = GetUserIdFromToken();
            if (userId <= 0)
                return Unauthorized("Invalid user ID in token.");

            // Get total count for pagination
            var totalCount = await _context.Friends
                .Where(f => f.UserId == userId)
                .CountAsync();


            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));


            var friends = await _context.Friends
                .Where(f => f.UserId == userId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FriendDto
                {
                    Id = f.Id,
                    FirstName = f.FirstName,
                    LastName = f.LastName,
                    PhoneNumber = f.PhoneNumber
                })
                .ToListAsync();


            return new
            {
                friends,
                totalCount,
                currentPage = page,
                pageSize,
                totalPages
            };
        }

        // Get a specific friend by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFriend(int id)
        {
            int userId = GetUserIdFromToken();
            if (userId <= 0)
                return Unauthorized("Invalid user ID in token.");

            var friend = await _context.Friends
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (friend == null)
                return NotFound("Friend not found.");

            var friendDto = new FriendDto
            {
                Id = friend.Id,
                FirstName = friend.FirstName,
                LastName = friend.LastName,
                PhoneNumber = friend.PhoneNumber
            };

            return Ok(friendDto);
        }

        // Add a new friend
        [HttpPost]
        public async Task<IActionResult> AddFriend([FromBody] FriendDto friendDto)
        {
            int userId = GetUserIdFromToken();
            if (userId <= 0)
                return Unauthorized("Invalid user ID in token.");

            var friend = new Friend
            {
                UserId = userId,
                FirstName = friendDto.FirstName,
                LastName = friendDto.LastName,
                PhoneNumber = friendDto.PhoneNumber
            };

            _context.Friends.Add(friend);
            await _context.SaveChangesAsync();
            friendDto.Id = friend.Id;
            return Ok(friendDto);
        }

        // Update an existing friend
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFriend(int id, [FromBody] FriendDto friendDto)
        {
            int userId = GetUserIdFromToken();
            if (userId <= 0)
                return Unauthorized("Invalid user ID in token.");

            var friend = await _context.Friends
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (friend == null)
                return NotFound("Friend not found.");

            friend.FirstName = friendDto.FirstName;
            friend.LastName = friendDto.LastName;
            friend.PhoneNumber = friendDto.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Friend updated successfully" });
        }

        // Delete a friend
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFriend(int id)
        {
            int userId = GetUserIdFromToken();
            if (userId <= 0)
                return Unauthorized("Invalid user ID in token.");

            var friend = await _context.Friends
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (friend == null)
                return NotFound("Friend not found.");

            _context.Friends.Remove(friend);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Friend deleted successfully" });
        }

        // Search friends
        [HttpGet("search")]
        public async Task<Object> SearchFriends([FromQuery] string searchTerm = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            int userId = GetUserIdFromToken();
            if (userId <= 0)
                return Unauthorized("Invalid user ID in token.");


            if (string.IsNullOrWhiteSpace(searchTerm))
            {

                var totalCount = await _context.Friends
                    .Where(f => f.UserId == userId)
                    .CountAsync();

                // Calculate total pages
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                

                page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

                // Get paginated data
                var friends = await _context.Friends
                    .Where(f => f.UserId == userId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FriendDto
                    {
                        Id = f.Id,
                        FirstName = f.FirstName,
                        LastName = f.LastName,
                        PhoneNumber = f.PhoneNumber
                    })
                    .ToListAsync();

                return new
                {
                    friends,
                    totalCount,
                    currentPage = page,
                    pageSize,
                    totalPages
                };
            }

            searchTerm = searchTerm.Trim().ToLower();
            var query = _context.Friends.Where(f => f.UserId == userId);

            if (searchTerm.Contains(" "))
            {
                var parts = searchTerm.Split(' ', 2);
                string firstNamePart = parts[0];
                string lastNamePart = parts[1];

                query = query.Where(f =>
                    f.FirstName.ToLower().Contains(firstNamePart) &&
                    f.LastName.ToLower().Contains(lastNamePart));
            }
            else
            {
                query = query.Where(f =>
                    f.FirstName.ToLower().Contains(searchTerm) ||
                    f.LastName.ToLower().Contains(searchTerm));
            }

            var filteredCount = await query.CountAsync();
            
            // Calculate total pages
            var totalFilteredPages = (int)Math.Ceiling(filteredCount / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalFilteredPages == 0 ? 1 : totalFilteredPages));


            var searchResults = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FriendDto
                {
                    Id = f.Id,
                    FirstName = f.FirstName,
                    LastName = f.LastName,
                    PhoneNumber = f.PhoneNumber
                })
                .ToListAsync();

            if (searchResults.Count == 0)
            {
                return Ok(new
                {
                    friends = new List<FriendDto>(),
                    message = "No friends found.",
                    totalCount = 0,
                    currentPage = page,
                    pageSize,
                    totalPages = 0
                });
            }

            return Ok(new
            {
                friends = searchResults,
                totalCount = filteredCount,
                currentPage = page,
                pageSize,
                totalPages = totalFilteredPages
            });
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return -1;

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return -1;

            return userId;
        }
    }
}
