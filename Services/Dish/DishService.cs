using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.Services.Account;
using UsersApp.Services.Repository;
using UsersApp.ViewModels;
using UsersApp.ViewModels.restaurant.Dish;
using UsersApp.ViewModels.restaurant.Review;
using UsersApp.ViewModels.Restaurant;
using UsersApp.ViewModels.Restaurant.Dish;
using DishModel = UsersApp.Models.Dish;

namespace UsersApp.Services.Dish
{
    public class DishService: IDishService
    {
       
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<DishService> _logger;
        private readonly IAccountService _accountService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DishService(
            IUnitOfWork unitOfWork,
            UserManager<Users> userManager,
            ILogger<DishService> logger,
            IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _accountService = accountService;
        }

        public async Task<Result<Guid>> AddDishAsync(CreateDishViewModel dishDto)
        {
            try
            {
                // التأكد من عدم وجود طبق بنفس الاسم في المطعم نفسه
                var existingDish = await _unitOfWork.Dishs
                    .Find(d => d.Name == dishDto.Name &&
                               d.RestaurantId == dishDto.RestaurantId &&
                               !d.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingDish != null)
                {
                    return Result<Guid>.Failure("اسم الطبق موجود مسبقا في هذا المطعم");
                }

                // التحقق من وجود المطعم
                var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(dishDto.RestaurantId);
                if (restaurant == null)
                {
                    _logger.LogWarning("Restaurant with ID: {RestaurantId} not found", dishDto.RestaurantId);
                    return Result<Guid>.Failure("المطعم غير موجود");
                }

                // رفع الصورة إذا وُجدت
                string pictureUrl = null;
                if (dishDto.picture != null && dishDto.picture.Length > 0)
                {
                    var uploadResult = await _accountService.UploadPictureAsync(dishDto.picture, dishDto.urlPicture);
                    if (!uploadResult.Success)
                    {
                        _logger.LogWarning("Failed to upload picture: {Error}", uploadResult.Message);
                        return Result<Guid>.Failure( "أتاكد من امداد الصوره وان حجمها لايذيد عن 2 ميجا");
                    }
                    pictureUrl = uploadResult.Data;
                }

                // إنشاء كيان الطبق
                var dish = new DishModel
                {
                    Name = dishDto.Name,
                    Description = dishDto.Description,
                    Price = dishDto.Price,
                    Picture = pictureUrl,
                    RestaurantId = dishDto.RestaurantId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    CategoryId=dishDto.Categoryid

                };

                await _unitOfWork.Dishs.AddAsync(dish);
                var saveResult = await _unitOfWork.CompleteAsync();

                if (saveResult > 0)
                {
                    _logger.LogInformation("Dish added successfully. ID: {DishId}", dish.Id);
                    return Result<Guid>.CreateSuccess(dish.Id, "تم إضافة الطبق بنجاح");
                }

                _logger.LogWarning("No changes saved for dish: {DishName}", dishDto.Name);
                return Result<Guid>.Failure("لم يتم حفظ التغييرات");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding dish");
                return Result<Guid>.Failure("حدث خطأ أثناء إضافة الطبق");
            }
        }

