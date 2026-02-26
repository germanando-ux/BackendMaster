using AutoMapper;
using Elastic.CommonSchema;
using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Data.DTO;
using Store.Data.Repositories;
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
        private readonly IUnitOfWork _unitOfWork;


        public VentaService(StoreDbContext context, IConfiguration configuration, IMapper mapper, ILogger<VentaService> logger, IUnitOfWork unitOfWork)
        {
            _context = context;
            _mapper = mapper;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
            _unitOfWork = unitOfWork;
            
        }

        public async Task<int> CrearVentaAsync(VentaCreateDto ventaDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable); // Nivel de aislamiento más estricto para evitar problemas de concurrencia
            try
            {
                

                Dictionary<int, Product> productosDb = new(); // Diccionario para almacenar los precios de los productos desde la DB
                string cacheKey = "products_list"; // La llave que buscaremos en Redis

                // Extraer los IDs de los productos que vienen del Front
                var idsProductos = ventaDto.Detalles.Select(d => d.ProductId).Distinct().ToList();

                // 1. Intentar recuperar de la caché a través de la Unit of Work
                var cachedProducts = await _unitOfWork.Cache.GetAsync<IEnumerable<Product>>(cacheKey);              

                if (cachedProducts != null)
                {
                    // Filtramos solo los productos que nos interesan y convertimos a diccionario
                    productosDb = cachedProducts
                        .Where(p => idsProductos.Contains(p.Id))
                        .ToDictionary(p => p.Id, p => p);

                    // VALIDACIÓN CRÍTICA: ¿Están todos los productos?
                    if (productosDb == null || productosDb.Count != idsProductos.Count)
                    {

                         productosDb = await _context.Products
                            .Where(p => idsProductos.Contains(p.Id))
                            .ToDictionaryAsync(p => p.Id, p => p );
                    }
                }
                else
                {

                     productosDb = await _context.Products
                        .Where(p => idsProductos.Contains(p.Id))
                        .ToDictionaryAsync(p => p.Id, p => p);
                }

            


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
                    // Accedemos al producto en el diccionario
                    var producto = productosDb[detalle.ProductId];

                    detalle.PrecioUnitario = producto.Price;

                    producto.Stock -= detalle.Cantidad; // Reducimos el stock del producto

                    await _unitOfWork.Cache.RemoveAsync($"product:{producto.Id}");

                    _context.Products.Update(producto);
                }


                nuevaVenta.Total = nuevaVenta.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

                // 5. Guardar con EF Core (Esto disparará el Outbox de MassTransit automáticamente)

                _context.Ventas.Add(nuevaVenta);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _unitOfWork.Cache.RemoveAsync(cacheKey);

                _logger.LogInformation("Venta creada exitosamente con ID: {VentaId}", nuevaVenta.Id);
                return nuevaVenta.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al intentar crear una venta para el cliente {Email}", ventaDto.EmailCliente);
                throw; // Re-lanzamos la excepción para que el controlador pueda manejarla
            }
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
