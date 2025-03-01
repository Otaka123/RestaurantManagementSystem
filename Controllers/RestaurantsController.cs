using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Security.Claims;
using UsersApp.Common.Results;
using UsersApp.Middleware;
using UsersApp.Models;
using UsersApp.Services.Repository;
using UsersApp.ViewModels;
using UsersApp.ViewModels.Restaurant;
using UsersApp.ViewModels.Restaurant.SelectionViewModel;
using UsersApp.Common.Message;
using Microsoft.Identity.Client;

namespace UsersApp.Controllers
{
    [Authorize(Roles = "Owner,Admin")]
    //[AllowAnonymous]
    public class RestaurantsController : Controller
    {
        private readonly IRestaurantService _restaurantService;
        private readonly ILogger<RestaurantsController> _logger;

        public RestaurantsController(
            IRestaurantService restaurantService,
            ILogger<RestaurantsController> logger)
        {
            _restaurantService = restaurantService;
            _logger = logger;
        }

       
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(Guid ownerId, int page = 1, int pageSize = 10)
        {

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _restaurantService.GetyourRestaurantsPagedAsync(page, pageSize, ownerId);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve restaurants: {Message}", result.Message);
                SetTempMessage(result.Message, MessageType.Error);
                return View(new PagedResult<Restaurant>());
            }

            return View(result.Data);
        }

        [HttpGet]
        //[ValidateOwnership]
        public async Task<IActionResult> CreateOrEdit(Guid? id)
        {
            try
            {
                // نظرًا لأن هذه الميثود مزينة بـ [Authorize] فلا حاجة لفحص تسجيل الدخول هنا
                if (id == null || id == Guid.Empty)
                {
                    return View(await InitializeViewModelAsync(null));
                }

                var restaurantResult = await _restaurantService.GetRestaurantByIdAsync(id.Value);
                if (!restaurantResult.Success)
                {
                    SetTempMessage(restaurantResult.Message, MessageType.Error);
                    return await redirecte();
                }

                var model = await _restaurantService.MapToViewModelAsync(restaurantResult.Data);
                if (model == null)
                {
                    SetTempMessage("Failed to load restaurant data.", MessageType.Error);
                    return await redirecte();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading restaurant data for ID: {Id}", id);
                SetTempMessage("An unexpected error occurred.", MessageType.Error);
                return await redirecte();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(CreateRestaurantViewModel model)
        {
            // نظرًا لاستخدام [Authorize] فلا حاجة لفحص تسجيل الدخول هنا مرة أخرى
            if (!ModelState.IsValid)
            {
                model = await InitializeViewModelAsync(model);
                return View(model);
            }

            try
            {
                if (model.Id != Guid.Empty)
                {
                    var updateResult = await _restaurantService.UpdateRestaurantAsync(model);
                    if (!updateResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, updateResult.Message);
                        return View(await InitializeViewModelAsync(model));
                    }
                    SetTempMessage("Restaurant updated successfully!", MessageType.Success);
                }
                else
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        SetTempMessage("You must be logged in to create a restaurant.", MessageType.Error);
                        return RedirectToAction(nameof(Index));
                    }

                    model.ownerId = currentUserId;
                    var createResult = await _restaurantService.AddRestaurantAsync(model);
                    if (!createResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, createResult.Message);
                        return View(await InitializeViewModelAsync(model));
                    }
                    SetTempMessage("Restaurant created successfully!", MessageType.Success);
                }

               return await redirecte();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing restaurant data.");
                SetTempMessage("An unexpected error occurred.", MessageType.Error);
                return View(await InitializeViewModelAsync(model));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateOwnership]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
                if (!restaurant.Success)
                {
                    SetTempMessage(restaurant.Message, MessageType.Error);
                    return RedirectToAction(nameof(Index), new { ownerId = id });
                }

                var result = await _restaurantService.SoftDeleteRestaurantAsync(id);
                if (!result.Success)
                {
                    SetTempMessage(result.Message, MessageType.Error);
                    return RedirectToAction(nameof(Index), new { ownerId = id });
                }

