using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TodoList.Api.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "");
        Task<T> GetByIDAsync(Guid id);
        Task CreateAsync(T entity);
        Task Remove(Guid id);
        void Remove(T entity);
        void Update(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    }
}