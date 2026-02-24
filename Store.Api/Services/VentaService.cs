using Store.Data.DTO;
using Store.Web.Models;

namespace Store.Api.Services
{
    public class VentaService : IVentaService
    {
        public Task<int> CrearVentaAsync(VentaCreateDto ventaDto)
        {
            // 1. Extraer los IDs de los productos que vienen del Front
            var idsProductos = ventaDto.Detalles.Select(d => d.ProductoId).Distinct().ToList();

            using (var db = GetConnection())
            {
                // 2. Consultar todos los productos de una vez con Dapper
                // Usamos "WHERE Id IN @ids" que Dapper maneja automáticamente
                var productosDb = (await db.QueryAsync<dynamic>(
                    "SELECT Id, Price, Name FROM Products WHERE Id IN @ids",
                    new { ids = idsProductos }
                )).ToList();

                // 3. VALIDACIÓN CRÍTICA: ¿Están todos los productos?
                if (productosDb.Count != idsProductos.Count)
                {
                    // Podríamos identificar cuál falta, pero por ahora lanzamos excepción general
                    throw new Exception("Uno o más productos seleccionados no existen en el catálogo.");
                }

                // 4. Aquí ya tenemos los precios reales (productosDb)
                // Ahora toca construir la entidad Venta y sus detalles para EF Core...
            }

            return 0; // Temporal
        }

        public Task<VentaReadDto?> ObtenerVentaPorIdAsync(int id)
        {
            throw new NotImplementedException();
        }

    }
}
