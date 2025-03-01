using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using System.Threading;
using UsersApp.Data;
using UsersApp.Models;
using DishModel = UsersApp.Models.Dish;
namespace UsersApp.Services.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _currentTransaction;
        public IGenericRepository<Restaurant> Restaurants { get; }
        public IGenericRepository<DishModel> Dishs { get; }
        public IGenericRepository<DishCategory> DishCategorys { get; }
  
        public IGenericRepository<RestaurantSchedule> RestaurantSchedules { get; }
        public IGenericRepository<Reviews> Reviews { get; set; }

        public IGenericRepository<City> Citys { get; }
        public IGenericRepository<RestaurantType> RestaurantTypes { get; }


        public IGenericRepository<ShoppingCart> ShoppingCarts { get; set; }
        public IGenericRepository<CartDetail> CartDetails { get; set; }
        public IGenericRepository<UsersApp.Models.Order> Orders { get; set; }
        public IGenericRepository<OrderDetail> OrderDetails { get; set; }
        public IGenericRepository<OrderStatus> orderStatuses { get; set; }


        public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Citys= new GenericRepository<City>(context);
        RestaurantTypes= new GenericRepository<RestaurantType>(context);
        Restaurants = new GenericRepository<Restaurant>(context);
        Dishs = new GenericRepository<DishModel>(context);
        RestaurantSchedules = new GenericRepository<RestaurantSchedule>(context);
        DishCategorys=new GenericRepository<DishCategory>(context);
        Reviews=new GenericRepository<Reviews>(context);
        ShoppingCarts=new GenericRepository<ShoppingCart>(context);
        CartDetails=new GenericRepository<CartDetail>(context);
        Orders=new GenericRepository<UsersApp.Models.Order>(context);
        OrderDetails=new GenericRepository<OrderDetail>(context);
        orderStatuses=new GenericRepository<OrderStatus>(context);

    }


      

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();

        }




        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }
                return result;
            }
            catch
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }
        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess=false,CancellationToken cancellationToken=default)
        {
            try
            {
                var result = await _context.SaveChangesAsync(acceptAllChangesOnSuccess,cancellationToken);
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
                return result;
            }
            catch
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public void Rollback()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void Disposet()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}
