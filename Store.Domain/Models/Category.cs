using System.ComponentModel.DataAnnotations;

namespace Store.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Propiedad de navegación: Una categoría tiene muchos productos
        public List<Product> Products { get; set; } = [];
    }
}
