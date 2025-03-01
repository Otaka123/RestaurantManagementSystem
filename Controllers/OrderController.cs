using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UsersApp.Services.Order;
using UsersApp.ViewModels;
using UsersApp.ViewModels.Cart;

namespace UsersApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrderController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        // عرض صفحة الدفع (Checkout)
    
        // GET: Orders/Create
        [HttpGet]
        public IActionResult Checkout(int shopoingcartid)
        {
            // تهيئة نموذج الطلب مع تفاصيل فارغة
            var orderVm = _orderService.GetOrderDetailsAsync(shopoingcartid);
            return View(orderVm.Result.Data);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel orderVm)
        {
            if (!ModelState.IsValid)
                return View(orderVm);
            //bool isGuest=true;
            //if (User?.Identity != null && User.Identity.IsAuthenticated)
            //    isGuest=false;
                // نفترض هنا أن المستخدم ليس زائراً (isGuest = false)
                var result = await _orderService.CreateOrderAsync(orderVm);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "تم إنشاء الطلب بنجاح!";
                return RedirectToAction("OrderConfirmation", new { id = result.Data.Id });
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                return View(orderVm);
            }
        }

        //// مثال لتحديث حالة الطلب
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateStatus(int orderId, int statusId)
        //{
        //    var result = await _orderService.UpdateOrderStatusAsync(orderId, statusId);
        //    if (result.IsSuccess)
        //    {
        //        TempData["SuccessMessage"] = "تم تحديث حالة الطلب بنجاح.";
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = result.Error;
        //    }
        //    return RedirectToAction("Details", new { id = orderId });
        //}

        // مثال لحذف الطلب (soft delete)
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int orderId)
        //{
        //    var result = await _orderService.SoftDeleteOrderAsync(orderId);
        //    if (result.IsSuccess)
        //    {
        //        TempData["SuccessMessage"] = "تم حذف الطلب بنجاح.";
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = result.Error;
        //    }
        //    return RedirectToAction("Index");
        //}

        // عرض تفاصيل الطلب
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            // هنا يجب جلب تفاصيل الطلب من قاعدة البيانات باستخدام خدمة مخصصة.
            // مثال:
            // var order = await _orderService.GetOrderByIdAsync(id);
            // return View(order);
            ViewBag.OrderId = id;
            // عرض توضيحي فقط:
            return View();
        }

        // عرض قائمة الطلبات
        public async Task<IActionResult> Index()
        {
            // يمكنك جلب قائمة الطلبات من قاعدة البيانات وعرضها في هذه الصفحة.
            return View();
        }
    }
}

