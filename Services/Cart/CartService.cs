using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.Services.Repository;
using UsersApp.ViewModels.Cart;

namespace UsersApp.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<CartService> _logger;
        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IUnitOfWork unitOfWork,
             ILogger<CartService> logger,
             IHttpContextAccessor httpContextAccessor,
             IMapper mapper,
            UserManager<Users> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper=mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<ShoppingCart>> GetCartAsync(string userId)
        {
            //using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                //if (string.IsNullOrEmpty(userId))
                //    return Result<ShoppingCart>.Failure("User not authenticated");

                var cart = await _unitOfWork.ShoppingCarts.Get().Where(s => s.UserId.ToString() == userId).FirstOrDefaultAsync();


                if (cart == null)
                    return Result<ShoppingCart>.CreateSuccess(new ShoppingCart());

                await _unitOfWork.CommitAsync();
                return Result<ShoppingCart>.CreateSuccess(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return Result<ShoppingCart>.Failure("Failed to get cart");
            }
        }
        public async Task<Result<CartViewModel>> GetCartviewModelAsync()
        {


            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();
                var cart = await _unitOfWork.ShoppingCarts
                    .GetWithIncludes(
                        c => c.CartDetails

                    )
                    .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

                if (cart == null)
                    return Result<CartViewModel>.Failure("Cart not found");

                var cartViewModel = _mapper.Map<CartViewModel>(cart);
                await transaction.CommitAsync();
                return Result<CartViewModel>.CreateSuccess(cartViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user cart");
                await transaction.RollbackAsync();
                return Result<CartViewModel>.Failure("Failed to retrieve cart");
            }

        }
        //public async Task<Result<ShoppingCart>> GetOrCreateCartAsync()
        //{
        //    var userId = GetUserId();

        //    var cart = await _unitOfWork.ShoppingCarts.GetWithIncludes(c => c.CartDetails)
        //        .FirstOrDefaultAsync(c =>
        //            (userId != null && c.UserId == userId));

        //    if (cart == null)
        //    {
        //        cart = new ShoppingCart
        //        {
        //            UserId = userId,
        //            //IsGuestCart = userId == null,

        //        };
        //        await _unitOfWork.ShoppingCarts.AddAsync(cart);

        //        var saveResult = await _unitOfWork.CompleteAsync();

        //        if (saveResult > 0)
        //        {
        //            _logger.LogInformation("Cart added successfully. ID: {Cartid}", cart.Id);
        //            return Result<ShoppingCart>.CreateSuccess(cart, "تم إضافة السله بنجاح");
        //        }

        //        _logger.LogWarning("No changes saved for cart: {Cartid}");
        //        return Result<ShoppingCart>.Failure("لم يتم حفظ التغييرات");
        //    }
        //    _logger.LogInformation("Cart Founded successfully. ID: {Cartid}", cart.Id);
        //    return Result<ShoppingCart>.CreateSuccess(cart, "تم إضافة السله بنجاح");
        //}
        public async Task<Result<ShoppingCart>> GetOrCreateCartAsync()
        {
            var userId = GetUserId();

            var cart = await _unitOfWork.ShoppingCarts
                .GetWithIncludes(c => c.CartDetails)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    //IsGuestCart = UserId==null?true userId.StartsWith("Guest") // يميز السلة الخاصة بالضيف
                };
                await _unitOfWork.ShoppingCarts.AddAsync(cart);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Cart created for user: {UserId}", userId);
            }

            return Result<ShoppingCart>.CreateSuccess(cart, "تم استرجاع السلة بنجاح");
        }

        //public async Task<Result<int>> AddItemAsync(Guid Dishid, int qty, string geustid)
        //{

        //    //using var transaction = await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        var userId = GetUserId();
        //        var Dish = await _unitOfWork.Dishs.GetByIdAsync(Dishid);

        //        if (Dish == null)
        //            return Result<int>.Failure("Dish not found");
        //        var cart = await GetOrCreateCartAsync();

        //        var existingItem = cart.Data.CartDetails.FirstOrDefault(ci => ci.DishId == Dishid);

        //        if (existingItem != null)
        //        {
        //            existingItem.Quantity += qty;
        //        }
        //        else
        //        {
        //            cart.Data.CartDetails.Add(new CartDetail
        //            {
        //                DishId = Dishid,
        //                Quantity = qty,
        //                UnitPrice = Dish.Price,
        //                ShoppingCartId = cart.Data.Id,

        //            });
        //        }

        //        var saveResult = await _unitOfWork.CompleteAsync();

        //        if (saveResult > 0)
        //        {
        //            _logger.LogInformation("Cart added successfully. ID:");
        //            return Result<int>.CreateSuccess(cart.Data.CartDetails.Sum(cd => cd.Quantity), "تم إضافة السله بنجاح");
        //        }

        //        _logger.LogWarning("No changes saved for cart: {Cartid}");
        //        return Result<int>.CreateSuccess(cart.Data.CartDetails.Sum(cd => cd.Quantity));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding item to cart");
        //        //await transaction.RollbackAsync();
        //        return Result<int>.Failure("Failed to add item to cart");
        //    }
        //}
        private async Task<Guid> GetRestaurantIdByDishid(Guid Dishid)
        {
            var dish = await _unitOfWork.Dishs.Get()
                .Where(d => d.Id == Dishid).FirstOrDefaultAsync();

            return dish?.RestaurantId ?? Guid.Empty; // إذا لم يوجد dish، نعيد Guid فارغ
        }
        public async Task<Result<int>> AddItemAsync(Guid dishId, int qty)
        {
            try
            {
                var userId = GetUserId();
                var dish = await _unitOfWork.Dishs.GetByIdAsync(dishId);

                if (dish == null)
                    return Result<int>.Failure("الوجبة غير موجودة");

                var cartResult = await GetOrCreateCartAsync();
                if (!cartResult.Success) return Result<int>.Failure(cartResult.Message);

                var cart = cartResult.Data;
                var existingItem = cart.CartDetails.FirstOrDefault(ci => ci.DishId == dishId);

                if (existingItem != null)
                {
                    existingItem.Quantity += qty;
                }
                else
                {
                    cart.CartDetails.Add(new CartDetail
                    {
                        DishId = dishId,
                        Quantity = qty,
                        UnitPrice = dish.Price,
                        ShoppingCartId = cart.Id,
                        Restaurantid= GetRestaurantIdByDishid(dishId).Result,

                    });
                }

                await _unitOfWork.CompleteAsync();
                return Result<int>.CreateSuccess(cart.CartDetails.Sum(cd => cd.Quantity), "تمت إضافة العنصر بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء إضافة العنصر إلى السلة");
                return Result<int>.Failure("فشل في إضافة العنصر");
            }
        }

        public async Task<Result<int>> RemoveItemAsync(Guid dishid)
        {
            //using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();

                var cart = await GetCartAsync(userId);

                var cartItem = cart.Data.CartDetails.FirstOrDefault(cd => cd.DishId == dishid);
                if (cartItem == null)
                    return Result<int>.Failure("Item not found in cart");

                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                    _unitOfWork.CartDetails.Delete(cartItem);
                }

                await _unitOfWork.CommitAsync();

                return Result<int>.CreateSuccess(cart.Data.CartDetails.Sum(cd => cd.Quantity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                //await transaction.RollbackAsync();
                return Result<int>.Failure("Failed to remove item from cart");
            }
        }
        //private string GetUserId()
        //{
        //    var user = _httpContextAccessor.HttpContext?.User;
        //    return user != null ?
        //        _userManager.GetUserId(user) :
        //        throw new UnauthorizedAccessException("User not authenticated");
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
    }
}
        //public async Task<Result> MergeCartsAsync(string userId, string guestId)
        //{
        //    try
        //    {
        //        var guestCart = await GetOrCreateCartAsync(null, guestId);
        //        var userCart = await GetOrCreateCartAsync(userId, null);

        //        foreach (var item in guestCart.Data.CartDetails)
        //        {
        //            var existingItem = userCart.Data.CartDetails.FirstOrDefault(ci => ci.DishId == item.DishId);

        //            if (existingItem != null)
        //            {
        //                existingItem.Quantity += item.Quantity;
        //            }
        //            else
        //            {
        //                userCart.Data.CartDetails.Add(new CartItemViewModel
        //                {
        //                    //Id = Guid.NewGuid(),
        //                    Dishid = item.DishId,
        //                    Quantity = item.Quantity,
        //                    TotalPrice = item.Price,
        //                    ImageUrl = item.ImageUrl,
        //                    CreatedAt = DateTime.UtcNow
        //                });
        //            }
        //        }

        //        // Delete guest cart
        //        _context.Carts.Remove(guestCart);
        //        await _context.SaveChangesAsync();

        //        return Result.Success();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error merging carts");
        //        return Result.Failure("Failed to merge carts");
        //    }
        //}

        //private CartViewModel MapToViewModel(ShoppingCart cart)
        //{
        //    return new CartViewModel
        //    {
        //         = cart.IsGuestCart,
        //        Items = cart.CartItems.Select(ci => new CartItemViewModel
        //        {
        //            Id = ci.Id,
        //            DishId = ci.DishId,
        //            DishName = ci.Dish.Name,
        //            Price = ci.Price,
        //            Quantity = ci.Quantity,
        //            ImageUrl = ci.ImageUrl,
        //            SubTotal = ci.Price * ci.Quantity
        //        }).ToList(),
        //        Total = cart.CartItems.Sum(ci => ci.Price * ci.Quantity)
        //    };
        //}
        //public async Task <Result<int>> UpdateItemAsync(CartItemViewModel model)
        //{
        //    try
        //    {
        //        var cart = await GetOrCreateCartAsync(model.Userid, model.Guestid);
        //        var item = cart.Data.CartDetails.FirstOrDefault(ci => ci.DishId == model.Dishid);

        //        if (item == null)
        //            return Result<int>.Failure("Item not found in cart");

        //        if (model.Quantity <= 0)
        //             await RemoveItemAsync(model.Userid, model.Guestid, model.Dishid.ToString());
        //        return Result<CartItemViewModel>.CreateSuccess("Item Deleted from cart");


        //        item.Quantity = model.Quantity;
        //        await _context.SaveChangesAsync();
        //        return Result.Success();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating cart item");
        //        return Result.Failure("Failed to update cart item");
        //    }
        //}
        //public async Task<int> AddItem(int bookId, int qty)
        //{
        //    string userId = GetUserId();
        //    using var transaction = await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        if (string.IsNullOrEmpty(userId))
        //            throw new UnauthorizedAccessException("user is not logged-in");
        //        var cart = await GetCart(userId);
        //        if (cart is null)
        //        {
        //            cart = new ShoppingCart
        //            {
        //                UserId = userId
        //            };
        //            _db.ShoppingCarts.Add(cart);
        //        }
        //        _db.SaveChanges();
        //        // cart detail section
        //        var cartItem = _db.CartDetails
        //                          .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
        //        if (cartItem is not null)
        //        {
        //            cartItem.Quantity += qty;
        //        }
        //        else
        //        {
        //            var book = _db.Books.Find(bookId);
        //            cartItem = new CartDetail
        //            {
        //                BookId = bookId,
        //                ShoppingCartId = cart.Id,
        //                Quantity = qty,
        //                UnitPrice = book.Price  // it is a new line after update
        //            };
        //            _db.CartDetails.Add(cartItem);
        //        }
        //        _db.SaveChanges();
        //        transaction.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    var cartItemCount = await GetCartItemCount(userId);
        //    return cartItemCount;
        //}


        //public async Task<int> RemoveItem(int bookId)
        //{
        //    //using var transaction = _db.Database.BeginTransaction();
        //    string userId = GetUserId();
        //    try
        //    {
        //        if (string.IsNullOrEmpty(userId))
        //            throw new UnauthorizedAccessException("user is not logged-in");
        //        var cart = await GetCart(userId);
        //        if (cart is null)
        //            throw new InvalidOperationException("Invalid cart");
        //        // cart detail section
        //        var cartItem = _db.CartDetails
        //                          .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
        //        if (cartItem is null)
        //            throw new InvalidOperationException("Not items in cart");
        //        else if (cartItem.Quantity == 1)
        //            _db.CartDetails.Remove(cartItem);
        //        else
        //            cartItem.Quantity = cartItem.Quantity - 1;
        //        _db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    var cartItemCount = await GetCartItemCount(userId);
        //    return cartItemCount;
        //}



        //public async Task<int> GetCartItemCount(string userId = "")
        //{
        //    if (string.IsNullOrEmpty(userId)) // updated line
        //    {
        //        userId = GetUserId();
        //    }
        //    var data = await (from cart in _db.ShoppingCarts
        //                      join cartDetail in _db.CartDetails
        //                      on cart.Id equals cartDetail.ShoppingCartId
        //                      where cart.UserId == userId // updated line
        //                      select new { cartDetail.Id }
        //                ).ToListAsync();
        //    return data.Count;
        //}

        //public async Task<bool> DoCheckout(CheckoutModel model)
        //{
        //    using var transaction = _db.Database.BeginTransaction();
        //    try
        //    {
        //        // logic
        //        // move data from cartDetail to order and order detail then we will remove cart detail
        //        var userId = GetUserId();
        //        if (string.IsNullOrEmpty(userId))
        //            throw new UnauthorizedAccessException("User is not logged-in");
        //        var cart = await GetCart(userId);
        //        if (cart is null)
        //            throw new InvalidOperationException("Invalid cart");
        //        var cartDetail = _db.CartDetails
        //                            .Where(a => a.ShoppingCartId == cart.Id).ToList();
        //        if (cartDetail.Count == 0)
        //            throw new InvalidOperationException("Cart is empty");
        //        var pendingRecord = _db.orderStatuses.FirstOrDefault(s => s.StatusName == "Pending");
        //        if (pendingRecord is null)
        //            throw new InvalidOperationException("Order status does not have Pending status");
        //        var order = new Order
        //        {
        //            UserId = userId,
        //            CreateDate = DateTime.UtcNow,
        //            Name = model.Name,
        //            Email = model.Email,
        //            MobileNumber = model.MobileNumber,
        //            PaymentMethod = model.PaymentMethod,
        //            Address = model.Address,
        //            IsPaid = false,
        //            OrderStatusId = pendingRecord.Id
        //        };
        //        _db.Orders.Add(order);
        //        _db.SaveChanges();
        //        foreach (var item in cartDetail)
        //        {
        //            var orderDetail = new OrderDetail
        //            {
        //                BookId = item.BookId,
        //                OrderId = order.Id,
        //                Quantity = item.Quantity,
        //                UnitPrice = item.UnitPrice
        //            };
        //            _db.OrderDetails.Add(orderDetail);

        //            // update stock here

        //            var stock = await _db.Stocks.FirstOrDefaultAsync(a => a.BookId == item.BookId);
        //            if (stock == null)
        //            {
        //                throw new InvalidOperationException("Stock is null");
        //            }

        //            if (item.Quantity > stock.Quantity)
        //            {
        //                throw new InvalidOperationException($"Only {stock.Quantity} items(s) are available in the stock");
        //            }
        //            // decrease the number of quantity from the stock table
        //            stock.Quantity -= item.Quantity;
        //        }
        //        //_db.SaveChanges();

        //        // removing the cartdetails
        //        _db.CartDetails.RemoveRange(cartDetail);
        //        _db.SaveChanges();
        //        transaction.Commit();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {

        //        return false;
        //    }
        //}

        //private string GetUserId()
        //{
        //    var user = _httpContextAccessor.HttpContext?.User;
        //    return user is not null ? _userManager.GetUserId(user) ?? string.Empty : string.Empty;

        //}
     
    

