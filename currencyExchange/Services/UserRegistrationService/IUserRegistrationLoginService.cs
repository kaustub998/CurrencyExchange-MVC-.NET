using currencyExchange.Models;

namespace currencyExchange.Services.UserRegistrationService
{
    public interface IUserRegistrationLoginService
    {
        Task<ResponseModel> UserRegistration(UserRegister RegistrationData);
        Task<SessionDetails> UserLogin(UserLogin LoginData);
    }
}
