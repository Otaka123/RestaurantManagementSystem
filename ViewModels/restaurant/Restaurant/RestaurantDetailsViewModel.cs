using System.ComponentModel.DataAnnotations;
using UsersApp.Models;
using UsersApp.Resourses;
using UsersApp.ViewModels.restaurant.Review;
using UsersApp.ViewModels.Restaurant;
using UsersApp.ViewModels.Restaurant.SelectionViewModel;

namespace UsersApp.ViewModels.restaurant
{
    public class RestaurantDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureUrl { get; set; }
        public string RestaurantName { get; set; }
        public ICollection<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public double AverageRating { get; set; }
    }
}
