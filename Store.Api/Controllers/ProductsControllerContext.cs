using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Data.DTO;
using Store.Domain.Models;

namespace Store.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsControllerContext : Controller
    {
        private readonly StoreDbContext _context;
        private readonly IMapper _mapper; // Variable para AutoMapper

        public ProductsControllerContext(StoreDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            // Usamos .Include para traer también los datos de la categoría (Eager Loading)
            // return await _context.Products.Include(p => p.Category).ToListAsync();
                var products = await _context.Products
             .Include(p => p.Category)
             .ToListAsync();
            //CON AUTOMAPPER 

            // Mapeamos la lista de entidades a una lista de DTOs
            var response = _mapper.Map<IEnumerable<ProductResponseDto>>(products);
            return Ok(response);

            //SIN AUTOMAPPER
            //var products = await _context.Products.Include(p => p.Category).Select(p => new ProductResponseDto // <-- Transformamos aquí
            //{
            //Id = p.Id,
            //Name = p.Name,
            //Description = p.Description,
            //Price = p.Price,
            //Stock = p.Stock,
            //CategoryId = p.CategoryId,
            //CategoryName = p.Category.Name // <-- Extraemos solo el nombre
            //})
            //.ToListAsync();

            //return Ok(products);
            }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            // 1. Validar si la categoría existe
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);

            if (!categoryExists)
            {
                // Gracias a nuestro Handler, esto devolverá un 400 Bad Request profesional
                throw new ArgumentException($"La categoría con ID {product.CategoryId} no existe.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                throw new ArgumentException("El ID del producto no coincide con el de la URL.");
            }

            // 1. Validar si la categoría existe antes de intentar actualizar
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
            if (!categoryExists)
            {
                throw new ArgumentException($"La categoría con ID {product.CategoryId} no existe.");
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound(new { message = $"El producto con ID {id} no existe." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204: Actualización exitosa sin contenido de vuelta
        }

        // Método auxiliar para el check de existencia
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
