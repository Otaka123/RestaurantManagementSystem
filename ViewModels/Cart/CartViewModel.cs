using UsersApp.Models;

namespace UsersApp.ViewModels.Cart
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public bool IsGuestCart { get; set; }

        public List<CartItemViewModel> Items { get; init; } = new ();
        public int TotalItems { get; init; }
        public decimal GrandTotal { get; init; } 
        public OrderViewModel? Order { get; init; }
    }
}
