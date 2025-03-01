using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.ViewModels;
using UsersApp.ViewModels.restaurant.Dish;
using UsersApp.ViewModels.Restaurant.Dish;

namespace UsersApp.Services.Dish
{
    public interface IDishService
    {
        Task<Result<Guid>> AddDishAsync(CreateDishViewModel dishDto);
        Task<Result<Guid>> UpdateDishAsync(CreateDishViewModel dishDto);
        Task<CreateDishViewModel> MapToDishViewModelAsync(UsersApp.Models.Dish dish);
        Task<Result<bool>> DeleteDishAsync(Guid dishId);
        Task<Result<PagedResult<UsersApp.Models.Dish>>> GetDishesPagedAsync(int pageNumber, int pageSize, Guid restaurantid);
        Task<Result<UsersApp.Models.Dish>> GetDishByIdAsync(Guid id);
        Task<Users> GetCurrentUserAsync();
        Task<Result<IEnumerable<DishCategory>>> GetAllCategoriesAsync();
        Task<Result<Restaurant>> GetRestaurantByDishIdAsync(Guid id);
        Task<Result<PagedResult<DishDetailsViewModel>>> GetFilteredDishesAsync(DishFilter filter);
        Task<Result<DishDetailsViewModel>> GetDishDetailsAsync(Guid id);
        Task<Result<decimal>> GetPriceAsync(Guid dishid);
    }
}
