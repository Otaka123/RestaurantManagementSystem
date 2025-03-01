using Microsoft.AspNetCore.Mvc;
using UsersApp.Models;
using UsersApp.Services.Dish;
using UsersApp.Services.Review;
using UsersApp.ViewModels.restaurant.Dish;
using UsersApp.ViewModels.restaurant.Review;

namespace UsersApp.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IDishService _dishService;

        public ReviewsController(IReviewService reviewService,
            IDishService dishService)
        {
            _reviewService = reviewService;
            _dishService = dishService;
        }
        [HttpPost("Dishes/Details/{id}/AddReview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Guid id, DishDetailsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // إذا كانت البيانات غير صالحة، نعيد عرض الصفحة مع الأخطاء
                //var dish = await _dishService.GetDishDetailsAsync(id);
                //var reviews = await _reviewService.GetReviewsByDishAsync(id);

                //var viewModel = new DishDetailsViewModel
                //{
                //    Id = dish.Data.Id,
                //    Name = dish.Data.Name,
                //    Description = dish.Data.Description,
                //    Price = dish.Data.Price,
                //    PictureUrl = dish.Data.PictureUrl,
                //    RestaurantName = dish.Data.RestaurantName,
                //    Reviews = reviews.Data.ToList(),
                //    NewReview = model.NewReview // نعيد النموذج مع الأخطاء
                //};
                return RedirectToAction("Details", "Dishes", new { dishId = id });
            }

            // إضافة التقييم
            model.NewReview.DishId = id;
            var result = await _reviewService.AddReviewAsync(model.NewReview);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "تمت إضافة التقييم بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", "Dishes", new { dishId = id });
        }
        //[HttpPost]
        //public async Task<IActionResult> AddReview([FromBody] ReviewViewModel model)
        //{
        //    var result = await _reviewService.AddReviewAsync(model);

        //    if (result.Success)
        //        return Ok(result.Data);

        //    return BadRequest(result.Message);
        //}

        //[HttpGet("restaurant/{restaurantId}")]
        //public async Task<IActionResult> GetReviewsByRestaurant(Guid restaurantId)
        //{
        //    var result = await _reviewService.GetReviewsByRestaurantIdAsync(restaurantId);

        //    if (result.Success)
        //        return Ok(result.Data);

        //    return BadRequest(result.Message);
        //}

        //[HttpGet("dish/{dishId}")]
        //public async Task<IActionResult> GetReviewsByDish(Guid dishId)
        //{
        //    var result = await _reviewService.GetReviewsByDishAsync(dishId);

        //    if (result.Success)
        //        return Ok(result.Data);

        //    return BadRequest(result.Message);
        //}
    }
}
