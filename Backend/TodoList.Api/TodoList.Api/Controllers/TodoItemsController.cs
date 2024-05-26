using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TodoList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(IUnitOfWork unitOfWork, ILogger<TodoItemsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<IActionResult> GetTodoItems()
        {
            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\GetTodoItems start");
            var results = await _unitOfWork.TodoItemRepository.GetAllAsync(filter: x => !x.IsCompleted);
            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\GetTodoItems end");
            return Ok(results);
        }

        // GET: api/TodoItems/...
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItem(Guid id)
        {
            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\GetTodoItem {id} start");
            var result = await _unitOfWork.TodoItemRepository.GetByIDAsync(id);

            if (result == null)
            {
            _logger.LogError($"Error: {DateTime.Now}: TodoItems\\GetTodoItem : {id} is not found");
                return NotFound();
            }

            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\GetTodoItem {id} end");
            return Ok(result);
        }

        // PUT: api/TodoItems/... 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="todoItem"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(Guid id, TodoItem todoItem)
        {
            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\PutTodoItem {id} start");
            if (id != todoItem.Id)
            {
            _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : {id} & {todoItem.Id} are not same");
                return BadRequest();
            }
            
            if (string.IsNullOrWhiteSpace(todoItem?.Description))
            {
                _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : error while updating TodoItem, Description is empty");
                return BadRequest("Description is required");
            }
            else 
            {
                var result = await TodoItemDescriptionExists(todoItem.Description, todoItem.Id);
                if(result)
                {
                    _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : error while updating TodoItem, Description is already exists");
                    return BadRequest("Description already exists");
                }
            } 

            try
            {
                _unitOfWork.TodoItemRepository.Update(todoItem);
                await _unitOfWork.Save();
            }
            catch(DbUpdateConcurrencyException ex )
            {

                var result=await TodoItemIdExists(id);
                if (result)
                {
                    
                    _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : {id} database error while updating, exception : {ex.Message}");
                    _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\PutTodoItem {id} end");
                    return BadRequest();
                }
                else{
                    _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : {id} is not exists");
                    _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\PutTodoItem {id} end");
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : {id} error while updating, exception : {ex.Message}");
                _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\PutTodoItem {id} end");
                return BadRequest();
            }

            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\PutTodoItem {id} end");
            return NoContent();
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoItem"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostTodoItem(TodoItem todoItem)
        {
            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\PutTodoItem new todoitem start");
            if (string.IsNullOrWhiteSpace(todoItem?.Description))
            {
                _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : error while adding new TodoItem, Description is empty");
                return BadRequest("Description is required");
            }
            else 
            {
                var result = await TodoItemDescriptionExists(todoItem.Description);
                if(result)
                {
                    _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : error while adding new TodoItem, Description is already exists");
                    return BadRequest("Description already exists");
                }
            } 


            try{
                todoItem.Id=new Guid();
                await _unitOfWork.TodoItemRepository.CreateAsync(todoItem);
                await _unitOfWork.Save();
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {DateTime.Now}: TodoItems\\PutTodoItem : error while adding new TodoItem, exception : {ex.Message}");
                return BadRequest();
            }
             
            _logger.LogDebug($"Debug: {DateTime.Now}: TodoItems\\PutTodoItem new todoitem end");
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        } 

        private async Task<bool> TodoItemIdExists(Guid id)
        {
            var result =await _unitOfWork.TodoItemRepository.AnyAsync(filter: x => x.Id == id);
            return result;
        }

        private  async Task<bool> TodoItemDescriptionExists(string description, Guid? id = null)
        {
            if(id.HasValue)
            {
                var result =await _unitOfWork.TodoItemRepository.AnyAsync(filter: x => x.Description.ToLowerInvariant() == description.ToLowerInvariant() && !x.IsCompleted && x.Id != id);
                return result;
            }
            else{
                var result =await _unitOfWork.TodoItemRepository.AnyAsync(filter: x => x.Description.ToLowerInvariant() == description.ToLowerInvariant() && !x.IsCompleted);
                return result;
            }
        }
    }
}
