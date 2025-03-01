using Microsoft.AspNetCore.Mvc.Rendering;
using UsersApp.Models;

namespace UsersApp.ViewModels.Restaurant.Dish
{
    public class CreateDishViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public IFormFile picture { get; set; }
        public string? urlPicture{ get; set; } // ✅ الصورة بصيغة `Base64`
        public bool IsDeleted { get; set; } = false;
        public int Categoryid {  get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = "";
        public int? Rating { get; set; }

        public List<SelectListItem> Dishescategories { get; set; } = new List<SelectListItem>();

    }
}
