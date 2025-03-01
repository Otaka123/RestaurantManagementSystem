using Microsoft.AspNetCore.Mvc.Rendering;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UsersApp.Resourses;
using UsersApp.ViewModels.Restaurant.SelectionViewModel;

namespace UsersApp.ViewModels.Restaurant
{
    public class CreateRestaurantViewModel
    {
        public Guid Id { get; set; }=Guid.Empty;
        [Required]
        public string Name { get; set; }

        public string? ownerId { get; set; }

        public IFormFile? Picture { get; set; } // لاستقبال الملف مباشرةً
        public string? Pictureurl { get; set; } // لاستخدام Base64
        [Required]

        public string Location { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int CityId { get; set; }

        [Required]
        public int RestaurantTypeId { get; set; }
        public List<RestaurantScheduleViewModel>? Schedules { get; set; } 
        public Dictionary<int, string> localizedDays = DateService.GetLocalizedDays();


       public RestaurantSelectionViewModel? option { get; set; }

    }
}
