using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq.Expressions;
using UsersApp.Data;

namespace UsersApp.Services.Repository
{
    public class GenericRepository<T> :IGenericRepository<T> where T : class
    {

            protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public IQueryable<T> GetWithIncludes(
       Expression<Func<T, bool>> filter = null,
       Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null,
       bool tracking = false)
        {
            IQueryable<T> query = _context.Set<T>();

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            if (includes != null)
            {
                query = includes(query);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }
        public IQueryable<T> GetWithIncludes(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        public IQueryable<T> Get()
        {
            return _dbSet.AsQueryable();
        }
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        public IQueryable<T> Find(
     Expression<Func<T, bool>> condition,
         bool asNoTracking = false,
     params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query.Where(condition);
        }
        public async Task AddRangeAsync(IEnumerable<T> list)
        {
             await _dbSet.AddRangeAsync(list);
        }
        //public async Task RemoveRange(IEnumerable<T> list)
        //{
        //    await _dbSet.RemoveRange(list);
        //}
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public void RemoveRange(IEnumerable<T> list)
        {
            _dbSet.RemoveRange(list);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}