//using AutoMapper;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using UsersApp.Common.Results;
//using UsersApp.Models;
//using UsersApp.Services.Cart;
//using UsersApp.Services.Repository;
//using UsersApp.ViewModels.Cart;

//public class CartService : ICartService
//{
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly UserManager<Users> _userManager;
//    private readonly ILogger<CartService> _logger;
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    private readonly IMapper _mapper;

//    public CartService(
//        IUnitOfWork unitOfWork,
//        UserManager<Users> userManager,
//        ILogger<CartService> logger,
//        IHttpContextAccessor httpContextAccessor,
//        IMapper mapper)
//    {
//        _unitOfWork = unitOfWork;
//        _userManager = userManager;
//        _logger = logger;
//        _httpContextAccessor = httpContextAccessor;
//        _mapper = mapper;
//    }

//public async Task<Result<int>> AddItemAsync(Guid Dishid, int qty, string geustid)
//{

//    using var transaction = await _unitOfWork.BeginTransactionAsync();
//    try
//    {
//        var userId = GetUserId();
//        var Dish = await _unitOfWork.Dishs.GetByIdAsync(Dishid);

//        if (Dish == null)
//            return Result<int>.Failure("Dish not found");
//        var cart = await GetOrCreateCartAsync(userId);

//        var existingItem = cart.Data.CartDetails.FirstOrDefault(ci => ci.DishId == Dishid);

