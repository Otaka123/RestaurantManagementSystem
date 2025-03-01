using Microsoft.AspNetCore.Mvc.Rendering;
using UsersApp.ViewModels.restaurant.Dish;
using UsersApp.ViewModels.Restaurant.Dish;

namespace UsersApp.ViewModels.restaurant
{
    public class DishIndexViewModel
    {
        public PagedResult<DishDetailsViewModel> PagedDishes { get; set; }
        public DishFilter Filter { get; set; }
        public IEnumerable<SelectListItem> Restaurants { get; set; }
        public IEnumerable<SelectListItem> SortDirections { get; set; }
    }
}
