using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiApp.Models
{
    public class Profile
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhotoUrl { get; set; }

        public User? User { get; set; }
    }
}
