using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Domain.Models;
using MassTransit; 
using Store.Domain.Contracts;
using AutoMapper;

namespace Store.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly StoreDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint; //  RabbitMQ
        private readonly ILogger<CategoriesController> _logger;


        public CategoriesController(StoreDbContext context, IPublishEndpoint publishEndpoint, ILogger<CategoriesController> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
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
            // 1. Iniciamos la transacción explícita en Postgres
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                // 4. Confirmamos la transacción (Se guarda categoría y mensaje a la vez)
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
            }
            
            catch (Exception ex)
            {
                // Si algo falla (DB, mapeo, etc.), no se guarda nada ni en Categories ni en Outbox
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear categoría con Outbox");
                return StatusCode(500, "Error interno al procesar la solicitud");
            }
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
