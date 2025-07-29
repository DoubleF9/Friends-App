using System.ComponentModel.DataAnnotations;
using System.Runtime;

namespace ApiApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        public Profile? Profile { get; set; }
        public bool IsLogged { get; set; }

        public ICollection<Friend> Friends { get; set; } = new List<Friend>();
    }
}
