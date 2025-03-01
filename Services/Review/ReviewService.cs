using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.Services.Repository;
using UsersApp.ViewModels.restaurant.Review;

namespace UsersApp.Services.Review
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(  // constructor should match the class name.
           IUnitOfWork unitOfWork,
           ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        private Reviews ToEntity(ReviewViewModel model)
        {
            if (model == null)
                return null;

            return new Reviews
            {
                Dishid=model.DishId,
                
                CustomerName = model.CustomerName,
                RestaurantId = model.RestaurantId, // لأن RestaurantId في ViewModel `nullable`
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.UtcNow
            };
        }
        public async Task<Result<Guid>> AddReviewAsync(ReviewViewModel model)
        {
            try
            {
               var Havereview=_unitOfWork.Reviews.Find(condition:(r=>r.Dishid == model.DishId));
                if(Havereview.Any())
                {
                    return Result<Guid>.Failure("You already reviewed this dish");

                }
                var review = ToEntity(model);
                if (review == null)
                {
                    _logger.LogInformation("Failed to add review model must have value");

                    return Result<Guid>.Failure("Failed to add review model must have value");

                }
                await _unitOfWork.Reviews.AddAsync(review);

                var saveResult = await _unitOfWork.CompleteAsync();
                if (saveResult > 0)
                {
                    _logger.LogInformation("Review added successfully.");
                    return Result<Guid>.CreateSuccess(review.Id, "Review added successfully.");
                }

                _logger.LogWarning("No changes saved ");
                return Result<Guid>.Failure("لم يتم حفظ التغييرات");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review");
                return Result<Guid>.Failure("Failed to add review");
            }
        }

        public async Task<Result<IEnumerable<ReviewViewModel>>> GetReviewsByRestaurantIdAsync(Guid restaurantId)
        {
            try
            {
                // الحصول على التقييمات من الـ repository مع تضمين Customer
                var reviews = await _unitOfWork.Reviews.GetWithIncludes()
                    .Where(r => r.RestaurantId == restaurantId).ToListAsync();

                // تحويل التقييمات إلى ReviewViewModel باستخدام AutoMapper أو الطريقة اليدوية
                var viewModels = reviews.Select(r => ToViewModel(r)).ToList();

                // إعادة النتيجة بنجاح
                return Result<IEnumerable<ReviewViewModel>>.CreateSuccess(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for restaurant");
                return Result<IEnumerable<ReviewViewModel>>.Failure("Failed to get reviews");
            }
        }
        private ReviewViewModel ToViewModel(Reviews entity)
        {
            if (entity == null)
                return null;

            return new ReviewViewModel
            {
                Id = entity.Id,
                CustomerName = entity.CustomerName,
                RestaurantId = entity.RestaurantId, // تحويل من `Guid` إلى `Guid?`
                Rating = entity.Rating,
                Comment = entity.Comment,
                CreatedAt = entity.CreatedAt
            };
        }


        public async Task<Result<IEnumerable<ReviewViewModel>>> GetReviewsByDishAsync(Guid dishId)
        {
            try
            {
                var reviewsData = await _unitOfWork.Reviews.GetWithIncludes()
                    .Where(r => r.Dishid == dishId)   // تأكد من أن اسم الخاصية صحيح (DishId وليس Dishid)
                    .ToListAsync();

                var reviews = reviewsData.Select(r => ToViewModel(r));

                return Result<IEnumerable<ReviewViewModel>>.CreateSuccess(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for dish");
                return Result<IEnumerable<ReviewViewModel>>.Failure("Failed to get reviews");
            }
        }
        public async Task<Result<double>> GetAverageRatingForRestaurantAsync(Guid restaurantId)
        {
            try
            {
                var averageRating = await _unitOfWork.Reviews.Get()
                    .Where(r => r.RestaurantId == restaurantId)
                    .AverageAsync(r => r.Rating);

                return Result<double>.CreateSuccess(averageRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for restaurant");
                return Result<double>.Failure("Failed to calculate average rating");
            }
        }

        public async Task<Result<double>> GetAverageRatingForDishAsync(Guid dishId)
        {
            try
            {
                var averageRating = await _unitOfWork.Reviews.Get()
                    .Where(r => r.Dishid == dishId)
                    .AverageAsync(r => r.Rating);

                return Result<double>.CreateSuccess(averageRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for dish");
                return Result<double>.Failure("Failed to calculate average rating");
            }
        }
    }

}
