using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Store.Api.Middlewares;
using Store.Data.DTO;
using Store.Data.Repositories;
using Store.Domain.Models;
using System.Text.Json;

namespace Store.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestión de productos.
    /// Utiliza el patrón Unit of Work para coordinar base de datos y caché.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductsControllerConCacheEnController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsControllerConCacheEnController> _logger;
        private readonly IDistributedCache _cache;

        public ProductsControllerConCacheEnController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductsControllerConCacheEnController> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Obtiene la lista de productos incluyendo su categoría.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {


            string cacheKey = "products_list"; // La llave que buscaremos en Redis
            _logger.LogInformation("Consultando productos...");

            // 1. Intentar obtener los datos de la caché (Redis)
            var cachedProducts = await _cache.GetStringAsync(cacheKey);

            // 2. Si existen en caché, los retornamos directamente
            if (!string.IsNullOrEmpty(cachedProducts))
            {
                _logger.LogInformation("--- Datos recuperados desde CACHÉ (Redis) ---");
                // Deserializamos el JSON que guardamos previamente
                var productsFromCache = JsonSerializer.Deserialize<IEnumerable<ProductResponseDto>>(cachedProducts);
                return Ok(productsFromCache);
            }

            // 3. Si NO hay caché (Cache Miss), vamos a la Base de Datos
            _logger.LogInformation("--- Datos recuperados desde BASE DE DATOS (Postgres) ---");
            // 4. Pedimos al Repositorio los productos e indicamos que incluya la Categoría
            var products = await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Category);
            // 5. Mapeamos la entidad al DTO
            var destination = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductResponseDto>>(products);

            // 6. Guardar el resultado en la caché para la próxima vez
            // Le ponemos un tiempo de expiración de 5 minutos para que no sea eterno
            var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            var serializedProducts = JsonSerializer.Serialize(destination);
            await _cache.SetStringAsync(cacheKey, serializedProducts, cacheOptions);



            return Ok(destination);
        }

        /// <summary>
        /// Crea un nuevo producto y confirma los cambios mediante la Unit of Work.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> PostProduct(ProductCreateDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            // Añadimos el producto al repositorio (esto solo ocurre en memoria)
            await _unitOfWork.Repository<Product>().AddAsync(product);

            // La Unit of Work confirma la transacción en la base de datos
            var result = await _unitOfWork.SaveAsync();

            if (result <= 0) return BadRequest("No se pudo guardar el producto");

            return Ok(_mapper.Map<ProductResponseDto>(product));
        }
    }
}
