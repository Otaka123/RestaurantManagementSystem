using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersApp.Models
{
    public class Dish
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // تحديد الدقة
        public decimal Price { get; set; }
        public int? Rating {  get; set; }
        public string? Picture { get; set; } // ✅ إضافة الصورة كـ `byte[]`
        public bool IsDeleted { get; set; } = false;  // ✅ Soft Delete

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual DishCategory Category { get; set; }

        [Required]
        [ForeignKey("Restaurants")]
        public Guid RestaurantId { get; set; }

        public virtual Restaurant Restaurants { get; set; }

        public virtual ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();

    }
}
