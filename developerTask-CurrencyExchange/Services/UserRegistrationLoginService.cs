﻿using developerTask_CurrencyExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace developerTask_CurrencyExchange.Services
{
    public class UserRegistrationLoginService : IUserRegistrationLoginService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserRegistrationLoginService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<ResponseModel> UserRegistration(UserRegister RegistrationData)
        {
            var EmailCollision = await _context.Users.Where(item => item.Email.ToLower() == RegistrationData.Email.ToLower() && item.IsDeleted != true).FirstOrDefaultAsync();
            var response = new ResponseModel();

            try
            {
                if (EmailCollision == null)
                {
                    var User = new UserRegister
                    {
                        FirstName = RegistrationData.FirstName,
                        LastName = RegistrationData.LastName,
                        Email = RegistrationData.Email,
                        SaltKey = Guid.NewGuid(),
                    };

                    User.Password = CreatePasswordHash(RegistrationData.Password, User.SaltKey);

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

        private string GenerateToken(User user)
        {
            try
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                };

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:WebTokenSecret").Value));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
                var token = new JwtSecurityToken(
                    claims: claims,
                    signingCredentials: creds,
                    expires: DateTime.Now.AddHours(2));
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                return jwt;

            }
            catch (Exception ex)
            {
                throw;
            }
            return null;

        }

        public async Task<SessionDetails> UserLogin(UserLogin LoginData)
        {
            SessionDetails sd = new SessionDetails();
            var user = await _context.Users.Where(x => x.Email.ToLower() == LoginData.Email.ToLower().Trim() && x.IsDeleted != true).FirstOrDefaultAsync();
            if (user != null)
            {
                if (user.PasswordHash == CreatePasswordHash(LoginData.Password, user.SaltKey))
                {
                    string token = GenerateToken(user);
                    sd.userId = user.UserId;
                    sd.tokenId = token;
                    sd.firstName = user?.FirstName ?? "";
                    sd.lastName = user?.LastName ?? "";
                    sd.userRoleId = user.IsAdmin;
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
