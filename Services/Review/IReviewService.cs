using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.ViewModels.restaurant.Review;

namespace UsersApp.Services.Review
{
    public interface IReviewService
    {
        Task<Result<Guid>> AddReviewAsync(ReviewViewModel model);
        //Task<Result<IEnumerable<ReviewViewModel>>> GetReviewsByRestaurantIdAsync(Guid restaurantId);
        Task<Result<IEnumerable<ReviewViewModel>>> GetReviewsByDishAsync(Guid dishId);
        Task<Result<double>> GetAverageRatingForRestaurantAsync(Guid restaurantId);
        Task<Result<double>> GetAverageRatingForDishAsync(Guid dishId);
    }
}
