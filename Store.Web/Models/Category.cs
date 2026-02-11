using System.ComponentModel.DataAnnotations;

namespace Store.Web.Models
{
    public class Category
    {
        // Usamos int? para permitir que sea nulo al crear una nueva categoría
        public int? Id { get; set; }

        // Validaciones que Blazor usará en el formulario modal
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre es demasiado largo")]
        public string Name { get; set; } = string.Empty;

        // Campo para la URL de la imagen (opcional)
        public string? Image { get; set; }
    }
}