//        if (existingItem != null)
//        {
//            existingItem.Quantity += qty;
//        }
//        else
//        {
//            cart.Data.CartDetails.Add(new CartDetail
//            {
//                DishId = Dishid,
//                Quantity = qty,
//                UnitPrice = Dish.Price,
//                ShoppingCartId = cart.Data.Id,

//            });
//        }

//        await transaction.CommitAsync();

//        return Result<int>.CreateSuccess(cart.Data.CartDetails.Sum(cd => cd.Quantity));
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error adding item to cart");
//        await transaction.RollbackAsync();
//        return Result<int>.Failure("Failed to add item to cart");
//    }
//}


//public async Task<Result<int>> AddOrUpdateItemAsync(Guid dishId, int quantity)
//{
//    var dish = await _unitOfWork.Dishs.GetByIdAsync(dishId);

//    if (dish == null)
//        return Result<int>.Failure("Dish not found");

//    var cart = await GetOrCreateCartAsync();
//    if (cart == null || cart.Data == null)
//        return Result<int>.Failure("Cart not found or failed to create.");
//    if (cart.Data.CartDetails == null)
//        cart.Data.CartDetails = new List<CartDetail>();

//    var existingItem = cart.Data.CartDetails.FirstOrDefault(ci => ci.DishId == dishId);
//    if (existingItem != null)
//    {
//        existingItem.Quantity += quantity;
//    }
//    else
//    {
//        cart.Data.CartDetails.Add(new CartDetail
//        {
//            DishId = dishId,
//            Quantity = quantity,
//            UnitPrice = dish.Price,
//            ShoppingCartId = cart.Data.Id
//        });
//    }

