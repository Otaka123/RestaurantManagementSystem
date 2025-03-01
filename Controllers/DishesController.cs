using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UsersApp.Common.Message;
using UsersApp.Middleware;
using UsersApp.Models;
using UsersApp.ViewModels.Restaurant.Dish;
using UsersApp.ViewModels;
using UsersApp.Services.Dish;
using Microsoft.AspNetCore.Mvc.Rendering;
using UsersApp.ViewModels.restaurant.Dish;

namespace UsersApp.Controllers
{
    [AllowAnonymous]

    public class DishesController : Controller
    {
        private readonly IDishService _dishService;
        private readonly ILogger<DishesController> _logger;

        public DishesController(
            IDishService dishService,
            ILogger<DishesController> logger)
        {
            _dishService = dishService;
            _logger = logger;
        }

        // عرض قائمة الأطباق الخاصة بمطعم معيّن
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(Guid id, int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _dishService.GetDishesPagedAsync(page, pageSize, id);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve dishes: {Message}", result.Message);
                SetTempMessage(result.Message, MessageType.Error);
                PagedResult<Dish> model = new PagedResult<Dish>();
                model.id = id;

                return View(model);
            }

            return View(result.Data);
        }
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetDish(Guid id, int page = 1, int pageSize = 10)
        //{
        //    page = Math.Max(1, page);
        //    pageSize = Math.Clamp(pageSize, 1, 100);

        //    var result = await _dishService.GetDishesPagedAsync(page, pageSize, id);
        //    if (!result.Success)
        //    {
        //        _logger.LogWarning("Failed to retrieve dishes: {Message}", result.Message);
        //        SetTempMessage(result.Message, MessageType.Error);
        //        PagedResult<Dish> model = new PagedResult<Dish>();
        //        model.id = id;

        //        return View(model);
        //    }

        //    return View(result.Data);
        //}
        //[HttpGet]

        //public async Task<IActionResult> GetDish(DishFilter filter)
        //{
        //    // تعيين قيم افتراضية إذا لم تُحدد
        //    if (filter.PageNumber < 1)
        //        filter.PageNumber = 1;
        //    if (filter.PageSize < 1)
        //        filter.PageSize = 10;

        //    // تحميل قائمة التصنيفات لعرضها في القائمة المنسدلة
        //    var categories = await _dishService.GetAllCategoriesAsync();
        //    ViewBag.Categories = categories.Data.ToList();

        //    var result = await _dishService.GetFilteredDishesAsync(filter);
        //    if (!result.Success)
        //    {
        //        TempData["Error"] = result.Message;
        //        return View("Error");
        //    }

        //    return View(result.Data);
        //}

        // عرض نموذج إنشاء أو تعديل طبق
        [HttpGet]
        //[ValidateOwnership]

        public async Task<IActionResult> CreateOrEdit(Guid restaurantid,Guid id)
        {
            try
            {

                if (id == Guid.Empty)
                {
                    // عملية إنشاء طبق جديد
                    ViewData["Restaurantid"] = restaurantid;

                    return View(await InitializeViewModelAsync(null));
                }

                var dishResult = await _dishService.GetDishByIdAsync(id);
                if (!dishResult.Success)
                {
                    SetTempMessage(dishResult.Message, MessageType.Error);
                    return await RedirectToIndex(restaurantid);
                }

                var model = await _dishService.MapToDishViewModelAsync(dishResult.Data);
                if (model == null)
                {
                    SetTempMessage("Failed to load dish data.", MessageType.Error);
                    return await RedirectToIndex(restaurantid);
                }
                ViewData["Restaurantid"] = model.RestaurantId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dish data for ID: {Id}", id);
                SetTempMessage("An unexpected error occurred.", MessageType.Error);
                return await RedirectToIndex(restaurantid);
            }
        }

