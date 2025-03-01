using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using UsersApp.Models;

namespace UsersApp.ViewModels.Admin.Restaurants
{
    public class AddRestaurantViewModel
    {
        [Required]
        [MaxLength(100)]
        public string RestaurantName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string Location { get; set; }

        [Required]
        public string OwnerId { get; set; }

      

    }
}