//    await _unitOfWork.CommitAsync();
//    return Result<int>.CreateSuccess(cart.Data.CartDetails.Sum(cd => cd.Quantity));
//}


//public async Task<Result<int>> AddOrUpdateItemAsync(Guid dishId, int quantity)
//{
//    using var transaction = await _unitOfWork.BeginTransactionAsync();
//    try
//    {
//        var userId = GetUserId();
//        var dish = await _unitOfWork.Dishs.GetByIdAsync(dishId);

//        if (dish == null)
//            return Result<int>.Failure("Dish not found");

//        var cart = await GetOrCreateCartAsync(userId);
//        var cartDetail = cart.Data.CartDetails.FirstOrDefault(cd => cd.DishId == dishId);

//        if (cartDetail != null)
//        {
//            cartDetail.Quantity += quantity;
//        }
//        else
//        {
//            cart.Data.CartDetails.Add(new CartDetail
//            {
//                DishId = dishId,
//                Quantity = quantity,
//                UnitPrice = dish.Price,
//                ShoppingCartId = cart.Data.Id
//            });
//        }

//        await _unitOfWork.CommitAsync();
//        await transaction.CommitAsync();

//        return Result<int>.CreateSuccess(cart.Data.CartDetails.Sum(cd => cd.Quantity));
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error updating cart");
//        await transaction.RollbackAsync();
//        return Result<int>.Failure("Failed to update cart");
//    }
//}
//public async Task<Result<int>> RemoveItemAsync(Guid dishid)
//{
//    //using var transaction = await _unitOfWork.BeginTransactionAsync();
//    try
//    {

