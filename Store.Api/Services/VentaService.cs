using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Data.DTO;
using Store.Domain.Models;
using System.Data;

namespace Store.Api.Services
{
    public class VentaService : IVentaService
    {
        private readonly StoreDbContext _context;
        private readonly string _connectionString;
        private readonly IMapper _mapper;
        private readonly ILogger<VentaService>   _logger;

        public VentaService(StoreDbContext context, IConfiguration configuration, IMapper mapper, ILogger<VentaService> logger)
        {
            _context = context;
            _mapper = mapper;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public async Task<int> CrearVentaAsync(VentaCreateDto ventaDto)
        {
            // 1. Extraer los IDs de los productos que vienen del Front
            var idsProductos = ventaDto.Detalles.Select(d => d.ProductoId).Distinct().ToList();


            var productosDb = await _context.Products
                .Where(p => idsProductos.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Price);

         

            // 3. VALIDACIÓN CRÍTICA: ¿Están todos los productos?
            if (productosDb.Count != idsProductos.Count)
            {
                // Podríamos identificar cuál falta, pero por ahora lanzamos excepción general
                throw new Exception("Uno o más productos seleccionados no existen en el catálogo.");
            }

      
            // 3. MAPEO con AutoMapper: De DTO a Entidad de Dominio
            var nuevaVenta = _mapper.Map<Venta>(ventaDto);
            nuevaVenta.Fecha = DateTime.UtcNow;

            // 5. ASIGNACIÓN DE PRECIOS Y TOTAL:
            // Recorremos los detalles mapeados para asignar el precio real de la DB
            foreach (var detalle in nuevaVenta.Detalles)
            {
                detalle.PrecioUnitario = productosDb[detalle.ProductoId];
            }

            nuevaVenta.Total = nuevaVenta.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            // 5. Guardar con EF Core (Esto disparará el Outbox de MassTransit automáticamente)
            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            return nuevaVenta.Id;
            

            
        }

    
        public async Task<VentaReadDto?> ObtenerVentaPorIdAsync(int id)
        {
            // 1. Consultamos la base de datos usando Eager Loading
            var ventaEntity = await _context.Ventas
                .Include(v => v.Detalles)           // Cargamos la lista de detalles
                    .ThenInclude(d => d.Product)    // De cada detalle, cargamos su objeto Producto
                .AsNoTracking()                     // Mejora rendimiento (solo lectura)
                .FirstOrDefaultAsync(v => v.Id == id);

            // 2. Si no existe, devolvemos null
            if (ventaEntity == null) return null;

            // 3. Mapeamos la entidad al DTO de lectura
            // AutoMapper se encargará de usar la regla src.Product.Name que pusimos antes
            return _mapper.Map<VentaReadDto>(ventaEntity);
        }
    }
}
