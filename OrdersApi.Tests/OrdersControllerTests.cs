using Microsoft.AspNetCore.Mvc;
using Moq;
using OrdersApi.Controllers;
using OrdersApi.Models;
using OrdersApi.Repositories.Interfaces;
using OrdersApi.Services.Interfaces;

namespace OrdersApi.Tests;

[TestClass]
public sealed class OrdersControllerTests
{
    private Mock<IUserService> _userServiceMock = null!;
    private Mock<IPaymentService> _paymentServiceMock = null!;
    private Mock<IOrderRepository> _orderRepositoryMock = null!;
    private OrdersController _controller = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _orderRepositoryMock = new Mock<IOrderRepository>();

        _controller = new OrdersController(
            _userServiceMock.Object,
            _paymentServiceMock.Object,
            _orderRepositoryMock.Object);
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderIsNull()
    {
        var result = await _controller.CreateOrder(null);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenUserIdIsMissing()
    {
        var order = new Order { UserId = "", TotalAmount = 100 };
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var order = new Order { UserId = "john", TotalAmount = 100 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync((User)null);
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenRepositoryFails()
    {
        var order = new Order { UserId = "john", TotalAmount = 100 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount))
            .ReturnsAsync(true);
        _orderRepositoryMock.Setup(x => x.Save(It.IsAny<Order>()))
            .ReturnsAsync(false);
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturnCreatedAtAction_WhenOrderIsCreated()
    {
        var order = new Order { UserId = "john", TotalAmount = 100 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount))
            .ReturnsAsync(true);
        _orderRepositoryMock.Setup(x => x.Save(It.IsAny<Order>()))
            .ReturnsAsync(true);
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturn500_WhenRepositoryThrowsException()
    {
        var order = new Order { UserId = "john", TotalAmount = 100 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount)).ReturnsAsync(true);
        _orderRepositoryMock.Setup(x => x.Save(It.IsAny<Order>())).ThrowsAsync(new Exception());
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenPaymentFails()
    {
        var order = new Order { UserId = "john", TotalAmount = 100 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount)).ReturnsAsync(false);
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturn500_WhenPaymentServiceThrows()
    {
        var order = new Order { UserId = "john", TotalAmount = 100 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount))
            .ThrowsAsync(new Exception());
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
    }
    
    [TestMethod]
    public async Task CreateOrder_ShouldReturnCreatedAtAction_WhenPaymentSucceeds()
    {
        var order = new Order { UserId = "john", TotalAmount = 200 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount))
            .ReturnsAsync(true);
        _orderRepositoryMock.Setup(x => x.Save(It.IsAny<Order>()))
            .ReturnsAsync(true);
        var result = await _controller.CreateOrder(order);
        Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
    }

    [TestMethod]
    public async Task CreateOrderShouldReturnBadRequestOrderAmountIsZeroOrNegative()
    {
        var order = new Order { UserId = "john", TotalAmount = 0 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount))
            .ReturnsAsync(true);
        _orderRepositoryMock.Setup(x => x.Save(It.IsAny<Order>()))
            .ReturnsAsync(true);
        
        var result = await _controller.CreateOrder(order);
        
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
    
    [TestMethod]
    public async Task CreateOrderShouldReturn500WhenUserServiceThrowsException()
    {
        var order = new Order { UserId = "john", TotalAmount = 0 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount))
            .ReturnsAsync(true);
        _orderRepositoryMock.Setup(x => x.Save(It.IsAny<Order>()))
            .ReturnsAsync(true);
        
        var result = await _controller.CreateOrder(order);
        
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
    
    [TestMethod]
    public async Task CreateOrderShouldReturnBadRequestWhenOrderAmountTooLarge()
    {
        var order = new Order { UserId = "john", TotalAmount = 10000 };
        _userServiceMock.Setup(x => x.GetUserById(order.UserId))
            .ReturnsAsync(new User { Name = "john", IsActive = true });
        _paymentServiceMock.Setup(x => x.ProcessOrder(order.UserId, order.TotalAmount))
            .ReturnsAsync(true);
        _orderRepositoryMock.Setup(x => x.Save(It.IsAny<Order>()))
            .ReturnsAsync(true);
        
        var result = await _controller.CreateOrder(order);
        
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
    
}
