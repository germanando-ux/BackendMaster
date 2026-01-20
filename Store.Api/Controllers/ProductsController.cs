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
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductsController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;            
        }

        /// <summary>
        /// Obtiene la lista de productos incluyendo su categoría.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {


            string cacheKey = "products_list"; // La llave que buscaremos en Redis
            _logger.LogInformation("Consultando productos...");

            // 1. Intentar recuperar de la caché a través de la Unit of Work
            var cachedProducts = await _unitOfWork.Cache.GetAsync<IEnumerable<ProductResponseDto>>(cacheKey);
            // 2. Si existen en caché, los retornamos directamente
            if (cachedProducts != null)
            {
                _logger.LogInformation("--- Datos recuperados desde CACHÉ ---");
                return Ok(cachedProducts);
            }

            
            // 3. Si NO hay caché (Cache Miss), vamos a la Base de Datos
            _logger.LogInformation("--- Datos recuperados desde BASE DE DATOS (Postgres) ---");
            // 4. Pedimos al Repositorio los productos e indicamos que incluya la Categoría
            var products = await _unitOfWork.Repository<Product>().GetAllAsync(p => p.Category);
            // 5. Mapeamos la entidad al DTO
            var destination = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductResponseDto>>(products);

            // 3. Guardar en caché para futuras peticiones (Expiración de 10 min)
            await _unitOfWork.Cache.SetAsync(cacheKey, destination, TimeSpan.FromMinutes(10));

            return Ok(destination);


        }


        /// <summary>
        /// Obtiene un producto específico por su ID.
        /// Implementa el patrón Cache-Aside con llaves dinámicas.
        /// </summary>
        /// <param name="id">ID del producto a buscar.</param>
        /// <returns>El producto solicitado o 404 si no existe.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
        {
            // Creamos la llave dinámica usando el ID del producto
            string cacheKey = $"product:{id}";
            _logger.LogInformation($"Buscando producto {id}...");

            // 1. Intentar recuperar de la caché (Unit of Work -> CacheService)
            var cachedProduct = await _unitOfWork.Cache.GetAsync<ProductResponseDto>(cacheKey);

            if (cachedProduct != null)
            {
                _logger.LogInformation($"--- Producto {id} recuperado desde CACHÉ ---");
                return Ok(cachedProduct);
            }

            // 2. Si no está en caché (Cache Miss), vamos a la DB
            _logger.LogInformation($"--- Producto {id} recuperado desde BASE DE DATOS ---");

            // Buscamos incluyendo la categoría para que el DTO esté completo
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, p => p.Category);

            if (product == null)
            {
                _logger.LogWarning($"Producto {id} no encontrado.");
                return NotFound();
            }

            // 3. Mapeamos a DTO
            var destination = _mapper.Map<ProductResponseDto>(product);

            // 4. Guardamos en caché por 10 minutos con su llave única
            await _unitOfWork.Cache.SetAsync(cacheKey, destination, TimeSpan.FromMinutes(10));

            return Ok(destination);
        }

        /// <summary>
        /// Crea un nuevo producto y confirma los cambios mediante la Unit of Work.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> PostProduct(ProductCreateDto productDto)
        {
            // 1. Mapeamos el DTO de entrada a la entidad de dominio
            var product = _mapper.Map<Product>(productDto);

            // Añadimos el producto al repositorio (esto solo ocurre en memoria)
            await _unitOfWork.Repository<Product>().AddAsync(product);

            // La Unit of Work confirma la transacción en la base de datos
            var result = await _unitOfWork.SaveAsync();

            // 4. Verificamos que la inserción fue exitosa antes de tocar la caché
            if (result > 0)
            {
                _logger.LogInformation($"Producto con id: {result} y nombre  {product.Name}");

                // INVALIDACIÓN: Al haber un nuevo producto, la lista anterior en caché ya no es válida.
                // Borramos la llave para asegurar que el próximo GET traiga la lista actualizada.
                await _unitOfWork.Cache.RemoveAsync("products_list");
                _logger.LogInformation("Caché 'products_list' invalidada tras la creación de un nuevo producto.");

                // 5. Devolvemos el objeto creado mapeado al DTO de respuesta
                var response = _mapper.Map<ProductResponseDto>(product);

                // Es buena práctica usar CreatedAtAction para indicar la ruta del nuevo recurso
                return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, response);
            }


            // Si llegamos aquí, algo falló en la persistencia
            _logger.LogError("Error crítico: No se pudo persistir el nuevo producto en la base de datos.");
            return BadRequest("No se pudo guardar el producto en el sistema.");
            //return Ok(_mapper.Map<ProductResponseDto>(product));
        }

        /// <summary>
        /// Actualiza un producto existente, guarda los cambios en la base de datos 
        /// e invalida la caché de la lista de productos.
        /// </summary>
        /// <param name="id">Identificador del producto a actualizar.</param>
        /// <param name="productDto">DTO con los nuevos datos del producto.</param>
        /// <returns>NoContent si es exitoso, o BadRequest si hay errores.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductUpdateDto productDto)
        {
            // Verificación de consistencia entre el ID de la URL y el del cuerpo
            if (id != productDto.Id)
            {
                return BadRequest("El ID del producto no coincide con el de la solicitud.");
            }

            // Mapeamos el DTO a nuestra entidad de dominio
            var product = _mapper.Map<Product>(productDto);

            // Notificamos al repositorio que la entidad ha sido modificada
            _unitOfWork.Repository<Product>().Update(product);

            // Persistimos los cambios en Postgres
            var result = await _unitOfWork.SaveAsync();

            if (result > 0)
            {
                // INVALIDACIÓN DOBLE:
                // 1. Borramos la lista general
                await _unitOfWork.Cache.RemoveAsync("products_list");
                // 2. Borramos el producto específico para que no quede "huérfano" con datos viejos
                await _unitOfWork.Cache.RemoveAsync($"product:{id}");

                _logger.LogInformation($"Caché total invalidada para el producto {id}");

                return NoContent();
            }

            return BadRequest("No se pudo actualizar el producto en la base de datos.");
        }

        /// <summary>
        /// Elimina físicamente un producto de la base de datos e invalida la caché.
        /// </summary>
        /// <param name="id">Identificador del producto a eliminar.</param>
        /// <returns>NoContent si es exitoso, o NotFound si el producto no existe.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Buscamos el producto para asegurarnos de que existe antes de intentar borrarlo
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning($"Intento de eliminar producto inexistente: {id}");
                return NotFound();
            }

            // Marcamos la entidad para ser eliminada
            _unitOfWork.Repository<Product>().Delete(product);

            // Confirmamos la eliminación en la base de datos
            var result = await _unitOfWork.SaveAsync();

            if (result > 0)
            {
                // INVALIDACIÓN DOBLE:
                await _unitOfWork.Cache.RemoveAsync("products_list");
                await _unitOfWork.Cache.RemoveAsync($"product:{id}");

                _logger.LogInformation($"Caché eliminada tras borrar el producto {id}");

                return NoContent();
            }

            return BadRequest("Error al intentar eliminar el producto.");
        }
    }
}
