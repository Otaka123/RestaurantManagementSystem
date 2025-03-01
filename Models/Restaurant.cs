using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersApp.Models
{
    public class Restaurant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Picture { get; set; } // ✅ تخزين الصورة كـ `byte[]`


        [Required]
        [ForeignKey("RestaurantType")]
        public int RestaurantTypeId { get; set; }
        public virtual RestaurantType RestaurantType { get; set; } // Many-to-One

        [Required]
        [ForeignKey("City")]
        public int CityId { get; set; }

        public virtual City City { get; set; } // Many-to-One

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Owner")]
        public string OwnerId { get; set; }

        [InverseProperty("OwnedRestaurants")]
        public virtual Users Owner { get; set; }

        public bool IsDeleted { get; set; } = false;  // ✅ Soft Delete

        public DateTime? DeletedOn {  get; set; }

        public virtual ICollection<RestaurantSchedule>? RestaurantSchedules { get; set; } = new List<RestaurantSchedule>();
        public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public virtual ICollection<CartDetail> Cartdetails { get; set; } = new List<CartDetail>();

        public virtual ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
    }
}
