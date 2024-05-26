using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace TodoList.Api.Repositories
{

    public class GenericRepository<T> : IGenericRepository<T> where T : class   
    {
        internal TodoContext context;
        internal DbSet<T> dbSet;
        
        public GenericRepository(TodoContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
                await dbSet.AddAsync(entity);
        }

        public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
        {
                IQueryable<T> query = dbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                if (orderBy != null)
                {
                    return  orderBy(query).ToListAsync();
                }
                else
                {
                    return query.ToListAsync();
                }
        }

        public async Task<T> GetByIDAsync(Guid id)
        {
            return await dbSet.FindAsync(id);
        }
        
        public async Task Remove(Guid id)
        {
                T entityToDelete = await dbSet.FindAsync(id);
                Remove(entityToDelete);
        }

        public void Remove(T entity)
        {
                if (context.Entry(entity).State == EntityState.Detached)
                {
                    dbSet.Attach(entity);
                }
                dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
                dbSet.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return await dbSet.AnyAsync(filter);
        }
    }
}