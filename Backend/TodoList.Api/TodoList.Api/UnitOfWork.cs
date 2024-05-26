using System;
using System.Threading.Tasks;
using TodoList.Api.Repositories;

namespace TodoList.Api
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        
        private TodoContext context;
        private GenericRepository<TodoItem> todoItemRepository;

        public UnitOfWork(TodoContext databaseContext){
            context = databaseContext;
        }

        public GenericRepository<TodoItem> TodoItemRepository
        {
            get
            {
                if (this.todoItemRepository == null)
                {
                    this.todoItemRepository = new GenericRepository<TodoItem>(context);
                }
                return todoItemRepository;
            }
        }

        public async Task Save()
        {
            await context.SaveChangesAsync();
        }
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}