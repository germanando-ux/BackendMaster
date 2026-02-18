using Store.Web.Models;

namespace Store.Web.Services
{
    public interface IAuthService
    {
        // Enviamos las credenciales y esperamos recibir el token si todo va bien
        Task<string?> Login(LoginDto loginDto);

        // Enviamos los datos de registro y esperamos una confirmación de éxito
        Task<bool> Register(RegisterDto registerDto);

        // Un método extra que nos vendrá muy bien para limpiar el estado
        Task Logout();
    }
}