        // معالجة عملية إنشاء أو تعديل طبق
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(CreateDishViewModel model)
        {
            if (!ModelState.IsValid)
            {

                model = await InitializeViewModelAsync(model);
                return View(model);
            }

            try
            {
                if (model.Id != Guid.Empty)
                {
                    // تحديث طبق موجود
                    var updateResult = await _dishService.UpdateDishAsync(model);
                    if (!updateResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, updateResult.Message);
                        return View(await InitializeViewModelAsync(model));
                    }
                    SetTempMessage("Dish updated successfully!", MessageType.Success);
                }
                else
                {
                    // إنشاء طبق جديد
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        SetTempMessage("You must be logged in to create a dish.", MessageType.Error);
                        return RedirectToAction("Login", "Account");
                    }

                    // يفترض أن يتم تمرير RestaurantId من الـ view أو يتم اختياره ضمن النموذج
                    var createResult = await _dishService.AddDishAsync(model);
                    if (!createResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, createResult.Message);
                        return View(await InitializeViewModelAsync(model));
                    }
                    SetTempMessage("Dish created successfully!", MessageType.Success);
                }

                return await RedirectToIndex(model.RestaurantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing dish data.");
                SetTempMessage("An unexpected error occurred.", MessageType.Error);
                return View(await InitializeViewModelAsync(model));
            }
        }

        // عملية حذف ناعم للطبق
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateOwnership]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                var dish = await _dishService.GetDishByIdAsync(id);
                var restaurant = await _dishService.GetRestaurantByDishIdAsync(id);
                if (!dish.Success)
                {
                    SetTempMessage(dish.Message, MessageType.Error);
                 
                    return RedirectToAction(nameof(Index));
                }

                var result = await _dishService.DeleteDishAsync(id);
                if (!result.Success)
                {
                    SetTempMessage(result.Message, MessageType.Error);
                    return RedirectToAction(nameof(Index));
                }

                SetTempMessage("Dish deleted successfully!", MessageType.Success);
                if (restaurant.Success)
                {
                    Guid restaurantid = restaurant.Data.Id;
                    return await RedirectToIndex(restaurantid);

                }
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish with ID: {Id}", id);
                SetTempMessage("An unexpected error occurred.", MessageType.Error);
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        //عرض تفاصيل طبق معيّن
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid dishId)
        {
            try
            {
                var result = await _dishService.GetDishDetailsAsync(dishId);
                if (!result.Success)
                {
                    SetTempMessage(result.Message, MessageType.Error);
                    return RedirectToAction(nameof(CreateOrEdit), new { id = dishId });
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading details for dish {DishId}", dishId);
                SetTempMessage("An error occurred while loading details.", MessageType.Error);
                return await RedirectToIndex(dishId);
            }
        }

        // دالة مساعدة لتحميل نموذج العرض لنموذج إنشاء/تعديل الطبق
        private async Task<CreateDishViewModel> InitializeViewModelAsync(CreateDishViewModel model )
        {
            var viewModel = model?? new CreateDishViewModel();
            // يفترض أن يعيد الـ service قائمة بالمطاعم أو خيارات أخرى لازمة لإنشاء طبق
            var categoryResult = await _dishService.GetAllCategoriesAsync();
            if (!categoryResult.Success)
            {
                // يمكنك معالجة الخطأ هنا أو إرجاع قائمة فارغة
                return viewModel;
            }

            viewModel.Dishescategories = categoryResult.Data
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();
            return viewModel;
        }

        // دالة مساعدة لعرض الرسائل عبر TempData
        private void SetTempMessage(string message, MessageType type)
        {
            TempData[type == MessageType.Success ? "SuccessMessage" : "ErrorMessage"] = message;
        }

        // دالة مساعدة لإعادة التوجيه إلى صفحة Index مع تمرير قيمة RestaurantId
        private async Task<IActionResult> RedirectToIndex(Guid restaurantid)
        {
            
            // هنا نفترض أن RestaurantId يمكن استرجاعه من الـ Claim الخاص بالمستخدم
            

            return RedirectToAction(nameof(Index), new { id = restaurantid });
        }
    }
}