        public async Task<Result<Guid>> UpdateDishAsync(CreateDishViewModel dishDto)
        {
            try
            {
                var dish = await _unitOfWork.Dishs.GetByIdAsync(dishDto.Id);
                if (dish == null || dish.IsDeleted)
                {
                    _logger.LogWarning("Dish with ID: {DishId} not found", dishDto.Id);
                    return Result<Guid>.Failure("الطبق غير موجود");
                }

                // في حال تغيير المطعم الذي ينتمي له الطبق
                if (dish.RestaurantId != dishDto.RestaurantId)
                {
                    var newRestaurant = await _unitOfWork.Restaurants.GetByIdAsync(dishDto.RestaurantId);
                    if (newRestaurant == null)
                    {
                        return Result<Guid>.Failure("المطعم الجديد غير موجود");
                    }
                    dish.RestaurantId = dishDto.RestaurantId;
                }

                // تحديث البيانات الأساسية للطبق
                dish.Name = dishDto.Name;
                dish.Description = dishDto.Description;
                dish.Price = dishDto.Price;
                dish.UpdatedAt = DateTime.Now;

                // تحديث الصورة إذا تم رفع صورة جديدة
                if (dishDto.picture != null && dishDto.picture.Length > 0)
                {
                    var uploadResult = await _accountService.UploadPictureAsync(dishDto.picture, dishDto.urlPicture);
                    if (!uploadResult.Success)
                    {
                        _logger.LogWarning("Failed to update picture: {Error}", uploadResult.Message);
                        return Result<Guid>.Failure("فشل تحديث الصورة");
                    }
                    dish.Picture = uploadResult.Data;
                }

                _unitOfWork.Dishs.Update(dish);
                var saveResult = await _unitOfWork.CompleteAsync();

                if (saveResult > 0)
                {
                    _logger.LogInformation("Dish updated successfully. ID: {DishId}", dish.Id);
                    return Result<Guid>.CreateSuccess(dish.Id, "تم تحديث الطبق بنجاح");
                }

                _logger.LogWarning("No changes saved for dish ID: {DishId}", dish.Id);
                return Result<Guid>.Failure("لم يتم حفظ التغييرات");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish ID: {DishId}", dishDto.Id);
                return Result<Guid>.Failure("حدث خطأ أثناء التحديث");
            }
        }

        public async Task<CreateDishViewModel> MapToDishViewModelAsync(UsersApp.Models.Dish dish)
        {
            try
            {
                var categoriesResult = await GetAllCategoriesAsync();
                List<SelectListItem> categoriesList = new List<SelectListItem>();

                if (categoriesResult.Success)
                {
                    // تحويل التصنيفات إلى SelectListItem
                    categoriesList = categoriesResult.Data
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                        .ToList();
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve categories for dish ID: {DishId}", dish.Id);
                }
                var viewModel = new CreateDishViewModel
                {
                    Id = dish.Id,
                    Name = dish.Name,
                    Description = dish.Description,
                    Price = dish.Price,
                    urlPicture = dish.Picture,
                    RestaurantId = dish.RestaurantId,
                    Categoryid = dish.CategoryId,
                    Dishescategories = categoriesList,
                    
                    
                };

                _logger.LogInformation("Mapped dish ID: {DishId} to view model", dish.Id);
                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping dish ID: {DishId}", dish.Id);
                return null;
            }
        }

        public async Task<Result<bool>> DeleteDishAsync(Guid dishId)
        {
            try
            {
                var dish = await _unitOfWork.Dishs.GetByIdAsync(dishId);
                if (dish == null || dish.IsDeleted)
                {
                    _logger.LogWarning("Dish with ID: {DishId} not found", dishId);
                    return Result<bool>.Failure("الطبق غير موجود");
                }

                dish.IsDeleted = true;
                _unitOfWork.Dishs.Update(dish);
                var saveResult = await _unitOfWork.CompleteAsync();

                if (saveResult > 0)
                {
                    _logger.LogInformation("Soft deleted dish ID: {DishId}", dishId);
                    return Result<bool>.CreateSuccess(true, "تم الحذف بنجاح");
                }

                _logger.LogWarning("No changes saved for dish ID: {DishId}", dishId);
                return Result<bool>.Failure("لم يتم حفظ التغييرات");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish ID: {DishId}", dishId);
                return Result<bool>.Failure("حدث خطأ أثناء الحذف");
            }
        }
        //public async Task<Result<PagedResult<CreateDishViewModel>>> GetFilteredDishesAsync(DishFilter filter)
        //{
        //    try
        //    {
        //        // التحقق من صحة معاملات التصفية (مثل PageNumber و PageSize)
        //        ValidateFilterParameters(filter);

        //        var baseQuery = _unitOfWork.Dishs
        //            .GetWithIncludes(d => d.Restaurants,d=>d.Reviews)
        //            .AsNoTracking();

        //        // تطبيق الفلاتر على الاستعلام
        //        var filteredQuery = ApplyFilters(baseQuery, filter);

        //        // حساب العدد الإجمالي قبل التقسيم إلى صفحات
        //        var totalCount = await filteredQuery.CountAsync();

