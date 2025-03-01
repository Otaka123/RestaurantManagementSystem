namespace UsersApp.ViewModels.Restaurant.Dish
{
    public class UpdateDishViewModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public IFormFile picture { get; set; }
        public string? urlPicture { get; set; } // ✅
        public bool IsDeleted { get; set; } = false;
        public int Categoryid { get; set; }
    }
}