                SetTempMessage("Restaurant deleted successfully!", MessageType.Success);
                return await redirecte();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting restaurant with ID: {Id}", id);
                SetTempMessage("An unexpected error occurred.", MessageType.Error);
                return RedirectToAction(nameof(Index), new { ownerId = id });
            }
        }

        private async Task<CreateRestaurantViewModel> InitializeViewModelAsync(CreateRestaurantViewModel model )
        {
            var viewModel = model ?? new CreateRestaurantViewModel();
            viewModel.option = await _restaurantService.GetRestaurantSelectionDataAsync();
            return viewModel;
        }

        private void SetTempMessage(string message, MessageType type)
        {
            TempData[type == MessageType.Success ? "SuccessMessage" : "ErrorMessage"] = message;
        }
        private async Task<IActionResult> redirecte ()
        {
            var ownerIdValue = Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
            return RedirectToAction(nameof(Index), new { ownerId = ownerIdValue });
        }
        //[AllowAnonymous]
        //public async Task<IActionResult> Details(Guid restaurantId)
        //{
        //    try
        //    {
        //        var result = await _restaurantService.GetRestaurantStatisticsAsync(restaurantId);
        //        if (!result.Success)
        //        {
        //            SetTempMessage(result.Message, MessageType.Error);
        //            return RedirectToAction(nameof(CreateOrEdit), new { id = restaurantId });
        //        }
        //        return View(result.Data);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading statistics for restaurant {RestaurantId}", restaurantId);
        //        SetTempMessage("An error occurred while loading statistics.", MessageType.Error);
        //        return RedirectToAction(nameof(Index));
        //    }

        //public class RestaurantsController : Controller
        //{
        //    private readonly IRestaurantService _RestaurantService;
        //    private readonly ILogger<RestaurantsController> _logger;
        //    public RestaurantsController(
        //        IRestaurantService RestaurantService,
        //        ILogger<RestaurantsController> logger)
        //    {
        //        _RestaurantService = RestaurantService;
        //        _logger = logger;
        //    }

        //    [AllowAnonymous]
        //    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        //    {
        //        if (page < 1) page = 1;
        //        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        //        var result = await _RestaurantService.GetRestaurantsPagedAsync(page, pageSize);
        //        if (!result.Success)
        //        {
        //            _logger.LogWarning("Failed to retrieve restaurants: {Message}", result.Message);
        //            SetTempMessage(result.Message);
        //            return View(new PagedResult<Restaurant>());
        //        }
        //        return View(result.Data);

        //    }

        //    [HttpGet]
        //    [Authorize(Roles = "Owner,Admin")]
        //    public async Task<IActionResult> Create(Guid id)
        //    {
        //        if (id == Guid.Empty)
        //        {
        //            // إنشاء جديد
        //            var model = await InitializeViewModelAsync();
        //            return View(model);
        //        }
        //        else
        //        {
        //            // تحرير موجود
        //            var restaurantResult = await _RestaurantService.GetRestaurantByIdAsync(id);
        //            if (!restaurantResult.Success)
        //            {
        //                SetTempMessage(restaurantResult.Message);
        //                return RedirectToAction(nameof(Index));
        //            }

        //            var model = await _RestaurantService.MapToViewModelAsync(restaurantResult.Data);
        //            if (model == null)
        //            {
        //                SetTempMessage("Failed to load restaurant data.");
        //                return RedirectToAction(nameof(Index));
        //            }

        //            //model.option = await _RestaurantService.GetRestaurantSelectionDataAsync();
        //            return View(model);
        //        }
        //    }

        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    [Authorize(Roles = "Owner,Admin")]
        //    public async Task<IActionResult> Create(CreateRestaurantViewModel model)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            model = await InitializeViewModelAsync(model);
        //            return View(model);
        //        }

        //        if (model.Id != Guid.Empty)
        //        {
        //            // تحديث المطعم الموجود
        //            var updateResult = await _RestaurantService.UpdateRestaurantAsync(model);
        //            if (!updateResult.Success)
        //            {
        //                ModelState.AddModelError(string.Empty, updateResult.Message);
        //                model = await InitializeViewModelAsync(model);
        //                return View(model);
        //            }
        //            SetTempMessage("Restaurant updated successfully!", true);
        //        }
        //        else
        //        {
        //            // إنشاء مطعم جديد
        //            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //            if (string.IsNullOrEmpty(currentUserId))
        //            {
        //                SetTempMessage("You must be logged in to create a restaurant.");
        //                return RedirectToAction(nameof(Index));
        //            }
        //            model.ownerId = currentUserId;
        //            var createResult = await _RestaurantService.AddRestaurantAsync(model);
        //            if (!createResult.Success)
        //            {
        //                ModelState.AddModelError(string.Empty, createResult.Message);
        //                model = await InitializeViewModelAsync(model);
        //                return View(model);
        //            }
        //            SetTempMessage("Restaurant created successfully!", true);
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }

        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    //[ValidateOwnership]
        //    [Authorize(Roles = "Admin")]
        //    public async Task<IActionResult> Delete(Guid id)
        //    {
        //        var restaurant = await _RestaurantService.GetRestaurantByIdAsync(id);
        //        if (!restaurant.Success)
        //        {
        //            SetTempMessage(restaurant.Message);
        //            return RedirectToAction(nameof(Index));
        //        }

        //        var result = await _RestaurantService.SoftDeleteRestaurantAsync(id);
        //        if (!result.Success)
        //        {
        //            SetTempMessage(result.Message);
        //            return RedirectToAction(nameof(Index));
        //        }

        //        SetTempMessage("Restaurant deleted successfully!", true);
        //        return RedirectToAction(nameof(Index));
        //    }
        //    private async Task<CreateRestaurantViewModel> InitializeViewModelAsync(CreateRestaurantViewModel model = null)
        //    {
        //        var viewModel = model ?? new CreateRestaurantViewModel();
        //        viewModel.option = await _RestaurantService.GetRestaurantSelectionDataAsync();
        //        return viewModel;
        //    } // GET: Restaurants/Details/5


        //    private void SetTempMessage(string message, bool isSuccess = false)
        //    {
        //        TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = message;
        //    }


        //[AllowAnonymous]
        //public async Task<IActionResult> GetStatistics(Guid restaurantId)
        //{

        //    try
        //    {


        //        var result = await _restaurantService.GetRestaurantStatisticsAsync(restaurantId);
        //        var restaurantlist = await _restaurantService.GetYourRestaurants();
        //        if (restaurantlist.Success)
        //        {
        //            ViewBag.Restaurants = restaurantlist.Data;

        //        }

        //        if (!result.Success)
        //        {

        //            return View(result.Data);
        //        }

        //        return View(result.Data);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading statistics for restaurant {RestaurantId}", restaurantId);
        //        TempData["ErrorMessage"] = "An error occurred while loading statistics";
        //        return RedirectToAction("Index", "RestaurantDashboardController");




        //    }
        //}
        [AllowAnonymous]
        public async Task<IActionResult> GetStatistics(Guid restaurantId)
        {
            try
            {
                var restaurantList = await _restaurantService.GetYourRestaurants();
                if (restaurantList.Success)
                {
                    ViewBag.Restaurants = restaurantList.Data;
                    ViewBag.SelectedRestaurantId = restaurantId; // إضافة هذا السطر
                }

                var result = await _restaurantService.GetRestaurantStatisticsAsync(restaurantId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(result.Data);
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading statistics for restaurant {RestaurantId}", restaurantId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل الإحصائيات";
                return RedirectToAction("Index", "RestaurantDashboard");
            }
        }












        //[AllowAnonymous]

        //public class RestaurantsController : Controller
        //{
        //    private readonly IRestaurantService _restaurantService;
        //    private readonly ILogger<RestaurantsController> _logger;

        //    public RestaurantsController(
        //        IRestaurantService restaurantService,
        //        ILogger<RestaurantsController> logger)
        //    {
        //        _restaurantService = restaurantService;
        //        _logger = logger;
        //    }

        //    [HttpGet]
        //    [AllowAnonymous]
        //    public async Task<IActionResult> Index(Guid Ownerid, int page = 1, int pageSize = 10)
        //    {
        //        if (!User.Identity.IsAuthenticated)
        //        {
        //            SetTempMessage("You must be logged in to perform this action.", MessageType.Error);
        //            return RedirectToAction(nameof(Index), "Home");
        //        }
        //        page = Math.Max(1, page);
        //        pageSize = Math.Clamp(pageSize, 1, 100);

        //        var result = await _restaurantService.GetyourRestaurantsPagedAsync(page, pageSize, Ownerid);
        //        if (!result.Success)
        //        {

        //            _logger.LogWarning("Failed to retrieve restaurants: {Message}", result.Message);
        //            SetTempMessage(result.Message, MessageType.Error);
        //            return View(new PagedResult<Restaurant>());
        //        }


        //        return View(result.Data);
        //    }

        //    [HttpGet]
        //    [ValidateOwnership]
        //    [Authorize(Roles = "Owner,Admin")]
        //    public async Task<IActionResult> CreateOrEdit(Guid? id)
        //    {
        //        try
        //        {
        //            if (!User.Identity.IsAuthenticated)
        //            {
        //                SetTempMessage("You must be logged in to perform this action.", MessageType.Error);
        //                return RedirectToAction(nameof(Index),"Home");
        //            }

        //            if (id == null || id == Guid.Empty)
        //            {
        //                return View(await InitializeViewModelAsync());
        //            }

        //            var restaurantResult = await _restaurantService.GetRestaurantByIdAsync(id.Value);
        //            if (!restaurantResult.Success)
        //            {
        //                SetTempMessage(restaurantResult.Message, MessageType.Error);
        //                return RedirectToAction(nameof(Index));
        //            }

        //            var model = await _restaurantService.MapToViewModelAsync(restaurantResult.Data);
        //            if (model == null)
        //            {
        //                SetTempMessage("Failed to load restaurant data.", MessageType.Error);
        //                return RedirectToAction(nameof(Index));
        //            }

        //            return View(model);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error loading restaurant data for ID: {Id}", id);
        //            SetTempMessage("An unexpected error occurred.", MessageType.Error);
        //            return RedirectToAction(nameof(Index));
        //        }
        //    }

        //    [HttpPost]
        //    [ValidateAntiForgeryToken]

        //    [Authorize(Roles = "Owner,Admin")]
        //    public async Task<IActionResult> CreateOrEdit(CreateRestaurantViewModel model)
        //    {
        //        if (!User.Identity.IsAuthenticated)
        //        {
        //            SetTempMessage("You must be logged in to perform this action.", MessageType.Error);
        //            return RedirectToAction(nameof(Index), "Home");
        //        }
        //        if (!ModelState.IsValid)
        //        {
        //            model = await InitializeViewModelAsync(model);
        //            return View(model);
        //        }

        //        try
        //        {
        //            if (model.Id != Guid.Empty)
        //            {
        //                var updateResult = await _restaurantService.UpdateRestaurantAsync(model);
        //                if (!updateResult.Success)
        //                {
        //                    ModelState.AddModelError(string.Empty, updateResult.Message);
        //                    return View(await InitializeViewModelAsync(model));
        //                }
        //                SetTempMessage("Restaurant updated successfully!", MessageType.Success);
        //            }
        //            else
        //            {
        //                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //                if (string.IsNullOrEmpty(currentUserId))
        //                {
        //                    SetTempMessage("You must be logged in to create a restaurant.", MessageType.Error);
        //                    return RedirectToAction(nameof(Index));
        //                }

        //                model.ownerId = currentUserId;
        //                var createResult = await _restaurantService.AddRestaurantAsync(model);
        //                if (!createResult.Success)
        //                {
        //                    ModelState.AddModelError(string.Empty, createResult.Message);
        //                    return View(await InitializeViewModelAsync(model));
        //                }
        //                SetTempMessage("Restaurant created successfully!", MessageType.Success);
        //            }

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error processing restaurant data.");
        //            SetTempMessage("An unexpected error occurred.", MessageType.Error);
        //            return View(await InitializeViewModelAsync(model));
        //        }
        //    }

        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    [ValidateOwnership]

        //    [Authorize(Roles = "Admin")]
        //    public async Task<IActionResult> SoftDelete(Guid id)
        //    {
        //        if (!User.Identity.IsAuthenticated)
        //        {
        //            SetTempMessage("You must be logged in to perform this action.", MessageType.Error);
        //            return RedirectToAction(nameof(Index), "Home");
        //        }
        //        try
        //        {
        //            var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
        //            if (!restaurant.Success)
        //            {
        //                SetTempMessage(restaurant.Message, MessageType.Error);
        //                return RedirectToAction(nameof(Index));
        //            }

        //            var result = await _restaurantService.SoftDeleteRestaurantAsync(id);
        //            if (!result.Success)
        //            {
        //                SetTempMessage(result.Message, MessageType.Error);
        //                return RedirectToAction(nameof(Index));
        //            }

        //            SetTempMessage("Restaurant deleted successfully!", MessageType.Success);
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error deleting restaurant with ID: {Id}", id);
        //            SetTempMessage("An unexpected error occurred.", MessageType.Error);
        //            return RedirectToAction(nameof(Index));
        //        }
        //    }

        //    private async Task<CreateRestaurantViewModel> InitializeViewModelAsync(CreateRestaurantViewModel model = null)
        //    {
        //        var viewModel = model ?? new CreateRestaurantViewModel();
        //        viewModel.option = await _restaurantService.GetRestaurantSelectionDataAsync();
        //        return viewModel;
        //    }

        //    private void SetTempMessage(string message, MessageType type)
        //    {
        //        TempData[type == MessageType.Success ? "SuccessMessage" : "ErrorMessage"] = message;
        //    }

        //    [AllowAnonymous]
        //    public async Task<IActionResult> Details(Guid restaurantId)
        //    {
        //        if (!User.Identity.IsAuthenticated)
        //        {
        //            SetTempMessage("You must be logged in to perform this action.", MessageType.Error);
        //            return RedirectToAction(nameof(Index), "Home");
        //        }
        //        try
        //        {
        //            var result = await _restaurantService.GetRestaurantStatisticsAsync(restaurantId);
        //            if (!result.Success)
        //            {
        //                SetTempMessage(result.Message, MessageType.Error);
        //                return RedirectToAction(nameof(CreateOrEdit), new { id = restaurantId });
        //            }
        //            return View(result.Data);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error loading statistics for restaurant {RestaurantId}", restaurantId);
        //            SetTempMessage("An error occurred while loading statistics.", MessageType.Error);
        //            return RedirectToAction(nameof(Index));
        //        }
        //    }

        //private enum MessageType
        //{
        //    Success,
        //    Error
        //}


    }
}