        //        // تطبيق الفرز باستخدام الترتيب التصاعدي أو التنازلي حسب الطلب
        //        var orderedQuery = ApplySorting(filteredQuery, filter);

        //        // تطبيق التقسيم إلى صفحات
        //        var pagedQuery = ApplyPagination(orderedQuery, filter);

        //        // تحويل الكيانات إلى ViewModel
        //        var items = await pagedQuery
        //                            .Select(d => MapToViewModel(d)) // استخدام الدالة الثابتة
        //                            .ToListAsync();


        //        var pagedResult = new PagedResult<CreateDishViewModel>
        //        {
        //            PageNumber = filter.PageNumber,
        //            PageSize = filter.PageSize,
        //            TotalCount = totalCount,
        //            Items = items
        //        };

        //        return Result<PagedResult<CreateDishViewModel>>.CreateSuccess(pagedResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error filtering dishes");
        //        return Result<PagedResult<CreateDishViewModel>>.Failure("Failed to filter dishes");
        //    }
        //}
        public async Task<Result<PagedResult<DishDetailsViewModel>>> GetFilteredDishesAsync(DishFilter filter)
        {
            try
            {
                // التحقق من صحة معاملات التصفية (مثل PageNumber و PageSize)
                ValidateFilterParameters(filter);

                var baseQuery = _unitOfWork.Dishs
                    .GetWithIncludes(d => d.Restaurants, d => d.Reviews)
                    .AsNoTracking();

                // تطبيق الفلاتر على الاستعلام
                var filteredQuery = ApplyFilters(baseQuery, filter);

                // حساب العدد الإجمالي قبل التقسيم إلى صفحات
                var totalCount = await filteredQuery.CountAsync();

                // تطبيق الفرز باستخدام الترتيب التصاعدي أو التنازلي حسب الطلب
                var orderedQuery = ApplySorting(filteredQuery, filter);

                // تطبيق التقسيم إلى صفحات
                var pagedQuery = ApplyPagination(orderedQuery, filter);

                // تحويل الكيانات إلى ViewModel
                var items = await pagedQuery
                    .Select(d => new DishDetailsViewModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        Price = d.Price,
                        PictureUrl = d.Picture,
                        Rating = d.Rating,
                        RestaurantId = d.RestaurantId,
                        CategoryName = d.Category.Name,  // هنا نعرض اسم التصنيف

                        RestaurantName = d.Restaurants.Name,
                        Reviews = d.Reviews.Select(r => new ReviewViewModel
                        {
                            Id = r.Id,
                            CustomerName = r.CustomerName,
                            RestaurantId = r.RestaurantId,
                            DishId = r.Dishid,
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedAt = r.CreatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<DishDetailsViewModel>
                {
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount,
                    Items = items
                };

                return Result<PagedResult<DishDetailsViewModel>>.CreateSuccess(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering dishes");
                return Result<PagedResult<DishDetailsViewModel>>.Failure("Failed to filter dishes");
            }
        }
        //public async Task<Result<PagedResult<DishDetailsViewModel>>> GetFilteredDishesAsync(DishFilter filter)
        //{
        //    const int maxPageSize = 100;

        //    try
        //    {
        //        // التحقق من صحة المدخلات
        //        ValidateFilterParameters(filter, maxPageSize);

        //        var baseQuery = _unitOfWork.Dishs
        //            .GetWithIncludes(d => d.Restaurants, d => d.Reviews, d => d.Category)
        //            .AsNoTracking();

        //        var filteredQuery = ApplyFilters(baseQuery, filter);
        //        var totalCount = await filteredQuery.CountAsync();

        //        var orderedQuery = ApplySorting(filteredQuery, filter);
        //        var pagedQuery = ApplyPagination(orderedQuery, filter);

        //        var items = await pagedQuery
        //            .Select(d => MapToViewModel(d))
        //            .ToListAsync();

        //        return Result<PagedResult<DishDetailsViewModel>>.CreateSuccess(
        //            new PagedResult<DishDetailsViewModel>
        //            {
        //                PageNumber = filter.PageNumber,
        //                PageSize = filter.PageSize,
        //                TotalCount = totalCount,
        //                Items = items
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error filtering dishes");
        //        return Result<PagedResult<DishDetailsViewModel>>.Failure("Failed to filter dishes");
        //    }
        //}

        //private DishDetailsViewModel MapToViewModel(UsersApp.Models.Dish d)
        //{
        //    return new DishDetailsViewModel
        //    {
        //        Id = d.Id,
        //        Name = d.Name,
        //        Description = d.Description,
        //        Price = d.Price,
        //        PictureUrl = d.Picture,
        //        Rating = d.Reviews.Any() ? Math.Round(d.Reviews.Average(r => r.Rating), 1) : 0,
        //        RestaurantId = d.RestaurantId,
        //        RestaurantName = d.Restaurants.Name,
        //        CategoryName = d.Category.Name,

        //        Reviews = d.Reviews.Select(r => new ReviewViewModel
        //        {
        //            Id = r.Id,
        //            CustomerName = r.CustomerName,
        //            Rating = r.Rating,
        //            Comment = r.Comment,
        //            CreatedAt = r.CreatedAt
        //        }).ToList()
        //    };
        //}
        private void ValidateFilterParameters(DishFilter filter)
        {
            if (filter.PageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0");

            if (filter.PageSize < 1 || filter.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100");

            // يمكنك إضافة المزيد من التحقق حسب الحاجة
        }
        //private void ValidateFilterParameters(DishFilter filter, int maxPageSize)
        //{
        //    if (filter.PageNumber < 1)
        //        throw new ArgumentException("Page number must be greater than 0");

        //    filter.PageSize = Math.Min(filter.PageSize, maxPageSize);

        //    if (filter.MinPrice > filter.MaxPrice)
        //        throw new ArgumentException("Minimum price cannot be greater than maximum price");
        //}
        private IQueryable<UsersApp.Models.Dish> ApplyFilters(IQueryable<UsersApp.Models.Dish> query, DishFilter filter)
        {
            if (filter.RestaurantId.HasValue)
                query = query.Where(d => d.RestaurantId == filter.RestaurantId);

            if (filter.MinPrice.HasValue)
                query = query.Where(d => d.Price >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(d => d.Price <= filter.MaxPrice);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(d => d.Name.Contains(filter.SearchTerm) ||
                                         d.Description.Contains(filter.SearchTerm) ||
                                         d.Restaurants.Name.Contains(filter.SearchTerm));

            if (filter.RestaurantTypeId.HasValue)
                query = query.Where(d => d.Restaurants.RestaurantTypeId == filter.RestaurantTypeId);

            if (filter.CategoryId.HasValue)
                query = query.Where(d => d.CategoryId == filter.CategoryId);

            return query;
        }

        private IQueryable<UsersApp.Models.Dish> ApplySorting(IQueryable<UsersApp.Models.Dish> query, DishFilter filter)
        {
            // تحديد العمود المراد الترتيب بناءً على الخاصية SortBy، مع افتراضي "name"
            var sortBy = string.IsNullOrEmpty(filter.SortBy) ? "name" : filter.SortBy.ToLower();
            // تحديد اتجاه الترتيب: إذا كانت القيمة "desc" يتم الترتيب تنازليًا، وإلا تصاعديًا
            bool isAscending = string.IsNullOrEmpty(filter.SortDirection) || filter.SortDirection.ToLower() != "desc";

            switch (sortBy)
            {
                case "price":
                    query = isAscending ? query.OrderBy(d => d.Price) : query.OrderByDescending(d => d.Price);
                    break;
                case "rating":
                    query = isAscending ? query.OrderBy(d => d.Rating) : query.OrderByDescending(d => d.Rating);
                    break;
                case "name":
                default:
                    query = isAscending ? query.OrderBy(d => d.Name) : query.OrderByDescending(d => d.Name);
                    break;
            }

            return query;
        }

        private IQueryable<UsersApp.Models.Dish> ApplyPagination(IQueryable<UsersApp.Models.Dish> query, DishFilter filter)
        {
            return query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }

        private static CreateDishViewModel MapToViewModel(UsersApp.Models.Dish dish)
        {
            return new CreateDishViewModel
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                urlPicture = dish.Picture,
                Rating = dish.Rating,
                RestaurantId = dish.RestaurantId,
                RestaurantName = dish.Restaurants?.Name ?? string.Empty
            };
        }



        //public async Task<Result<PagedResult<UsersApp.Models.Dish>>> GetDishesPagedAsync(int pageNumber, int pageSize, Guid restaurantid)
        //{
        //    try
        //    {
        //        var totalCount = await _unitOfWork.Dishs
        //            .Get()
        //            .CountAsync(r => !r.IsDeleted);

        //        var Dishes = await _unitOfWork.Dishs
        //            .GetWithIncludes(r => r.Category, r => r.Restaurants,r=>r.Reviews)
        //            .Where(r => !r.IsDeleted && r.RestaurantId == restaurantid)
        //            .OrderBy(r => r.Name)
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        var pagedResult = new PagedResult<UsersApp.Models.Dish>
        //        {
        //            id=restaurantid,
        //            Items = Dishes,
        //            TotalCount = totalCount,
        //            PageNumber = pageNumber,
        //            PageSize = pageSize
        //        };
        //        foreach (var restaurant in pagedResult.Items)
        //        {
        //            if (restaurant.Category == null)
        //            {
        //                _logger.LogWarning("Dish {Id} has no associated Category!", restaurant.Id);
        //                restaurant.Category = new DishCategory { Name = "Unknown" }; // تعيين قيمة افتراضية
        //            }
        //        }
        //        return Result<PagedResult<UsersApp.Models.Dish>>.CreateSuccess(pagedResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving paged Restaurants");
        //        return Result<PagedResult<UsersApp.Models.Dish>>.Failure("Failed to retrieve paged Restaurants");
        //    }
        //}
        public async Task<Result<PagedResult<UsersApp.Models.Dish>>> GetDishesPagedAsync(int pageNumber, int pageSize, Guid restaurantid)
        {
            try
            {
                var totalCount = await _unitOfWork.Dishs
                    .Get()
                    .CountAsync(r => !r.IsDeleted);



                var Dishes = await _unitOfWork.Dishs
                    .GetWithIncludes(r => r.Category, r => r.Restaurants, r => r.Reviews)
                    .Where(r => !r.IsDeleted && r.RestaurantId == restaurantid)
                    .OrderBy(r => r.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagedResult = new PagedResult<UsersApp.Models.Dish>
                {
                    id = restaurantid,
                    Items = Dishes,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                foreach (var dish in pagedResult.Items)
                {
                    if (dish.Category == null)
                    {
                        _logger.LogWarning("Dish {Id} has no associated Category!", dish.Id);
                        dish.Category = new DishCategory { Name = "Unknown" }; // تعيين قيمة افتراضية
                    }

                    // التأكد من أن التقييمات ليست فارغة
                    if (dish.Reviews == null)
                    {
                        dish.Reviews = new List<Reviews>();
                    }
                }

                return Result<PagedResult<UsersApp.Models.Dish>>.CreateSuccess(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged Dishes");
                return Result<PagedResult<UsersApp.Models.Dish>>.Failure("Failed to retrieve paged Dishes");
            }
        }
        //public async Task<Result<PagedResult<UsersApp.Models.Dish>>> GetDishesPagedAsync(int pageNumber, int pageSize, Guid restaurantid, int Categoryid)
        //{
        //    try
        //    {
        //        var query = _unitOfWork.Dishs
        //            .GetWithIncludes(r => r.Category, r => r.Restaurants, r => r.Reviews)
        //            .Where(r => !r.IsDeleted && r.RestaurantId == restaurantid);

        //        // تطبيق الفلترة بناءً على CategoryId إذا لم يكن 0
        //        if (Categoryid != 0)
        //        {
        //            query = query.Where(r => r.CategoryId == Categoryid);
        //        }

        //        var totalCount = await query.CountAsync();

        //        var Dishes = await query
        //            .OrderBy(r => r.Name)
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        var pagedResult = new PagedResult<UsersApp.Models.Dish>
        //        {
        //            id = restaurantid,
        //            Items = Dishes,
        //            TotalCount = totalCount,
        //            PageNumber = pageNumber,
        //            PageSize = pageSize
        //        };

        //        foreach (var dish in pagedResult.Items)
        //        {
        //            if (dish.Category == null)
        //            {
        //                _logger.LogWarning("Dish {Id} has no associated Category!", dish.Id);
        //                dish.Category = new DishCategory { Name = "Unknown" }; // تعيين قيمة افتراضية
        //            }

        //            // التأكد من أن التقييمات ليست فارغة
        //            if (dish.Reviews == null)
        //            {
        //                dish.Reviews = new List<Reviews>();
        //            }
        //        }

        //        return Result<PagedResult<UsersApp.Models.Dish>>.CreateSuccess(pagedResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving paged Dishes");
        //        return Result<PagedResult<UsersApp.Models.Dish>>.Failure("Failed to retrieve paged Dishes");
        //    }
        //}

        public async Task<Users> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user;
        }
        public async Task<Result<UsersApp.Models.Dish>> GetDishByIdAsync(Guid id)
        {
            var dish = await _unitOfWork.Dishs.GetByIdAsync(id);
            if (dish != null)
            {
                return Result<UsersApp.Models.Dish>.CreateSuccess(dish);

            }
            else
            {
                return Result<UsersApp.Models.Dish>.Failure("Failed to retrieve Restaurant");
            }
        }
        public async Task<Result<decimal>> GetPriceAsync(Guid dishid)
        {
            var dish =await GetDishByIdAsync(dishid);
            if (dish != null)
            {
                return Result<decimal>.CreateSuccess(dish.Data.Price);
            }
            else {
                return Result<decimal>.Failure("We can't Find this dish");

            }
        }
        public async Task<Result<Restaurant>> GetRestaurantByDishIdAsync(Guid id)
        {
            var dish = await _unitOfWork.Dishs
                            .GetWithIncludes(d => d.Restaurants) // تضمين بيانات المطعم
                            .FirstOrDefaultAsync(d => d.Id == id);

            if (dish != null)
            {
                return Result<Restaurant>.CreateSuccess(dish.Restaurants);

            }
            else
            {
                return Result<Restaurant>.Failure("Failed to retrieve Restaurant");
            }
        }
        public async Task<Result<IEnumerable<DishCategory>>> GetAllCategoriesAsync()
        {
            try
            {
                // تأكد من أن _unitOfWork يحتوي على Repository للـ RestaurantType
                var types = await _unitOfWork.DishCategorys
                    .Get()
                    .AsNoTracking()
                    .ToListAsync();

                return Result<IEnumerable<DishCategory>>.CreateSuccess(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Restaurant types");
                return Result<IEnumerable<DishCategory>>.Failure("Failed to retrieve Restaurant types");
            }
        }

        public async Task<Result<DishDetailsViewModel>> GetDishDetailsAsync(Guid id)
        {
            try
            {
                var dish = await _unitOfWork.Dishs.GetWithIncludes(d => d.Restaurants,d=>d.Reviews)
                    .Where(d => d.Id == id)
                    .Select(d =>
                        new DishDetailsViewModel
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Description = d.Description,
                            Price = d.Price,
                            PictureUrl = d.Picture,
                            Rating = d.Rating,
                            RestaurantId = d.RestaurantId,
                            RestaurantName = d.Restaurants.Name,
                            Reviews = d.Reviews.Select(r => new ReviewViewModel
                            {
                                Id = r.Id,
                                CustomerName = r.CustomerName,
                                RestaurantId = r.RestaurantId,
                                DishId = r.Dishid,
                                Rating = r.Rating,
                                Comment = r.Comment,
                                CreatedAt = r.CreatedAt
                            }).ToList()
                        })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return dish != null
                    ? Result<DishDetailsViewModel>.CreateSuccess(dish)
                    : Result<DishDetailsViewModel>.Failure("Dish not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Dish details");
                return Result<DishDetailsViewModel>.Failure("Failed to retrieve Dish details");
            }
        }

    }
}


