using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace UsersApp.Services.Repository
{
    public interface IGenericRepository<T> where T : class
    {

        IQueryable<T> Find(
     Expression<Func<T, bool>> condition,
         bool asNoTracking = false,
     params Expression<Func<T, object>>[] includes);
        IQueryable<T> Get(); // ✅ لاسترجاع البيانات باستخدام `IQueryable`
        IQueryable<T> GetWithIncludes(
       Expression<Func<T, bool>> filter = null,
       Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null,
       bool tracking = false);
        IQueryable<T> GetWithIncludes(params Expression<Func<T, object>>[] includes);
        Task AddRangeAsync(IEnumerable<T> list);
        //Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveChangesAsync();
        void RemoveRange(IEnumerable<T> list);

    }
}
