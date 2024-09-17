using developerTask_CurrencyExchange.Models;

namespace developerTask_CurrencyExchange.Services.UserService
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
    }
}
