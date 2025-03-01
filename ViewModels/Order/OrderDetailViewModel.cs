namespace UsersApp.ViewModels
{
    public class OrderDetailViewModel
    {
        public Guid DishId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? DishName { get; set; }
        public Guid Restaurantid { get; set; }
    }
}
