namespace UsersApp.ViewModels.restaurant.Restaurant.Dashboard
{
    public class FilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SearchQuery { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
    }
}
