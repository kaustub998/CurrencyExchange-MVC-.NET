using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using currencyExchange.Models;
using System.Text;

namespace currencyExchange.Services.JWTauthenticationServices
{
    public class JWTService
    {
        private readonly IConfiguration _config;
        public JWTService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            try
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:WebTokenSecret").Value));
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

        public string ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:WebTokenSecret").Value);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero

                }, out SecurityToken validatedToken);

                // Corrected access to the validatedToken
                var jwtToken = (JwtSecurityToken)validatedToken;

                return "";
            }
            catch
            {
                return null;
            }
        }
    }
}
