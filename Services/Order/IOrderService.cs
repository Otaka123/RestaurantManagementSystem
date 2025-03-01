using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.ViewModels.Cart;
using UsersApp.ViewModels;

namespace UsersApp.Services.Order
{
    public interface IOrderService
    {
        Task<Result<OrderViewModel>> GetOrderDetailsAsync(int shoppingCartId);
        Task<Result<UsersApp.Models.Order>> CreateOrderAsync(OrderViewModel orderVm);
        //Task<Result<OrderViewModel>> CreateOrderAsync(CheckoutViewModel model);
        //Task<IEnumerable<UsersApp.Models.Order>> UserOrders(bool getAll = false);
        ////Task ChangeOrderStatus(UpdateOrderStatusModel data);
        //Task TogglePaymentStatus(Guid orderId);
        //Task<UsersApp.Models.Order?> GetOrderById(Guid id);
        //Task<IEnumerable<OrderStatus>> GetOrderStatuses();
    }
}
