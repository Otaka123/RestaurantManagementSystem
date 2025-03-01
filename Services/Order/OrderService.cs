using AutoMapper;
using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.Services.Order;
using UsersApp.Services.Repository;
using UsersApp.ViewModels.Cart;
using UsersApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using UsersApp.ViewModels.restaurant.Dish;
using UsersApp.ViewModels.restaurant.Review;
using System.ComponentModel.DataAnnotations;
using System.Data;
using UsersApp.Services.Cart;
using Microsoft.EntityFrameworkCore.Storage;
using UsersApp.Services.Dish;
using DocumentFormat.OpenXml.Office2010.Excel;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartService _cartService;
    private readonly IDishService _dishService;

    
    private readonly ILogger<OrderService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<Users> _userManager;

    public OrderService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OrderService> logger,
        IDishService dishService,
        ICartService cartService,
        IMapper mapper,
        UserManager<Users> userManager)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _cartService= cartService;
        _dishService= dishService;
    }




    public async Task<Result<Order>> CreateOrderAsync(OrderViewModel orderVm)
    {
        try
        {
            // التحقق من صحة البيانات المدخلة
            //var validationResult = ValidateOrderViewModel(orderVm);
            //if (!validationResult.IsValid)
            //    return Result<Order>.Failure(validationResult.ErrorMessage);

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            // إنشاء كائن الطلب مع حساب الإجمالي تلقائياً
            var order = await CreateOrderFromViewModel(orderVm);

            // حفظ الطلب في قاعدة البيانات
            var saveResult = await SaveOrderAsync(order);
            if (!saveResult.Success)
                return saveResult;

            // مسح سلة المستخدم
            var clearCartResult = await ClearCartAsync(order.UserId);
            if (!clearCartResult.Success)
                return clearCartResult;

            // تأكيد المعاملة إذا نجحت جميع العمليات
            await transaction.CommitAsync();

            _logger.LogInformation("Order {OrderId} created successfully.", order.Id);
            return Result<Order>.CreateSuccess(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Result<Order>.Failure("حدث خطأ أثناء إنشاء الطلب.");
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━ Helper Methods ━━━━━━━━━━━━━━━━━━━━━━

    //private ValidationResult ValidateOrderViewModel(OrderViewModel orderVm)
    //{
    //    if (orderVm == null)
    //        return ValidationResult.Failure("بيانات الطلب مطلوبة.");

    //    if (orderVm.OrderDetails?.Any() != true)
    //        return ValidationResult.Failure("يجب أن يحتوي الطلب على عنصر واحد على الأقل.");

    //    if (orderVm.UserId <= 0)
    //        return ValidationResult.Failure("معرّف المستخدم غير صالح.");

    //    return ValidationResult.Success();
    //}
  
    private async Task<Order> CreateOrderFromViewModel(OrderViewModel orderVm)
    {
        var orderDetails = orderVm.OrderDetails.Select(d => new OrderDetail
        {
            DishId = d.DishId,
            Quantity = d.Quantity,
            UnitPrice = GetValidUnitPrice(d.DishId, d.UnitPrice).Result,
            Restaurantid = d.Restaurantid,


        }).ToList();
        return new Order
        {
            UserId = orderVm.UserId,
            Name = orderVm.Name.Trim(),
            Email = orderVm.Email.ToLower().Trim(),
            MobileNumber = orderVm.MobileNumber?.Trim(),
            Address = orderVm.Address?.Trim(),
            PaymentMethod = orderVm.PaymentMethod,
            totalprice = orderDetails.Sum(d => d.Quantity * d.UnitPrice), // حساب الإجمالي بشكل آمن
            CreateDate = DateTime.UtcNow,
            OrderStatusId = 1,
            OrderDetail = orderDetails
        };
    }

    private async Task<decimal> GetValidUnitPrice(Guid dishId, decimal requestedPrice)
    {
        var actualPrice = await _dishService.GetPriceAsync(dishId);
        if (!actualPrice.Success)
        {
            if (actualPrice.Data != requestedPrice)
                throw new InvalidOperationException("اختلاف في أسعار الأطباء المطلوبة");
        }
        return actualPrice.Data;
    }

    private async Task<Result<Order>> SaveOrderAsync(Order order)
    {
        try
        {
            await _unitOfWork.Orders.AddAsync(order);
            var affectedRows = await _unitOfWork.CompleteAsync();

            return affectedRows > 0 && order.Id > 0
                ? Result<Order>.CreateSuccess(order)
                : Result<Order>.Failure("فشل في حفظ الطلب في قاعدة البيانات.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving order");
            return Result<Order>.Failure("حدث خطأ أثناء حفظ الطلب.");
        }
    }
    private async Task<Result<Order>> ClearCartAsync(
       string? userId,
       CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return Result<Order>.Failure("");

            var rowsAffected = await _unitOfWork.ShoppingCarts.Get()
                .Where(c => c.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
            if (rowsAffected > 0)
            {
                _logger.LogInformation("Cleared {Count} cart items for user {UserId}", rowsAffected, userId);
                return Result<Order>.CreateSuccess(null);

            }
            else
            {
                return Result<Order>.Failure("");

            }
        }
        catch (Exception ex)
        {

                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                 return Result<Order>.Failure("فشل في مسح محتويات السلة.");
        }
    }
   








    public async Task<Result<OrderViewModel>> GetOrderDetailsAsync(int shoppingCartId)
    {
        try
        {
            // 1. إصلاح شرط البحث لاستخدام المعلمة الصحيحة

            var cart = _unitOfWork.ShoppingCarts.Find(condition: (s => s.Id == shoppingCartId), includes: (c => c.CartDetails)).FirstOrDefault();





            if (cart == null)
            {
                _logger.LogWarning("سلة التسوق بالرقم {ShoppingCartId} غير موجودة.", shoppingCartId);
                return Result<OrderViewModel>.Failure("سلة التسوق غير موجودة");
            }

            if (!cart.CartDetails.Any())
            {
                _logger.LogWarning("سلة التسوق موجودة لكنها فارغة! {ShoppingCartId}", shoppingCartId);
                return Result<OrderViewModel>.Failure("سلة التسوق لا تحتوي على عناصر.");
            }

            var orderDetails = new OrderViewModel
            {
                UserId = cart.UserId,
                TotalPrice = cart.CartDetails.Sum(cd => cd.UnitPrice * cd.Quantity),
                OrderDetails = cart.CartDetails.Select(cd => new OrderDetailViewModel
                {
                    
                    DishId = cd.DishId,
                    Quantity = cd.Quantity,
                    UnitPrice = cd.UnitPrice,
                    DishName = cd.Dish.Name ?? "غير معروف",
                    Restaurantid=cd.Restaurantid,
                    
                }).ToList()
            };

            return Result<OrderViewModel>.CreateSuccess(orderDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ أثناء استرجاع تفاصيل الطلب لسلة التسوق {ShoppingCartId}", shoppingCartId);
            return Result<OrderViewModel>.Failure("فشل استرجاع تفاصيل الطلب");
        }
    }





    public async Task<Result<int>> UpdateOrderStatusAsync(
        int orderId,
        int newStatusId,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.Get()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found.", orderId);
            return Result<int>.Failure("Order not found.");
        }

       

        order.OrderStatusId = newStatusId;
        await _unitOfWork.SaveChangesAsync(false,cancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to {StatusId}.", orderId, newStatusId);
        return Result<int>.CreateSuccess(newStatusId);
    }

    public async Task<Result<int>> SoftDeleteOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.Get()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found.", orderId);
            return Result<int>.Failure("Order not found.");
        }

        order.IsDeleted = true;
        //order.DeletedDate = DateTime.UtcNow; // Add this property to your Order entity

        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Order {OrderId} marked as deleted.", orderId);
        return Result<int>.CreateSuccess(1);
    }

    //private async Task ClearCartAsync(
    //    string userId,
    //    CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrEmpty(userId))
    //        return;

    //    var rowsAffected = await _unitOfWork.ShoppingCarts.Get()
    //        .Where(c => c.UserId == userId)
    //        .ExecuteDeleteAsync(cancellationToken);

    //    _logger.LogInformation("Cleared {Count} cart items for user {UserId}", rowsAffected, userId);
    //}
    private string GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return _userManager.GetUserId(user);
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var guestId = httpContext?.Request.Cookies["GuestId"];

        if (string.IsNullOrEmpty(guestId))
        {
            guestId = Guid.NewGuid().ToString();
            httpContext?.Response.Cookies.Append("GuestId", guestId, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(7),
                HttpOnly = true
            });
        }

        return guestId;
    }
    //public async Task<Result<OrderViewModel>> CreateOrderAsync(CheckoutViewModel model)
    //{
    //    using var transaction = await _unitOfWork.BeginTransactionAsync();
    //    try
    //    {
    //        // نستخدم model.userid والذي يكون إما معرف مستخدم مسجل أو GuestId
    //        var userId = GetUserId();

    //        // استرجاع السلة الخاصة بالـ userId سواء كان مستخدم أو ضيف
    //        var cart = await _unitOfWork.ShoppingCarts
    //            .GetWithIncludes(c => c.CartDetails)
    //            .FirstOrDefaultAsync(c => c.UserId == userId);

    //        if (cart == null || cart.CartDetails == null || !cart.CartDetails.Any())
    //        {
    //            _logger.LogWarning("السلة فارغة أو غير موجودة للمستخدم: {UserId}", userId);
    //            return Result<OrderViewModel>.Failure("السلة فارغة");
    //        }

    //        // إنشاء كيان الطلب بناءً على بيانات الدفع والسلة
    //        var order = new Order
    //        {
    //            UserId = userId,
    //            totalprice = cart.CartDetails.Sum(cd => cd.UnitPrice * cd.Quantity),
    //            Name = model.Name,
    //            Email = model.Email,
    //            MobileNumber = model.MobileNumber,
    //            Address = model.Address,
    //            PaymentMethod = model.PaymentMethod,
    //            OrderStatusId = 1, // الحالة الافتراضية
    //            OrderDetail = cart.CartDetails.Select(cd => new OrderDetail
    //            {
    //                DishId = cd.DishId,
    //                Quantity = cd.Quantity,
    //                UnitPrice = cd.UnitPrice,
    //            }).ToList()
    //        };

    //        // إضافة الطلب وإزالة تفاصيل السلة
    //        await _unitOfWork.Orders.AddAsync(order);
    //        _unitOfWork.CartDetails.RemoveRange(cart.CartDetails);

    //        var saveResult = await _unitOfWork.CommitAsync();
    //        if (saveResult <= 0)
    //        {
    //            _logger.LogWarning("لم يتم حفظ التغييرات عند إنشاء الطلب للمستخدم: {UserId}", userId);
    //            await transaction.RollbackAsync();
    //            return Result<OrderViewModel>.Failure("فشل عملية إنشاء الطلب");
    //        }

    //        await transaction.CommitAsync();
    //        _logger.LogInformation("تم إنشاء الطلب بنجاح للمستخدم: {UserId}", userId);

    //        return Result<OrderViewModel>.CreateSuccess(_mapper.Map<OrderViewModel>(order), "تم إنشاء الطلب بنجاح");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "فشل عملية إنشاء الطلب");
    //        await transaction.RollbackAsync();
    //        return Result<OrderViewModel>.Failure("فشل عملية إنشاء الطلب");
    //    }
    //}

    //public async Task<Result<OrderViewModel>> CreateOrderAsync(CheckoutViewModel model)
    //{
    //    using var transaction = await _unitOfWork.BeginTransactionAsync();
    //    try
    //    {
    //        var userid= GetUserId();
    //        // استرجاع سلة المستخدم
    //        var cart = await _unitOfWork.ShoppingCarts
    //            .GetWithIncludes(c => c.CartDetails)
    //            .FirstOrDefaultAsync(c => c.UserId == userid);

    //        if (cart == null || !cart.CartDetails.Any())
    //        {
    //            _logger.LogWarning("السلة فارغة للمستخدم: {UserId}", userid);
    //            return Result<OrderViewModel>.Failure("السلة فارغة");
    //        }

    //        // إنشاء الطلب
    //        var order = new Order
    //        {
    //            UserId = userid,
    //            Name = model.Name,
    //            Email = model.Email,
    //            MobileNumber = model.MobileNumber,
    //            Address = model.Address,
    //            PaymentMethod = model.PaymentMethod,
    //            OrderStatusId = 1, // حالة الطلب الافتراضية (مثلاً: "Pending")
    //            totalprice = cart.CartDetails.Sum(cd => cd.UnitPrice * cd.Quantity),
    //            OrderDetail = cart.CartDetails.Select(cd => new OrderDetail
    //            {
    //                DishId = cd.DishId,
    //                Quantity = cd.Quantity,
    //                UnitPrice = cd.UnitPrice,
    //            }).ToList()
    //        };

    //        // إضافة الطلب إلى قاعدة البيانات
    //       await _unitOfWork.Orders.AddAsync(order);

    //        // إزالة تفاصيل السلة بعد إنشاء الطلب
    //        _unitOfWork.CartDetails.RemoveRange(cart.CartDetails);

    //        // حفظ التغييرات
    //        await transaction.CommitAsync();

    //        _logger.LogInformation("تم إنشاء الطلب بنجاح للمستخدم: {UserId}", userid);

    //        // إرجاع نموذج الطلب
    //        var orderViewModel = new OrderViewModel
    //        {
    //            Id = order.Id,
    //            UserId = order.UserId,
    //            Name = order.Name,
    //            Email = order.Email,
    //            MobileNumber = order.MobileNumber,
    //            Address = order.Address,
    //            PaymentMethod = order.PaymentMethod,
    //            TotalPrice = order.totalprice??5,
    //            OrderStatus = order.OrderStatus?.StatusName,
    //            OrderDetails = order.OrderDetail.Select(od => new OrderDetailViewModel
    //            {
    //                DishName = od.Dish?.Name,
    //                Quantity = od.Quantity,
    //                UnitPrice = od.UnitPrice,
    //                TotalPrice = od.Quantity * od.UnitPrice
    //            }).ToList()
    //        };

    //        return Result<OrderViewModel>.CreateSuccess(orderViewModel, "تم إنشاء الطلب بنجاح");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "فشل في إنشاء الطلب");
    //        await transaction.RollbackAsync();
    //        return Result<OrderViewModel>.Failure("فشل في إنشاء الطلب");
    //    }
    //}
}

    //public async Task<Result<OrderViewModel>> CreateOrderAsync(CheckoutViewModel model)
    //{
    //    using var transaction = await _unitOfWork.BeginTransactionAsync();
    //    try
    //    {
    //        var userId = model.userid;
    //        // استرجاع السلة مع التفاصيل الخاصة بها
    //        var cart = await _unitOfWork.ShoppingCarts
    //            .GetWithIncludes(c => c.CartDetails)
    //            .FirstOrDefaultAsync(c => c.UserId == userId);

    //        if (cart == null || cart.CartDetails == null || !cart.CartDetails.Any())
    //        {
    //            _logger.LogWarning("لم يتم إيجاد سلة أو السلة فارغة للمستخدم: {UserId}", userId);
    //            return Result<OrderViewModel>.Failure("السلة فارغة");
    //        }

    //        // إنشاء كيان الطلب بناءً على بيانات الدفع والسلة
    //        var order = new UsersApp.Models.Order
    //        {
    //            UserId = userId,
    //            totalprice = cart.CartDetails.Sum(cd => cd.UnitPrice * cd.Quantity),
    //            Name = model.Name,
    //            Email = model.Email,
    //            MobileNumber = model.MobileNumber,
    //            Address = model.Address,
    //            PaymentMethod = model.PaymentMethod,
    //            // يمكن تعيين حالة الطلب الافتراضية هنا
    //            OrderStatusId = 1,
    //            OrderDetail = cart.CartDetails.Select(cd => new OrderDetail
    //            {
    //                DishId = cd.DishId,
    //                Quantity = cd.Quantity,
    //                UnitPrice = cd.UnitPrice,
    //            }).ToList()
    //        };

    //        // يمكن هنا استدعاء عملية لتعديل المخزون إن وجدت
    //        // await ProcessStockAsync(cart.CartDetails);

    //        await _unitOfWork.Orders.AddAsync(order);
    //        // إزالة تفاصيل السلة بعد إنشاء الطلب
    //        _unitOfWork.CartDetails.RemoveRange(cart.CartDetails);

    //        var saveResult = await _unitOfWork.CommitAsync();
    //        if (saveResult <= 0)
    //        {
    //            _logger.LogWarning("لم يتم حفظ التغييرات عند إنشاء الطلب للمستخدم: {UserId}", userId);
    //            await transaction.RollbackAsync();
    //            return Result<OrderViewModel>.Failure("فشل عملية الدفع");
    //        }

    //        await transaction.CommitAsync();
    //        _logger.LogInformation("تم إنشاء الطلب بنجاح للمستخدم: {UserId}", userId);

    //        var orderViewModel = _mapper.Map<OrderViewModel>(order);
    //        return Result<OrderViewModel>.CreateSuccess(orderViewModel, "تم إنشاء الطلب بنجاح");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "فشل عملية إنشاء الطلب");
    //        await transaction.RollbackAsync();
    //        return Result<OrderViewModel>.Failure("فشل عملية الدفع");
    //    }
    //}



