using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UsersApp.Services.Dish;
using UsersApp.Services.Repository;
using UsersApp.ViewModels.restaurant;
using UsersApp.ViewModels.restaurant.Dish;

namespace UsersApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IDishService _dishService;
        private readonly IRestaurantService _restaurantService;
        private readonly ILogger<CustomerController> _logger;
        public CustomerController(IDishService dishService, IRestaurantService restaurantService, ILogger<CustomerController> logger)
        {
           _dishService = dishService;
           _restaurantService = restaurantService;
           _logger = logger;
         
        }

        [HttpGet]
        public async Task<IActionResult> Index(DishFilter filter, string resetFilter = null)
        {
            try
            {
               
                    if (resetFilter == "true")
                    {
                        filter = new DishFilter(); // إعادة تعيين الفلتر
                    }

                    var result = await _dishService.GetFilteredDishesAsync(filter);

                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                        return View(new DishIndexViewModel());
                    }

                    var restaurants = await _restaurantService.GetAllRestaurantsAsync();

                    var viewModel = new DishIndexViewModel
                    {
                        PagedDishes = result.Data,
                        Filter = filter,
                        Restaurants = restaurants.Data.Select(r => new SelectListItem
                        {
                            Value = r.Id.ToString(),
                            Text = r.Name
                        })
                        

                    };

                    return View(viewModel);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dishes/Index");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل البيانات";
                return View(new DishIndexViewModel());
            }
        }
    }
}

