using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TodoList.Api.Controllers;
using Xunit;

namespace TodoList.Api.UnitTests
{
    public class DummyTestShould
    {

        private TodoItemsController todoItemsController;

        public DummyTestShould()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(databaseName: "TodoItemsDBTest")
            .Options;
            var logger=new Mock<ILogger<TodoItemsController>>();
            var testDbContext=new TodoContext(options);
            todoItemsController= new TodoItemsController(new UnitOfWork(testDbContext),logger.Object);
        }

        [Theory]
        [InlineData("TodoList Item Test 1", StatusCodes.Status201Created)]
        [InlineData("TodoList Item Test 2", StatusCodes.Status201Created)]
        [InlineData("",StatusCodes.Status400BadRequest)]
        [InlineData("TodoList Item Test 1",StatusCodes.Status400BadRequest)]
        public async void Test_AddTodoItem(string description, int expectedStatus)
        {
            var result= await todoItemsController.PostTodoItem(new TodoItem{ Description = description});

            var statusResult=(IStatusCodeActionResult)result;
            Assert.Equal(expectedStatus, statusResult.StatusCode);
        }

        [Fact]
        public async void Test_GetTodoItems()
        {
            var result= await todoItemsController.GetTodoItems();

            var statusResult=(IStatusCodeActionResult)result;
            Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        }

        
        [Theory]
        [InlineData(true, StatusCodes.Status200OK)]
        [InlineData(false,StatusCodes.Status400BadRequest)]
        public async void Test_GetTodoItem(bool isExists, int expectedStatus)
        {
            IActionResult result;
            if(isExists)
            {
                var data=Assert.IsAssignableFrom<OkObjectResult>(await todoItemsController.GetTodoItems());
                var todoItemsList=Assert.IsAssignableFrom<List<TodoItem>>(data.Value);
                result = await todoItemsController.GetTodoItem(todoItemsList.First().Id);
            }
            else{
                result = await todoItemsController.GetTodoItem(new Guid());
            }

            var statusResult=(IStatusCodeActionResult)result;
            Assert.Equal(expectedStatus, statusResult.StatusCode);
        }
        [Theory]
        [InlineData(1, StatusCodes.Status200OK,"TodoList Item Update Test")]
        [InlineData(1, StatusCodes.Status400BadRequest,"")]
        [InlineData(1, StatusCodes.Status400BadRequest,"TodoList Item Test 2")]
        [InlineData(2,StatusCodes.Status400BadRequest,"TodoList Item Update Test")]
        [InlineData(3,StatusCodes.Status400BadRequest,"TodoList Item Update Test")]
        public async void Test_UpdateTodoItem(int testCase, int expectedStatus, string description)
        {
            IActionResult result;
            
            if(testCase==3)
            {
                
                result = await todoItemsController.PutTodoItem(new Guid(), new TodoItem(){
                    Description=description,
                    Id=new Guid()
                });
            }
            else{
                
                var data=Assert.IsAssignableFrom<OkObjectResult>(await todoItemsController.GetTodoItems());
                var todoItemsList=Assert.IsAssignableFrom<List<TodoItem>>(data.Value);
                var id = testCase == 1 ? todoItemsList.First().Id: new Guid(); 
                result = await todoItemsController.PutTodoItem(id, todoItemsList.First());
            }

            var statusResult=(IStatusCodeActionResult)result;
            Assert.Equal(expectedStatus, statusResult.StatusCode);
        }
    }
}
