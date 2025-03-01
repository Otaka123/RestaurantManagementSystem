using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels.Cart
{
    public class CartItemViewModel
    {
        //public string? Userid {  get; set; }
        //public string? Guestid { get; set; }
        public int Id { get; set; }
        public Guid Dishid { get; init; }
        public string DishName { get; set; }

        public Guid Restaurantid { get; init; }
        public string RestaurantName { get; set; }
        public string ImageUrl { get; set; }
        public string Description {  get; set; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal TotalPrice { get; init; }
    }
}
