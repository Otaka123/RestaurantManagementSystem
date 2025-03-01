namespace UsersApp.ViewModels.restaurant.Dish
{
    public class DishFilter : PagedResult<UsersApp.Models.Dish>
    {
        public Guid? RestaurantId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        // خاصية الترتيب، مثل "price", "rating", "name"
        public string SortBy { get; set; } = "name";
        // اتجاه الترتيب: "asc" للتصاعدي و "desc" للتنازلي. القيمة الافتراضية ستكون التصاعدي.
        public string SortDirection { get; set; }
        public string SearchTerm {  get; set; }
        public int? RestaurantTypeId { get; set; }
        public int? CategoryId { get; set; }

    }
}
