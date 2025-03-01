using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersApp.Models
{
    public class Reviews
    {
        [Key]
        public Guid Id { get; set; }

        //[Required]
        //[ForeignKey("Customer")]
        //public string CustomerId { get; set; }

        //[InverseProperty("Reviews")]
        //public Users Customer { get; set; }
        public string ? CustomerName { get; set; }

        [Required]
        [ForeignKey("Restaurant")]
        public Guid RestaurantId { get; set; }

        [InverseProperty("Reviews")]
        public virtual Restaurant Restaurant { get; set; }

        [ForeignKey("dish")]
        public Guid? Dishid { get; set; }

        public virtual Dish dish { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } // القيم المسموحة بين 1 و 5

        [Required]
        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
