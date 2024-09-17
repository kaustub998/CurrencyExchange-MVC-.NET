using developerTask_CurrencyExchange.Models;

namespace developerTask_CurrencyExchange.Services
{
    public interface IUserRegistrationLoginService
    {
        Task<ResponseModel> UserRegistration(UserRegister RegistrationData);
        Task<SessionDetails> UserLogin(UserLogin LoginData);
    }
}
