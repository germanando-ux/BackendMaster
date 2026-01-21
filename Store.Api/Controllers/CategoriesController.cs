using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Domain.Models;
using MassTransit; 
using Store.Domain.Contracts;

namespace Store.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly StoreDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint; //  RabbitMQ

        public CategoriesController(StoreDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            //throw new UnauthorizedAccessException("Acceso denegado de prueba");
            return await _context.Categories.ToListAsync();
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            // 1. Persistencia en Base de Datos (Síncrono)
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // 2. Notificación al Bus de Mensajes (Asíncrono)
            // Publicamos el evento para que cualquier servicio interesado se entere
            await _publishEndpoint.Publish(new CategoryCreated
            {
                Id = category.Id,
                Name = category.Name
            });

            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
                //throw new KeyNotFoundException();
            }

            _context.Categories.Remove(category);

            // Aquí es donde saltará la excepción si hay productos vinculados
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