//        var cart = await GetOrCreateCartAsync();

//        var cartItem = cart.Data.CartDetails.FirstOrDefault(cd => cd.DishId == dishid);
//        if (cartItem == null)
//            return Result<int>.Failure("Item not found in cart");

//        if (cartItem.Quantity > 1)
//        {
//            cartItem.Quantity--;
//        }
//        else
//        {
//            _unitOfWork.CartDetails.Delete(cartItem);
//        }

//        await _unitOfWork.CommitAsync();

//        return Result<int>.CreateSuccess(cart.Data.CartDetails.Sum(cd => cd.Quantity));
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error removing item from cart");
//        //await transaction.RollbackAsync();
//        return Result<int>.Failure("Failed to remove item from cart");
//    }
//}
//public async Task<Result<ShoppingCart>> GetOrCreateCartAsync()
//{
//    var userId = GetUserId();

//    var cart = await _unitOfWork.ShoppingCarts.GetWithIncludes(c => c.CartDetails)
//        .FirstOrDefaultAsync(c =>
//            (userId != null && c.UserId == userId));

//    if (cart == null)
//    {
//        cart = new ShoppingCart
//        {
//            UserId = userId,
//            //IsGuestCart = userId == null,

//        };
//        await _unitOfWork.ShoppingCarts.AddAsync(cart);

