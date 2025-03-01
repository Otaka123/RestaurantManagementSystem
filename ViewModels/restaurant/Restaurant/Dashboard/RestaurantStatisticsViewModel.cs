using UsersApp.Models;
using UsersApp.ViewModels.Order;
using UsersApp.ViewModels.restaurant.Review;

namespace UsersApp.ViewModels.restaurant.Restaurant.Dashboard
{
    public class RestaurantStatisticsViewModel
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int TotalDishes { get; set; }
        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public IEnumerable<OrderSummaryViewModel> RecentOrders { get; set; } // آخر 10 طلبات
        public IEnumerable<ReviewSummaryViewModel> RecentReviews { get; set; } // آخر 10 مراجعات
    }
}
