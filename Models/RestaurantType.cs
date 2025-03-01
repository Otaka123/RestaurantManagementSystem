namespace UsersApp.Models
{
    public class RestaurantType
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    }
}
