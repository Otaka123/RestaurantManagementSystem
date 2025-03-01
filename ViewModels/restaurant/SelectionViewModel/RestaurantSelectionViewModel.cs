using Microsoft.AspNetCore.Mvc.Rendering;
using UsersApp.Models;

namespace UsersApp.ViewModels.Restaurant.SelectionViewModel
{
    public class RestaurantSelectionViewModel
    {
        public List<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> RestaurantTypes { get; set; } = new List<SelectListItem>();
        public List<RestaurantScheduleViewModel> Schedules { get; set; } = new List<RestaurantScheduleViewModel>();
    }
}
