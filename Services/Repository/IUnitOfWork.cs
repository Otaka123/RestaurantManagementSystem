using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using UsersApp.Models;
using DishModel = UsersApp.Models.Dish;
namespace UsersApp.Services.Repository
{
    public interface IUnitOfWork: IDisposable
    {
        IGenericRepository<Restaurant> Restaurants { get; }
        IGenericRepository<DishModel> Dishs { get; }
        IGenericRepository<RestaurantSchedule> RestaurantSchedules { get; }
        IGenericRepository<City> Citys { get; }
        IGenericRepository<RestaurantType> RestaurantTypes { get; }
        IGenericRepository<DishCategory> DishCategorys { get; }
        IGenericRepository <Reviews> Reviews { get; set; }
        IGenericRepository<ShoppingCart> ShoppingCarts { get; set; }
        IGenericRepository<CartDetail> CartDetails { get; set; }
        IGenericRepository<UsersApp.Models.Order> Orders { get; set; }
        IGenericRepository<OrderDetail> OrderDetails { get; set; }
        IGenericRepository<OrderStatus> orderStatuses { get; set; }


        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess=false, CancellationToken cancellationToken = default);
        Task<int> CompleteAsync();
        //IQueryable<T> GetWithIncludes<T>(params Expression<Func<T, object>>[] includes) where T : class;
        Task<int> CommitAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        void Rollback();
    }
}
