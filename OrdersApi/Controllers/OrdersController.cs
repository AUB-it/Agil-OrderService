using Microsoft.AspNetCore.Mvc;
using OrdersApi.Models;
using OrdersApi.Repositories.Interfaces;
using OrdersApi.Services.Interfaces;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPaymentService _paymentService;
    private readonly IOrderRepository _orderRepository;

    public OrdersController(IUserService userService, IPaymentService paymentService, IOrderRepository orderRepository)
    {
        _userService = userService;
        _paymentService = paymentService;
        _orderRepository = orderRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order? order)
    {
        if (order is  null)
            return BadRequest("Order missing from request");
        
        if (string.IsNullOrWhiteSpace(order.UserId))
            return BadRequest("UserId missing from order");
        
        if (order.TotalAmount <= 0)
            return BadRequest("TotalAmount must be greater than zero");
        if (order.TotalAmount >= 10000)
            return BadRequest("TotalAmount too large");
        
        try
        {
            var user = await _userService.GetUserById(order.UserId);
            if (user is null)
                return NotFound("User for order not found");
            
            
            var paymentSuccess = await _paymentService.ProcessOrder(order.UserId, order.TotalAmount);
            if (!paymentSuccess)
                return BadRequest("Payment failed");
            
            var orderProcessSuccess = await _orderRepository.Save(order);
            if (!orderProcessSuccess)
                return BadRequest("Order processing failed");
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            var objectResult = new ObjectResult(e.Message);
            objectResult.StatusCode = 500;
            return objectResult;
        }

        return CreatedAtAction(nameof(CreateOrder), order);
    }
    
}
