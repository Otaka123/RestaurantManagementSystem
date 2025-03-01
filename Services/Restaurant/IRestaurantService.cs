using Microsoft.AspNetCore.Mvc.Rendering;
using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.ViewModels;
using UsersApp.ViewModels.restaurant.Restaurant.Dashboard;
using UsersApp.ViewModels.Restaurant;
using UsersApp.ViewModels.Restaurant.Dish;
using UsersApp.ViewModels.Restaurant.SelectionViewModel;

namespace UsersApp.Services.Repository
{
    public interface IRestaurantService
    {
        //Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync();
        ////Task<IEnumerable<CreateRestaurantViewModel>> GetAllRestaurantsAsync();
        //Task<Restaurant?> GetRestaurantByIdAsync(Guid id);
        //Task<Guid> AddRestaurantAsync(CreateRestaurantViewModel RestaurantDto, string ownerId);
        //Task<bool> UpdateRestaurantAsync(Guid id, UpdateRestaurantViewModel RestaurantDto);
        //Task<bool> UpdateRestaurantSchedulesAsync(Guid RestaurantId, List<RestaurantScheduleViewModel> schedules);
        //Task<bool> SoftDeleteRestaurantAsync(Guid id);
        //Task<RestaurantStatisticsViewModel?> GetRestaurantStatisticsAsync(Guid RestaurantId);
        Task<CreateRestaurantViewModel> MapToViewModelAsync(Restaurant Restaurant);
        Task<Result<Guid>> AddRestaurantAsync(CreateRestaurantViewModel RestaurantDto);
        Task<Result<Guid>> UpdateRestaurantAsync(CreateRestaurantViewModel RestaurantDto);
        Task<Result<IEnumerable<CreateRestaurantViewModel>>> GetAllRestaurantsAsync();
        Task<Result<Restaurant>> GetRestaurantByIdAsync(Guid id);
        //Task<Result<bool>> UpdateRestaurantAsync(Guid id, UpdateRestaurantViewModel RestaurantDto);
        Task<Result<RestaurantStatisticsViewModel>> GetRestaurantStatisticsAsync(Guid RestaurantId);
        Task<Result<bool>> SoftDeleteRestaurantAsync(Guid id);
        //Task AssignRestaurantClaimAsync(string userId, Guid RestaurantId);
        Task<Result<PagedResult<Restaurant>>> GetRestaurantsPagedAsync(int pageNumber, int pageSize);
        Task<Result<IEnumerable<Restaurant>>> GetPopularRestaurantsAsync();
        Task<Result<IEnumerable<City>>> GetAllCitiesAsync();
        Task<Result<IEnumerable<RestaurantType>>> GetAllRestaurantTypesAsync();
        Task<Result<IEnumerable<RestaurantSchedule>>> GenerateDefaultSchedulesAsync();
        Task<RestaurantSelectionViewModel> GetRestaurantSelectionDataAsync();
        Task<Users> GetCurrentUserAsync();
        Task<bool> ValidateOwnership(Guid RestaurantId, string userId);
        //Task<Result<Guid>> AddDishAsync(CreateDishViewModel dishDto);
        //Task<Result<Guid>> UpdateDishAsync(CreateDishViewModel dishDto);
        //Task<CreateDishViewModel> MapToDishViewModelAsync(Models.Dish dish);
        //Task<Result<bool>> DeleteDishAsync(Guid dishId);
        Task<Result<PagedResult<Restaurant>>> GetyourRestaurantsPagedAsync(int pageNumber, int pageSize, Guid oweneridd);
        Task<Result<SelectList>> GetYourRestaurants();
    }
}
