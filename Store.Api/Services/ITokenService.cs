using Store.Domain.Models;
using System.Security.Claims;

namespace Store.Api.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
