using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UsersApp.Services.Repository;
using UsersApp.ViewModels.restaurant.Restaurant.Dashboard;

namespace UsersApp.Controllers
{
    [Authorize]
    public class RestaurantDashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IRestaurantService _restaurantService;
        private readonly ILogger<RestaurantDashboardController> _logger;

        public RestaurantDashboardController(
            IRestaurantService restaurantService,
            IUnitOfWork unitOfWork,
            ILogger<RestaurantDashboardController> logger)
        {
            _restaurantService = restaurantService;
            _logger = logger;
            _unitOfWork= unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var restaurantlist= await _restaurantService.GetYourRestaurants();
            if (restaurantlist.Success) {
                ViewBag.Restaurants = restaurantlist.Data;

            }
            
            return View();
        }
        //[HttpGet("api/restaurant/{restaurantId}/stats")]
        //public async Task<IActionResult> GetRestaurantStats(Guid restaurantId, [FromQuery] FilterViewModel filter)

        ////public async Task<IActionResult> Dashboard(Guid restaurantId ,[FromQuery] FilterViewModel filter)
        //{
        //    try
        //    {
        //        var result = await _restaurantService.GetRestaurantStatisticsAsync(restaurantId, filter);

        //        if (!result.Success)
        //        {
        //            TempData["ErrorMessage"] = result.Message;
        //            return RedirectToAction("Index", "Restaurants");
        //        }

        //        return View(result.Data);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading dashboard for restaurant {RestaurantId}", restaurantId);
        //        TempData["ErrorMessage"] = "Error loading dashboard";
        //        return RedirectToAction("Index", "Restaurants");
        //    }
        //}
        [HttpPost("export-orders")]
        public async Task<IActionResult> ExportOrders(Guid restaurantId)
        {
            var orders = await _unitOfWork.OrderDetails
               .GetWithIncludes(d=>d.Order).Where(o => o.Restaurantid == restaurantId)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Orders");
            worksheet.Cell(1, 1).Value = "Order ID";
            worksheet.Cell(1, 2).Value = "Total Amount";
            worksheet.Cell(1, 3).Value = "Status";
            worksheet.Cell(1, 4).Value = "Order Date";

            for (int i = 0; i < orders.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = orders[i].Id;
                worksheet.Cell(i + 2, 2).Value = orders[i].Order.totalprice;
                worksheet.Cell(i + 2, 3).Value = orders[i].Order.OrderStatus.StatusName;
                worksheet.Cell(i + 2, 4).Value = orders[i].Order.CreateDate.ToString("dd/MM/yyyy");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
        }
    }
}
