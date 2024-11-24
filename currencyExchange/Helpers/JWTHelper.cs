using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class JwtHelper
{
    public static string GetUserIdFromJwtCookie(IRequestCookieCollection cookies)
    {
        if (cookies.ContainsKey("JWToken"))
        {
            var token = cookies["JWToken"];
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");

                if (userIdClaim != null)
                {
                    return userIdClaim.Value; 
                }
            }
        }
        return null;
    }
}
