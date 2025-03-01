using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels.restaurant.Review
{
    public class ReviewViewModel
    {
        public Guid? Id { get; set; }

        public string CustomerName { get; set; } 

        [Required]
        public Guid RestaurantId { get; set; }

        public Guid? DishId { get; set; }

        [Required(ErrorMessage = "التقييم مطلوب")]
        [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "التعليق مطلوب")]
        [MaxLength(500, ErrorMessage = "التعليق يجب ألا يتجاوز 500 حرف")]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
