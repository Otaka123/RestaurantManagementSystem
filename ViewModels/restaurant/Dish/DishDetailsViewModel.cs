using UsersApp.Models;
using UsersApp.ViewModels.restaurant.Review;
using UsersApp.ViewModels.Restaurant.Dish;

namespace UsersApp.ViewModels.restaurant.Dish
{
    public class DishDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureUrl { get; set; }
        public string RestaurantName { get; set; }
        public double? Rating { get; set; }
        public Guid RestaurantId { get; set; }
        public string? CategoryName { get; set; }  // لعرض اسم التصنيف

        public ICollection<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public ReviewViewModel NewReview { get; set; } = new ReviewViewModel();

        // إضافة خاصية لحساب متوسط التقييم
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
    }
}
