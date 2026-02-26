using System.ComponentModel.DataAnnotations;

namespace Store.Data.DTO
{
    public class VentaCreateDto
    {

        /// <summary>
        /// Correo del cliente que actúa como identificador temporal.
        /// </summary>

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string EmailCliente { get; set; } = string.Empty;

        /// <summary>
        /// Lista de productos seleccionados en el carrito.
        /// </summary>    

        [Required]
        [MinLength(1, ErrorMessage = "La venta debe tener al menos un detalle")]
        public List<VentaDetalleDto> Detalles { get; set; } = new();

    }

    public class VentaDetalleDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser válido")]
        public int ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000")]
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        public int Cantidad { get; set; }
        // El precio lo recuperaremos en el Backend por seguridad 🛡️
    }
}
