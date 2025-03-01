using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.ViewModels.Cart;

namespace UsersApp.Services.Cart
{
    public interface ICartService
    {
        //Task<Result<CartViewModel>> GetUserCartAsync();
        //Task<Result<int>> AddOrUpdateItemAsync(Guid dishId, int quantity);
        //Task<Result<int>> RemoveItemAsync(Guid dishid);
        Task<Result<ShoppingCart>> GetCartAsync(string userId);
        Task<Result<ShoppingCart>> GetOrCreateCartAsync();
        //Task<Result<int>> AddItemAsync(Guid Dishid, int qty, string geustid);
        Task<Result<int>> AddItemAsync(Guid dishId, int qty);
        Task<Result<int>> RemoveItemAsync(Guid dishid);
        Task<Result<CartViewModel>> GetCartviewModelAsync();
        //Task<Result<ShoppingCart>> GetOrCreateCartAsync();
        //Task<Result<ShoppingCart>> GetCartAsync(string userId);
        ////Task<Result<ShoppingCart>> GetOrCreateCartAsync(string? userId, string? guestId);
        //Task<Result<int>> AddItemAsync(Guid Dishid, int qty, string geustid);
        //Task<Result<int>> RemoveItemAsync( Guid dishid);
        //Task<int> GetCartItemCount(string userId = "");
        //Task<ShoppingCart> GetCart(string userId);
        //Task<bool> DoCheckout(CheckoutViewModel model);
    }
}
