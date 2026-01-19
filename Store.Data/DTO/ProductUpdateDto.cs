using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Store.Data.DTO
{
    /// <summary>
    /// Objeto de transferencia de datos para la actualización de un producto existente.
    /// Incluye validaciones necesarias para asegurar la integridad en la persistencia.
    /// </summary>
    public class ProductUpdateDto
    {
        /// <summary>
        /// Identificador único del producto que se desea modificar.
        /// Es obligatorio para asegurar que estamos editando el registro correcto.
        /// </summary>
        [Required(ErrorMessage = "El Id es obligatorio para actualizar.")]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del producto.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del producto.
        /// </summary>
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Precio unitario del producto. Debe ser mayor a 0.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Price { get; set; }

        /// <summary>
        /// URL de la imagen del producto.
        /// </summary>
        public string? PictureUrl { get; set; }

        /// <summary>
        /// Identificador de la categoría a la que pertenece el producto.
        /// </summary>
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int CategoryId { get; set; }
    }
}
