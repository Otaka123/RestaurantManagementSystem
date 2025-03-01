using Microsoft.AspNetCore.Mvc.Rendering;
using UsersApp.ViewModels.restaurant.Dish;

namespace UsersApp.ViewModels.restaurant.Restaurant.Dashboard
{
    public class RestaurantIndexViewModel
    {
        public PagedResult<RestaurantDetailsViewModel> PagedDishes { get; set; }
        public RestaurantFilter Filter { get; set; }
        public IEnumerable<SelectListItem> Restaurants { get; set; }
        public IEnumerable<SelectListItem> SortDirections { get; set; }
    }
}
