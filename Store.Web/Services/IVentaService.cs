using Store.Web.Models;

namespace Store.Web.Services
{
    public interface IVentaService
    {
        Task<int> CrearVentaAsync(VentaCreateDto ventaDto);
    }
}
