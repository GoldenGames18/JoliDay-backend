using JoliDay.Models;
using System.IdentityModel.Tokens.Jwt;

namespace JoliDay.Services
{
    public interface IServiceToken
    {
        JwtSecurityToken GenereteToken(User user);


    }
}
