namespace OrdersApi.Models;

public class PaymentRequest
{
    public string UserName { get; set; }
    public decimal Amount { get; set; }
}