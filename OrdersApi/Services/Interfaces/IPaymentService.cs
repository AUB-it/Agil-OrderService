using OrdersApi.Models;

namespace OrdersApi.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPayment(PaymentRequest request);
    Task<bool> ProcessOrder(string userId, decimal totalAmount);
}