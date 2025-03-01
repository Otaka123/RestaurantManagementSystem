using System.ComponentModel.DataAnnotations;

namespace UsersApp.Models
{
    public class DishCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}
