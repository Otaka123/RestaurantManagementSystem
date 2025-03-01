using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UsersApp.Validators;

namespace UsersApp.Models
{
    public class Users : IdentityUser
    {

     
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? DOB { get; set; }  // يمكن أن يكون فارغًا

        [Required]
        public DateTime Createtime { get; set; } = DateTime.UtcNow;

        public string? ProfilePictureUrl { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? Gender { get; set; }  // القيم الممكنة: "Male", "Female", "Other"

        [Phone]
        [MaxLength(15)]
        public string? Phone { get; set; }

        public string? UserType { get; set; }

        [NotMapped]
        public string? FullName => $"{FirstName} {LastName}";
        public virtual ICollection<Restaurant> OwnedRestaurants { get; set; } = new List<Restaurant>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();

    }
}
