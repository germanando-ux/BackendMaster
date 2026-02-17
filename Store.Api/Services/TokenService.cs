using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Store.Domain.Models;

namespace Store.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config)
        {
            _config = config;
        }
        /// <summary>
        /// Creación de un token JWT utilizando una lista de claims proporcionada.
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public string CreateToken(User user)
        {
            //Preparamos los Claims basados en las propiedades del usuario
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // Si usas roles
            };

            //Crea la clave asimetrica
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            //definir las credenciales de firma (algorimo HMAC SHA256)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            //configurar el cuerpo del token (payload)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // El token expira en 1 hora
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            //generar y escribir el token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