//        var saveResult = await _unitOfWork.CompleteAsync();

//        if (saveResult > 0)
//        {
//            _logger.LogInformation("Cart added successfully. ID: {Cartid}", cart.Id);
//            return Result<ShoppingCart>.CreateSuccess(cart, "تم إضافة السله بنجاح");
//        }

//        _logger.LogWarning("No changes saved for cart: {Cartid}");
//        return Result<ShoppingCart>.Failure("لم يتم حفظ التغييرات");
//    }
//    _logger.LogInformation("Cart Founded successfully. ID: {Cartid}", cart.Id);
//    return Result<ShoppingCart>.CreateSuccess(cart, "تم إضافة السله بنجاح");
//}
//private async Task<Result<ShoppingCart>> GetOrCreateCartAsync(string userId)
//{
//    try
//    {
//        var cart = await _unitOfWork.ShoppingCarts
//            .GetWithIncludes(c => c.CartDetails)
//            .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

//        if (cart == null)
//        {
//            cart = new ShoppingCart
//            {
//                UserId = userId,
//                CartDetails = new List<CartDetail>(), // تأكد من التهيئة هنا
//                CreatedAt = DateTime.UtcNow
//            };
//            await _unitOfWork.ShoppingCarts.AddAsync(cart);
//            var saveResult = await _unitOfWork.CompleteAsync();
//            if (saveResult <= 0)
//                return Result<ShoppingCart>.Failure("Failed to save the cart.");
//        }


//        return Result<ShoppingCart>.CreateSuccess(cart);
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error getting or creating cart");
//        return Result<ShoppingCart>.Failure("Cart operation failed");
//    }
//}


