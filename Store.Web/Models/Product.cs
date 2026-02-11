using System.ComponentModel.DataAnnotations;

namespace Store.Web.Models
{
    public class Product
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es necesaria")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        // CAMPO NUEVO: Stock
        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, 10000, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        public string? Image { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una categoría")]
        [Range(1, int.MaxValue, ErrorMessage = "Categoría no válida")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
