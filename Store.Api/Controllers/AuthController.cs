using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Api.Services;
using Store.Data.Data;
using Store.Data.DTO;
using Store.Domain.Models;

namespace Store.Api.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly StoreDbContext _context;

        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Hace el login de un usuario, si el correo electrónico y la contraseña son correctos devuelve un token JWT con la información del usuario y su rol
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            //buscar el usuario por correo electrónico
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email.ToLower());
            //si no existe devolvemos 401 (Unauthorized)
            if (user == null)
            {
                return Unauthorized("Correo electrónico o contraseña incorrectos.");
            }

            // 3. Recrear el Hash usando el Salt guardado
            // Pasamos el Salt al constructor para que la "trituradora" use la misma configuración
            using var hmac = new System.Security.Cryptography.HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDto.Password));

            //comparar el hash generado con el de la base de datos byte a byte
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Correo electrónico o contraseña incorrectos.");
                }
            }

            return Ok(_tokenService.CreateToken(user));
        }

        /// <summary>
        /// da de alta a un nuevo usuario, se le asigna el rol de "Seller" por defecto
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>

        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDTO registerDto)
        {
            //validadar si el correo ya existe
            if (await _context.Users.AnyAsync(x => x.Email == registerDto.EmailAdress.ToLower()))
            {
                return BadRequest("El correo electrónico ya está en uso.");
            }

            using var hmac = new System.Security.Cryptography.HMACSHA512();

            var user = new User
            {
                Email = registerDto.EmailAdress.ToLower(),
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key,
                Role = "Seller" // Rol por defecto
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // crear el hash y el salt usando HMACSHA512
            return Ok("Usuario registrado correctamente");
        }
    }
}
