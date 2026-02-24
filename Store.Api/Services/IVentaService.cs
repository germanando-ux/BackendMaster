using Store.Data.DTO;
namespace Store.Api.Services
{
    public interface IVentaService
    {
        /// <summary>
        /// Procesa la creación de una venta: valida productos, calcula totales y guarda en DB.
        /// </summary>
        /// <param name="ventaDto">Datos de la venta enviados desde el Front.</param>
        /// <returns>El ID de la venta generada.</returns>
        Task<int> CrearVentaAsync(VentaCreateDto ventaDto);

        /// <summary>
        /// Obtiene una venta específica con todos sus detalles para mostrar un recibo.
        /// </summary>
        Task<VentaReadDto?> ObtenerVentaPorIdAsync(int id);
    }
}