//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using UsersApp.Common.Results;
//using UsersApp.Models;
//using UsersApp.Services.Order;
//using UsersApp.Services.Repository;
//using UsersApp.ViewModels;
//using UsersApp.ViewModels.Cart;

//public class OrderService : IOrderService
//{
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly ILogger<OrderService> _logger;
//    private readonly IMapper _mapper;

//    public OrderService(
//        IUnitOfWork unitOfWork,
//        ILogger<OrderService> logger,
//        IMapper mapper)
//    {
//        _unitOfWork = unitOfWork;
//        _logger = logger;
//        _mapper = mapper;
//    }

//    public async Task<Result<OrderViewModel>> CreateOrderAsync(CheckoutViewModel model)
//    {
//        using var transaction = await _unitOfWork.BeginTransactionAsync();
//        try
//        {
//            var userId = model.userid;
//            var cart = await _unitOfWork.ShoppingCarts
//                .GetWithIncludes(
//                    c => c.CartDetails

//                )
//                .FirstOrDefaultAsync(c => c.UserId == userId);

//            if (cart?.CartDetails == null || !cart.CartDetails.Any())
//                return Result<OrderViewModel>.Failure("Cart is empty");

//            var order = new UsersApp.Models.Order
//            {
//                UserId = userId,
//                totalprice = cart.CartDetails.Sum(cd => cd.UnitPrice * cd.Quantity),
//                //OrderStatus = ,
//                OrderDetail = cart.CartDetails.Select(cd => new OrderDetail
//                {
//                    DishId = cd.DishId,
//                    Quantity = cd.Quantity,
//                    UnitPrice = cd.UnitPrice,
                    
                    
//                }).ToList()
//            };

//            //await ProcessStockAsync(cart.CartDetails);

//            await _unitOfWork.Orders.AddAsync(order);
//            _unitOfWork.CartDetails.RemoveRange(cart.CartDetails);

//            await _unitOfWork.CommitAsync();
//            await transaction.CommitAsync();

//            return Result<OrderViewModel>.CreateSuccess(_mapper.Map<OrderViewModel>(order));
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Checkout failed");
//            await transaction.RollbackAsync();
//            return Result<OrderViewModel>.Failure("Checkout process failed");
//        }
//    }

//    //private async Task ProcessStockAsync(IEnumerable<CartDetail> cartDetails)
//    //{
//    //    foreach (var item in cartDetails)
//    //    {
//    //        var stock = await _unitOfWork.Stocks
//    //            .GetFirstAsync(s => s.DishId == item.DishId);

//    //        if (stock.Quantity < item.Quantity)
//    //            throw new InvalidOperationException($"Insufficient stock for dish {item.DishId}");

//    //        stock.Quantity -= item.Quantity;
//    //        _unitOfWork.Stocks.Update(stock);
//    //    }
//    //}
//}