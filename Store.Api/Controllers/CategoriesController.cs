using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Data.Repositories;
using Store.Domain.Contracts;
using Store.Domain.Models;

namespace Store.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly StoreDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint; //  RabbitMQ
        private readonly ILogger<CategoriesController> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;



        public CategoriesController(StoreDbContext context, IPublishEndpoint publishEndpoint, IMapper mapper, ILogger<CategoriesController> logger, IUnitOfWork unitOfWork)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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
                // 2. Notificación al Bus de Mensajes (Asíncrono)
                // Publicamos el evento para que cualquier servicio interesado se entere
                await _publishEndpoint.Publish(new CategoryCreated
                {
                    Id = category.Id,
                    Name = category.Name
                });

                // 1. Persistencia en Base de Datos (Síncrono)
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();



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

    

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            // Verificación de consistencia entre el ID de la URL y el del cuerpo
            if (id != category.Id)
            {
                return BadRequest("El ID de la categoría  no coincide con el de la solicitud.");
            }


            // Notificamos al repositorio que la entidad ha sido modificada
            _unitOfWork.Repository<Category>().Update(category);

            // Persistimos los cambios en Postgres
            var result = await _unitOfWork.SaveAsync();

            if (result > 0)
            {
                // INVALIDACIÓN DOBLE:
                // 1. Borramos la lista general
                await _unitOfWork.Cache.RemoveAsync("category_list");
                // 2. Borramos la categoría específica para que no quede "huérfana" con datos viejos
                await _unitOfWork.Cache.RemoveAsync($"category:{id}");

                _logger.LogInformation($"Caché total invalidada para la categoría {id}");

                return NoContent();
            }

            return BadRequest("No se pudo actualizar la categoría en la base de datos.");
        }
    }

}
