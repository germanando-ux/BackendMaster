using System.ComponentModel.DataAnnotations;

namespace Store.Web.Models
{
    public class VentaCreateDto
    {

        /// <summary>
        /// Correo del cliente que actúa como identificador temporal.
        /// </summary>
        [Required (ErrorMessage = "El correo electrónico del cliente es obligatorio")]
        public string EmailCliente { get; set; } = string.Empty;

        /// <summary>
        /// Lista de productos seleccionados en el carrito.
        /// </summary>
          public List<VentaDetalleDto> Detalles { get; set; } = new();
        
    }

    public class VentaDetalleDto
    {
        [Required(ErrorMessage = "El id de producto es obligatorio")]
        public int ProductId { get; set; }
        [Range(0, 1000, ErrorMessage = "El stock no puede ser negativo")]
        public int Cantidad { get; set; }
        // El precio lo recuperaremos en el Backend por seguridad 🛡️
    }
}
