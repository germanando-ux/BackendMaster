using System.ComponentModel.DataAnnotations;

namespace Store.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor que cero")]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }
        public string? Image { get; set; }

        // Clave ajena
        [Required]
        public int CategoryId { get; set; }
        // Propiedad de navegación: Un producto pertenece a una categoría
        public Category? Category { get; set; }
    }
}
