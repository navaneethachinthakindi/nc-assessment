using System.Threading.Tasks;
using TodoList.Api.Repositories;

namespace TodoList.Api
{
    public interface IUnitOfWork
    {
        GenericRepository<TodoItem> TodoItemRepository {get;}
        Task Save();

    }
}