using OrdersApi.Models;

namespace OrdersApi.Repositories.Interfaces;

public interface IOrderRepository
{
    Task CreateAsync(Order order);
    Task<bool> Save(Order order);
}