using currencyExchange.Models;
using currencyExchange.Services.JWTauthenticationServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace currencyExchange.Services.UserRegistrationService
{
    public class UserRegistrationLoginService : IUserRegistrationLoginService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly JWTService _jWTService;

        public UserRegistrationLoginService(AppDbContext context, IConfiguration config, JWTService jWTService)
        {
            _context = context;
            _config = config;
            _jWTService = jWTService;
        }

        public async Task<ResponseModel> UserRegistration(UserRegister registrationData)
        {
            var EmailCollision = await _context.Users.Where(item => item.Email.ToLower() == registrationData.Email.ToLower() && item.IsDeleted != true).FirstOrDefaultAsync();
            var response = new ResponseModel();

            try
            {
                if (EmailCollision == null)
                {
                    var User = new UserRegister
                    {
                        FirstName = registrationData.FirstName,
                        LastName = registrationData.LastName,
                        Email = registrationData.Email,
                        SaltKey = Guid.NewGuid(),
                        IsAdmin = registrationData.IsAdmin,
                    };

                    User.PasswordHash = CreatePasswordHash(registrationData.Password, User.SaltKey);

                    _context.Users.Add(User);
                    await _context.SaveChangesAsync();

                    response.isSuccess = true;
                    response.isError = false;
                    response.message = "";
                }
                else
                {
                    response.isSuccess = false;
                    response.isError = true;
                    response.message = "Email Already Exists";
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.isError = true;
                response.message = "Something Went Wrong";
            }

            return response;
        }

        private static string CreatePasswordHash(string plainPassword, Guid guidSaltKey)
        {
            var guidSaltedPassword = string.Concat(plainPassword, guidSaltKey);
            return CryptoService.CreatePasswordHash(guidSaltedPassword);
        }

        public async Task<SessionDetails> UserLogin(UserLogin loginData)
        {
            SessionDetails sd = new SessionDetails();
            var user = await _context.Users.Where(x => x.Email.ToLower() == loginData.Email.ToLower().Trim() && x.IsDeleted != true).FirstOrDefaultAsync();
            if (user != null)
            {
                if (user.PasswordHash == CreatePasswordHash(loginData.Password, user.SaltKey))
                {
                    string token = _jWTService.GenerateToken(user);
                    sd.userId = user.UserId;
                    sd.tokenId = token;
                    sd.firstName = user?.FirstName ?? "";
                    sd.lastName = user?.LastName ?? "";
                    sd.userRoleId = user.IsAdmin;

                    user.LastLogin = DateTime.UtcNow;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                }
                else
                {
                    sd.message = "Password do not match!!!";
                    sd.userId = user.UserId;
                    sd.tokenId = "";
                }
            }
            else
            {
                sd.userId = 0;
                sd.message = "User Not Found !!!";
                sd.tokenId = "";
            }

            return sd;
        }
    }
}
