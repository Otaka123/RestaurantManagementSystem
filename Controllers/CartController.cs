using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UsersApp.Common.Message;
using UsersApp.Common.Results;
using UsersApp.Hubs;
using UsersApp.Models;
using UsersApp.Services.Cart;
using UsersApp.Services.Order;
using UsersApp.ViewModels;
using UsersApp.ViewModels.Cart;

namespace UsersApp.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IHubContext<CartHub> _hubContext;

        public CartController(
            ICartService cartService,
            IOrderService orderService,
            IHubContext<CartHub> hubContext
           )
        {
            _cartService  = cartService;
            _orderService = orderService;
            _hubContext = hubContext;
        }
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var result = await _cartService.GetCartviewModelAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new CartViewModel());
            }
            return View(result.Data);
        }
        private void SetTempMessage(string message, MessageType type)
        {
            TempData[type == MessageType.Success ? "SuccessMessage" : "ErrorMessage"] = message;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(Guid dishId, int quantity = 1)
        {
            //    var result = await _cartService.AddOrUpdateItemAsync(dishId, quantity);
            //    if (!result.Success)
            //        TempData["Error"] = result.Message;

            //    return RedirectToAction(nameof(GetUserCart));

            //}


            var result = await _cartService.AddItemAsync(dishId, quantity);
            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCartCount", result.Data);

            }
            //else
            //{
            //    RedirectToAction("Login", "Account");
            //}

            return RedirectToAction("Index");
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Checkout(OrderViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);
        //    if(model.Id )
        //    var result = await _orderService.CreateOrderAsync(model);
        //    if (!result.Success)
        //    {
        //        TempData["Error"] = result.Message;
        //        return View(model);
        //    }

        //    return RedirectToAction("OrderConfirmation", new { orderId = result.Data.Id });
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Checkout(CheckoutViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var result = await _orderService.CreateOrderAsync(model);
        //    if (!result.Success)
        //    {
        //        TempData["Error"] = result.Message;
        //        return View(model);
        //    }

        //    return RedirectToAction("OrderConfirmation", new { orderId = result.Data.Id });
        //}
        //private readonly ICartService _cartRepo;
        //public CartController(ICartService cartRepo)
        //{
        //    _cartRepo = cartRepo;
        //}
        //public IActionResult Index()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public async Task<IActionResult> AddItem(Guid dishid, int qty = 1, int redirect = 0,string geustid="")
        //{
        //    var cartCount =  _cartRepo.AddItemAsync(dishid, qty, geustid).Result.Data;

        //    if (redirect == 0)
        //        return Json(new { cartCount = cartCount }); // 🔹 إرجاع العدد بصيغة JSON
        //    return RedirectToAction("GetUserCart");
        //}
        //[HttpGet]
        //public async Task<IActionResult> GetUserCart()
        //{
        //    var cart =  _cartRepo.GetOrCreateCartAsync().Result.Data;
        //    return View(cart);
        //}
        [HttpPost]
        public async Task<IActionResult> RemoveItem(Guid Dishid)
        {
            //var cartCount = await _cartService.RemoveItemAsync(Dishid);
            //return RedirectToAction("GetUserCart");
            var result = await _cartService.RemoveItemAsync(Dishid);
            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCartCount", result.Data);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            var result = await _cartService.GetCartviewModelAsync();
            return Json(new { count = result.Data?.Items.Sum(s=>s.Quantity) ?? 0 });
        }

        ////public async Task<IActionResult> GetTotalItemInCart()
        ////{
        ////    int cartItem = await _cartRepo.GetCartItemCount();
        ////    return Ok(cartItem);
        ////}

        //public IActionResult Checkout()
        //{
        //    return View();
        //}

        ////[HttpPost]
        ////public async Task<IActionResult> Checkout(CheckoutModel model)
        ////{
        ////    if (!ModelState.IsValid)
        ////        return View(model);
        ////    bool isCheckedOut = await _cartRepo.DoCheckout(model);
        ////    if (!isCheckedOut)
        ////        return RedirectToAction(nameof(OrderFailure));
        ////    return RedirectToAction(nameof(OrderSuccess));
        ////}

        //public IActionResult OrderSuccess()
        //{
        //    return View();
        //}

        //public IActionResult OrderFailure()
        //{
        //    return View();
        //}
    }
}
