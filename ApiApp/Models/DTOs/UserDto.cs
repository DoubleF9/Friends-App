using System.ComponentModel.DataAnnotations;

namespace ApiApp.Models.DTOs
{
    public class UserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
