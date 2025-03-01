using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.Services.Account;
using UsersApp.ViewModels;
using UsersApp.ViewModels.Order;
using UsersApp.ViewModels.restaurant.Restaurant.Dashboard;
using UsersApp.ViewModels.restaurant.Review;
using UsersApp.ViewModels.Restaurant;
using UsersApp.ViewModels.Restaurant.Dish;
using UsersApp.ViewModels.Restaurant.SelectionViewModel;

namespace UsersApp.Services.Repository
{
    public class RestaurantService: IRestaurantService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAccountService _accountService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<RestaurantService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RestaurantService(IMemoryCache memoryCache,
            IUnitOfWork unitOfWork,
            UserManager<Users> userManager,
            ILogger<RestaurantService> logger,
            IHttpContextAccessor httpContextAccessor,
            IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _accountService = accountService;
            _memoryCache = memoryCache;

        }
        private string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user != null ?
                _userManager.GetUserId(user) :
                throw new UnauthorizedAccessException("User not authenticated");
        }
        public async Task<Result<Guid>> AddRestaurantAsync(CreateRestaurantViewModel RestaurantDto)
        {
            try
            {
                string pictureUrl = null;
                if (RestaurantDto.Picture != null && RestaurantDto.Picture.Length > 0)
                {
                    var result = await _accountService.UploadPictureAsync(RestaurantDto.Picture, RestaurantDto.Pictureurl);
                    if (result.Success)
                    {
                        pictureUrl = result.Data;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload picture: {ErrorMessage}", result.Message);
                        return Result<Guid>.Failure("Failed to upload picture.");
                    }
                }

                var Restaurant = new Restaurant
                {
                    Name = RestaurantDto.Name,
                    Location = RestaurantDto.Location,
                    Description = RestaurantDto.Description,
                    OwnerId = RestaurantDto.ownerId ?? "",
                    CityId = RestaurantDto.CityId,
                    RestaurantTypeId = RestaurantDto.RestaurantTypeId,
                    Picture = pictureUrl
                };

                await _unitOfWork.Restaurants.AddAsync(Restaurant);

                if (RestaurantDto.Schedules != null && RestaurantDto.Schedules.Any())
                {
                    Restaurant.RestaurantSchedules = RestaurantDto.Schedules.Where(s => s.IsOpen == true).Select(s => new RestaurantSchedule
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = Restaurant.Id,
                        DayOfWeek = s.DayOfWeek,
                        OpeningTime = s.OpeningTime,
                        ClosingTime = s.ClosingTime,
                        IsOpen = s.IsOpen

                    }).ToList();

                    await _unitOfWork.RestaurantSchedules.AddRangeAsync(Restaurant.RestaurantSchedules);
                }

                var saveState = await _unitOfWork.CompleteAsync();

                if (saveState > 0)
                {
                    await AssignRestaurantClaimAsync(RestaurantDto.ownerId, Restaurant.Id);
                    _logger.LogInformation("Restaurant and schedules added successfully with ID: {RestaurantId}", Restaurant.Id);
                    return Result<Guid>.CreateSuccess(Restaurant.Id, "Restaurant and schedules added successfully.");
                }
                else
                {
                    _logger.LogWarning("No changes were saved to the database for the Restaurant.");
                    return Result<Guid>.Failure("No changes were made to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Restaurant");
                return Result<Guid>.Failure("Failed to add Restaurant");
            }
        }
         public async Task<CreateRestaurantViewModel> MapToViewModelAsync(Restaurant Restaurant)
        {
            try
            {
                CreateRestaurantViewModel newcreate = new CreateRestaurantViewModel();
                    newcreate.Id = Restaurant.Id;
                newcreate.Pictureurl = Restaurant.Picture;
                newcreate.Schedules = Restaurant.RestaurantSchedules.Select( s => new RestaurantScheduleViewModel
                {
                    id = s.Id,
                    Restaurantid = s.RestaurantId,
                    DayOfWeek = s.DayOfWeek,
                    OpeningTime = s.OpeningTime,
                    ClosingTime = s.ClosingTime,
                    IsOpen = s.IsOpen,

                }).ToList();

                newcreate.option = await GetRestaurantSelectionDataAsync();
                newcreate.CityId = Restaurant.CityId;
                newcreate.Description = Restaurant.Description;
                newcreate.Location = Restaurant.Location;
                newcreate.Name = Restaurant.Name;
                newcreate.RestaurantTypeId = Restaurant.RestaurantTypeId;
                newcreate.ownerId = Restaurant.OwnerId;
                _logger.LogInformation("Restaurant with ID: {RestaurantId} updated successfully.", Restaurant.Id);
                return  newcreate;

            }
            catch(Exception e)
            {
                _logger.LogInformation("Restaurant dont updated : {RestaurantId} Error.", e);
                return null;
            }
        }
        public async Task<Result<Guid>> UpdateRestaurantAsync(CreateRestaurantViewModel RestaurantDto)
        {
            try
            {
                var Restaurant = await _unitOfWork.Restaurants.GetByIdAsync(RestaurantDto.Id);
                if (Restaurant == null)
                {
                    _logger.LogWarning("Restaurant with ID: {RestaurantId} not found.", RestaurantDto.Id);
                    return Result<Guid>.Failure("Restaurant not found.");
                }

                // Update basic Restaurant details
                Restaurant.Name = RestaurantDto.Name;
                Restaurant.Location = RestaurantDto.Location;
                Restaurant.Description = RestaurantDto.Description;
                Restaurant.CityId = RestaurantDto.CityId;
                Restaurant.RestaurantTypeId = RestaurantDto.RestaurantTypeId;

                // Update picture if a new one is provided
                if (RestaurantDto.Picture != null && RestaurantDto.Picture.Length > 0)
                {
                    var result = await _accountService.UploadPictureAsync(RestaurantDto.Picture, Restaurant.Picture);
                    if (result.Success)
                    {
                        Restaurant.Picture = result.Data;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload picture: {ErrorMessage}", result.Message);
                        return Result<Guid>.Failure("Failed to upload new picture.");
                    }
                }

                // Update schedules
                if (RestaurantDto.Schedules != null && RestaurantDto.Schedules.Any())
                {
                    var existingSchedules =  _unitOfWork.RestaurantSchedules.Get().Where(s=>s.RestaurantId== RestaurantDto.Id);
                   
                    _unitOfWork.RestaurantSchedules.RemoveRange(existingSchedules);

                    Restaurant.RestaurantSchedules = RestaurantDto.Schedules.Select(s => new RestaurantSchedule
                    {
                        Id= Guid.NewGuid(),
                        RestaurantId = Restaurant.Id,
                        DayOfWeek = s.DayOfWeek,
                        OpeningTime = s.OpeningTime,
                        ClosingTime = s.ClosingTime,
                        IsOpen = s.IsOpen,
                        
                    }).ToList();

                    await _unitOfWork.RestaurantSchedules.AddRangeAsync(Restaurant.RestaurantSchedules);
                }

                // Save all changes
                var saveState = await _unitOfWork.CompleteAsync();
                if (saveState > 0)
                {
                    _logger.LogInformation("Restaurant with ID: {RestaurantId} updated successfully.", Restaurant.Id);
                    return Result<Guid>.CreateSuccess(Restaurant.Id, "Restaurant updated successfully.");
                }
                else
                {
                    _logger.LogWarning("No changes were saved to the database for the Restaurant with ID: {RestaurantId}.", Restaurant.Id);
                    return Result<Guid>.Failure("No changes were made to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Restaurant with ID: {RestaurantId}", RestaurantDto.Id);
                return Result<Guid>.Failure("Failed to update Restaurant.");
            }
        }

       



        public async Task<Users> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user;
        }
        public async Task<Result<Restaurant>> GetRestaurantbyidasync(Guid id)
        {
            var restauranr = await _unitOfWork.Restaurants.GetByIdAsync(id);
            if(restauranr != null)
            {
                return Result<Restaurant>.CreateSuccess(restauranr);

            }
            else
            {
                return Result<Restaurant>.Failure("Failed to retrieve Restaurant");
            }
        }
        public async Task<Result<IEnumerable<City>>> GetAllCitiesAsync()
        {
            try
            {
                // تأكد من أن _unitOfWork يحتوي على Repository للـ City
                var cities = await _unitOfWork.Citys.Get().AsNoTracking().ToListAsync();


                return Result<IEnumerable<City>>.CreateSuccess(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities");
                return Result<IEnumerable<City>>.Failure("Failed to retrieve cities");
            }
        }

        public async Task<Result<IEnumerable<RestaurantType>>> GetAllRestaurantTypesAsync()
        {
            try
            {
                // تأكد من أن _unitOfWork يحتوي على Repository للـ RestaurantType
                var types = await _unitOfWork.RestaurantTypes
                    .Get()
                    .AsNoTracking()
                    .ToListAsync();

                return Result<IEnumerable<RestaurantType>>.CreateSuccess(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Restaurant types");
                return Result<IEnumerable<RestaurantType>>.Failure("Failed to retrieve Restaurant types");
            }
        }
        public async Task<Result<IEnumerable<RestaurantSchedule>>> GenerateDefaultSchedulesAsync()
        {
            try
            {
                // تعريف أسماء الأيام مع أرقامها
                var schedules = Enumerable.Range(0, 7)
                    .Select(i => new RestaurantSchedule
                    {
                        DayOfWeek = i,
                        OpeningTime = new TimeOnly(9, 0),   // وقت افتراضي: 09:00
                        ClosingTime = new TimeOnly(22, 0),// وقت افتراضي: 22:00
                        IsOpen = false
                    })
                    .ToList();



                return Result<IEnumerable<RestaurantSchedule>>.CreateSuccess(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating default Restaurant schedules");
                return Result<IEnumerable<RestaurantSchedule>>.Failure("Failed to generate Restaurant schedules");
            }
        }
        public async Task<RestaurantSelectionViewModel> GetRestaurantSelectionDataAsync()
        {
            var cacheKey = "RestaurantSelectionData";

            // التحقق مما إذا كانت البيانات موجودة في الكاش
            if (_memoryCache.TryGetValue(cacheKey, out RestaurantSelectionViewModel cachedData))
            {
                return cachedData; // إرجاع البيانات المخزنة مباشرةً
            }
            var viewModel = new RestaurantSelectionViewModel();

            var citiesResult = await GetAllCitiesAsync();
            var typesResult = await GetAllRestaurantTypesAsync();
            var schedulesResult = await GenerateDefaultSchedulesAsync();

            viewModel.Cities = citiesResult.Success
                ? citiesResult.Data.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
                : new List<SelectListItem>();

            viewModel.RestaurantTypes = typesResult.Success
                ? typesResult.Data.Select(rt => new SelectListItem { Value = rt.Id.ToString(), Text = rt.Name }).ToList()
                : new List<SelectListItem>();

            viewModel.Schedules = schedulesResult.Success
                ? schedulesResult.Data.Select(s => new RestaurantScheduleViewModel
                {

                    DayOfWeek = s.DayOfWeek,
                    OpeningTime = s.OpeningTime,
                    ClosingTime = s.ClosingTime,
                    IsOpen = s.IsOpen

                }).ToList()
            : new List<RestaurantScheduleViewModel>(); // تأكد أنها ليست `null`
            _memoryCache.Set(cacheKey, viewModel, CacheOptions);

            return viewModel;
        }

        public async Task<Result<IEnumerable<CreateRestaurantViewModel>>> GetAllRestaurantsAsync()
        {
            try
            {
                const string cacheKey = "AllRestaurants";

                if (!_memoryCache.TryGetValue(cacheKey, out List<CreateRestaurantViewModel> restaurants))
                {
                    restaurants = await _unitOfWork.Restaurants
                        .Get()
                        .Where(r => !r.IsDeleted)
                        .AsNoTracking()
                        .Select(r => new CreateRestaurantViewModel
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Location = r.Location,
                            Description = r.Description,
                            CityId = r.CityId,
                            RestaurantTypeId = r.RestaurantTypeId
                        })
                        .ToListAsync();

                    _memoryCache.Set(cacheKey, restaurants, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(30)
                    });
                }

                return Result<IEnumerable<CreateRestaurantViewModel>>.CreateSuccess(
                    restaurants,
                    "Restaurants retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Restaurants");
                return Result<IEnumerable<CreateRestaurantViewModel>>.Failure("Failed to retrieve Restaurants");
            }
        }


        public async Task<Result<Restaurant>> GetRestaurantByIdAsync(Guid id)
        {
            try
            {
                var Restaurant = await _unitOfWork.Restaurants
                    .GetWithIncludes(r => r.RestaurantSchedules, r => r.Dishes)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                return Restaurant != null
                    ? Result<Restaurant>.CreateSuccess(Restaurant)
                    : Result<Restaurant>.Failure("Restaurant not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving Restaurant {id}");
                return Result<Restaurant>.Failure("Failed to retrieve Restaurant");
            }
        }


        //public async Task<Result<Guid>> AddRestaurantAsync(CreateRestaurantViewModel RestaurantDto)
        //{
        //    try
        //    {



        //        // أو استخدام IFormFile مباشرةً
        //        string pictureUrl = null;
        //        if (RestaurantDto.Picture != null && RestaurantDto.Picture.Length > 0)
        //        {

        //            var result = await _accountService.UploadPictureAsync(RestaurantDto.Picture, RestaurantDto.Pictureurl);
        //            pictureUrl = result.Data;
        //        }

        //        var schedules = RestaurantDto.Schedules ?? new List<RestaurantScheduleViewModel>();

        //        var Restaurant = new Restaurant
        //        {
        //            Name = RestaurantDto.Name,
        //            Location = RestaurantDto.Location,
        //            Description = RestaurantDto.Description,
        //            OwnerId = RestaurantDto.ownerId??"",
        //            CityId = RestaurantDto.CityId,
        //            RestaurantTypeId = RestaurantDto.RestaurantTypeId,
        //            Picture = pictureUrl,

        //        };

        //        await _unitOfWork.Restaurant.AddAsync(Restaurant);

        //        var saveState = await _unitOfWork.CompleteAsync();

        //        if (saveState > 0)
        //        {
        //            _logger.LogInformation("Restaurant added successfully with ID: {RestaurantId}", Restaurant.Id);

        //            Restaurant.RestaurantSchedules = RestaurantDto.Schedules.Select(s => new RestaurantSchedule
        //            {
        //                RestaurantId = Restaurant.Id,
        //                DayOfWeek = s.DayOfWeek,
        //                OpeningTime = s.OpeningTime,
        //                ClosingTime = s.ClosingTime
        //            }).ToList();

        //            await _unitOfWork.RestaurantSchedules.AddRangeAsync(Restaurant.RestaurantSchedules);

        //            var scheduleSaveState = await _unitOfWork.CompleteAsync();
        //            if (scheduleSaveState > 0)
        //            {
        //                await AssignRestaurantClaimAsync(RestaurantDto.ownerId, Restaurant.Id);
        //                _logger.LogInformation("Schedules added successfully for Restaurant ID: {RestaurantId}", Restaurant.Id);
        //                return Result<Guid>.CreateSuccess(Restaurant.Id, "Restaurant and schedules added successfully.");

        //            }
        //            else
        //            {
        //                _logger.LogWarning("Schedules were not saved for Restaurant ID: {RestaurantId}", Restaurant.Id);
        //                return Result<Guid>.Failure("Restaurant added, but schedules were not saved.");
        //            }
        //        }
        //        else
        //        {
        //            _logger.LogWarning("No changes were saved to the database for the Restaurant.");
        //            return Result<Guid>.Failure("No changes were made to the database.");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding Restaurant");
        //        return Result<Guid>.Failure("Failed to add Restaurant");
        //    }
        //}

        //public async Task<Result<bool>> UpdateRestaurantAsync(Guid id, UpdateRestaurantViewModel RestaurantDto)
        //{
        //    try
        //    {
        //        var Restaurant = await _unitOfWork.Restaurant.GetByIdAsync(id);
        //        if (Restaurant == null || Restaurant.IsDeleted)
        //            return Result<bool>.Failure("Restaurant not found");

        //        Restaurant.Name = RestaurantDto.Name;
        //        Restaurant.Description = RestaurantDto.Description;
        //        Restaurant.Location = RestaurantDto.Location;

        //        _unitOfWork.Restaurant.Update(Restaurant);
        //        await _unitOfWork.CompleteAsync();

        //        return Result<bool>.CreateSuccess(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error updating Restaurant {id}");
        //        return Result<bool>.Failure("Failed to update Restaurant");
        //    }
        //}


        private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
        };

        //public async Task<Result<RestaurantStatisticsViewModel>> GetRestaurantStatisticsAsync(Guid RestaurantId)
        //{
        //    var cacheKey = $"RestaurantStats_{RestaurantId}";
        //    if (_memoryCache.TryGetValue(cacheKey, out RestaurantStatisticsViewModel cachedStats))
        //    {
        //        return Result<RestaurantStatisticsViewModel>.CreateSuccess(cachedStats);
        //    }

        //    try
        //    {
        //        var stats = await _unitOfWork.Restaurants.GetWithIncludes(r=>r.Dishes, r => r.Orders,r=>r.Reviews)
        //            .Where(r => r.Id == RestaurantId && !r.IsDeleted)
        //            .Select(r => new RestaurantStatisticsViewModel
        //            {
        //                TotalDishes = r.Dishes.Count(d => !d.IsDeleted),
        //                TotalOrders = r.Orders.Count(o => !o.IsDeleted),
        //                TotalReviews = r.Reviews.Count,
        //                AverageRating = r.Reviews.Count > 0 ? r.Reviews.Average(review => review.Rating) : 0,
        //                RecentOrders = r.Orders
        //                    .Where(o => !o.IsDeleted)
        //                    .OrderByDescending(o => o.CreateDate)
        //                    .Take(10)
        //                    .ToList(),
        //                RecentReviews = r.Reviews
        //                    .OrderByDescending(r => r.CreatedAt)
        //                    .Take(10)
        //                    .ToList()
        //            })
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync();

        //        if (stats == null)
        //            return Result<RestaurantStatisticsViewModel>.Failure("Restaurant not found");

        //        _memoryCache.Set(cacheKey, stats, CacheOptions);
        //        return Result<RestaurantStatisticsViewModel>.CreateSuccess(stats);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting statistics for Restaurant {RestaurantId}", RestaurantId);
        //        return Result<RestaurantStatisticsViewModel>.Failure("Failed to get statistics");
        //    }
        //}
        public async Task<Result<RestaurantStatisticsViewModel>> GetRestaurantStatisticsAsync(Guid restaurantId)
        {
            const int cacheExpirationMinutes = 15;
            var cacheKey = $"RestaurantStats_{restaurantId}";

            if (_memoryCache.TryGetValue(cacheKey, out RestaurantStatisticsViewModel cachedStats))
            {
                return Result<RestaurantStatisticsViewModel>.CreateSuccess(cachedStats);
            }

            try
            {
                var stats = await _unitOfWork.Restaurants
                    .GetWithIncludes(
                        filter: r => r.Id == restaurantId && !r.IsDeleted,
                        includes: r => r.Include(x => x.Dishes)
                                        .Include(x => x.Cartdetails)
                                        .Include(x => x.Reviews)
                                        .Include(x => x.Orders)
                                        .Include(x => x.OrderDetails),


                        tracking: false
                    )
                    .Select(r => new RestaurantStatisticsViewModel
                    {
                        RestaurantId = r.Id,
                        RestaurantName = r.Name,
                        TotalDishes = r.Dishes.Count(d => !d.IsDeleted),
                        TotalOrders = r.Cartdetails.Count(),
                        TotalReviews = r.Reviews.Count,
                        AverageRating = r.Reviews.Any() ?
                            Math.Round(r.Reviews.Average(review => review.Rating), 1) : 0,
                        RecentOrders = r.OrderDetails
                            .Where(o => o.Restaurantid==restaurantId )
                            .Take(10)
                            .Select(o => new OrderSummaryViewModel
                            {
                                OrderId = o.Id,
                                TotalAmount = o.UnitPrice*o.Quantity,
                                Quantity = o.Quantity,
                                dishname=o.Dish.Name,
                                UnitPrice = o.UnitPrice
                            })
                            .ToList(),
                        RecentReviews = r.Reviews
                            .OrderByDescending(r => r.CreatedAt)
                            .Take(5)
                            .Select(r => new ReviewSummaryViewModel
                            {
                                Rating = r.Rating,
                                Comment = r.Comment,
                                CreatedAt = r.CreatedAt
                            })
                            .ToList(),
                        
                            
                    })
                    .FirstOrDefaultAsync();

                if (stats == null)
                {
                    return Result<RestaurantStatisticsViewModel>.Failure("Restaurant not found");
                }

                _memoryCache.Set(cacheKey, stats,
                    new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes)));

                return Result<RestaurantStatisticsViewModel>.CreateSuccess(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for restaurant {RestaurantId}", restaurantId);
                return Result<RestaurantStatisticsViewModel>.Failure("Failed to retrieve statistics");
            }
        }
        public async Task<Result<bool>> SoftDeleteRestaurantAsync(Guid id)
        {
            try
            {
                var Restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
                if (Restaurant == null || Restaurant.IsDeleted)
                    return Result<bool>.Failure("Restaurant not found");

                Restaurant.IsDeleted = true;
                Restaurant.DeletedOn = DateTime.UtcNow;

                _unitOfWork.Restaurants.Update(Restaurant);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting Restaurant {id}");
                return Result<bool>.Failure("Failed to delete Restaurant");
            }
        }

        // Helper methods
        private static byte[]? ConvertBase64ToBytes(string? base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String)) return null;
            try
            {
                return Convert.FromBase64String(base64String);
            }
            catch
            {
                return null;
            }
        }

        private async Task AssignRestaurantClaimAsync(string userId, Guid RestaurantId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString()); // تحويل Guid إلى string
                if (user == null) return;

                var existingClaim = (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(c => c.Type == "RestaurantId");

                if (existingClaim != null)
                    await _userManager.RemoveClaimAsync(user, existingClaim);

                await _userManager.AddClaimAsync(user, new Claim("RestaurantId", RestaurantId.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning claim for user {userId}");
            }
        }

        // Dish methods with similar improvements...
        public async Task<bool> ValidateOwnership(Guid RestaurantId, string userId)
        {
            return await _unitOfWork.Restaurants
                .Get()
                .AnyAsync(r => r.Id == RestaurantId && r.OwnerId == userId);
        }
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public async Task<Result<IEnumerable<Restaurant>>> GetPopularRestaurantsAsync()
        {
            const string cacheKey = "PopularRestaurants";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<Restaurant> cached))
                return Result<IEnumerable<Restaurant>>.CreateSuccess(cached);

            var Restaurants = await _unitOfWork.Restaurants
                .Get()
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.Reviews.Average(x => x.Rating))
                .Take(10)
                .ToListAsync();

            _cache.Set(cacheKey, Restaurants, TimeSpan.FromMinutes(30));

            return Result<IEnumerable<Restaurant>>.CreateSuccess(Restaurants);
        }
        public async Task<Result<PagedResult<Restaurant>>> GetyourRestaurantsPagedAsync(int pageNumber, int pageSize, Guid oweneridd)
        {
            try
            {
                var totalCount = await _unitOfWork.Restaurants
                    .Get()
                    .CountAsync(r => !r.IsDeleted);

                var Restaurants = await _unitOfWork.Restaurants
                    .GetWithIncludes(r => r.City, r => r.RestaurantType)
                    .Where(r => !r.IsDeleted && r.OwnerId == oweneridd.ToString())
                    .OrderBy(r => r.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagedResult = new PagedResult<Restaurant>
                {
                    Items = Restaurants,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                foreach (var restaurant in pagedResult.Items)
                {
                    if (restaurant.City == null)
                    {
                        _logger.LogWarning("Restaurant {Id} has no associated city!", restaurant.Id);
                        restaurant.City = new City { Name = "Unknown" }; // تعيين قيمة افتراضية
                    }
                }
                return Result<PagedResult<Restaurant>>.CreateSuccess(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged Restaurants");
                return Result<PagedResult<Restaurant>>.Failure("Failed to retrieve paged Restaurants");
            }
        }
        public async Task<Result<PagedResult<Restaurant>>> GetRestaurantsPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var totalCount = await _unitOfWork.Restaurants
                    .Get()
                    .CountAsync(r => !r.IsDeleted);

                var Restaurants = await _unitOfWork.Restaurants
                    .GetWithIncludes(r=>r.City,r=>r.RestaurantType)
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagedResult = new PagedResult<Restaurant>
                {
                    Items = Restaurants,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                foreach (var restaurant in pagedResult.Items)
                {
                    if (restaurant.City == null)
                    {
                        _logger.LogWarning("Restaurant {Id} has no associated city!", restaurant.Id);
                        restaurant.City = new City { Name = "Unknown" }; // تعيين قيمة افتراضية
                    }
                }
                return Result<PagedResult<Restaurant>>.CreateSuccess(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged Restaurants");
                return Result<PagedResult<Restaurant>>.Failure("Failed to retrieve paged Restaurants");
            }
        }
        public async Task<Result<SelectList>> GetYourRestaurants()
        {
            try
            {
                var ownerId= GetUserId();
                if(ownerId == null)
                {
                    return Result<SelectList>.Failure("Youshould login");

                }
                var restaurants = await _unitOfWork.Restaurants
                    .GetWithIncludes(r => r.City, r => r.RestaurantType)
                    .Where(r => !r.IsDeleted && r.OwnerId == ownerId) // مقارنة Guid مباشرة
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                if (!restaurants.Any()) // تحسين التحقق من القائمة الفارغة
                {
                    return Result<SelectList>.Failure("You don't own any restaurant.");
                }

                var restaurantList = new SelectList(restaurants, "Id", "Name");

                return Result<SelectList>.CreateSuccess(restaurantList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving restaurants.");
                return Result<SelectList>.Failure("Failed to retrieve restaurants.");
            }
        }



    }





}


