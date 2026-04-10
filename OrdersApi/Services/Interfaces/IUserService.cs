using OrdersApi.Models;

namespace OrdersApi.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserById(string userId);
}