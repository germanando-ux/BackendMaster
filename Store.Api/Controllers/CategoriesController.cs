using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Domain.Models;

namespace Store.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly StoreDbContext _context;

        public CategoriesController(StoreDbContext context)
        {
            _context = context;
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
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

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
