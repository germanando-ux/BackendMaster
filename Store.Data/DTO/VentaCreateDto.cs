namespace Store.Data.DTO
{
    public class VentaCreateDto
    {

        /// <summary>
        /// Correo del cliente que actúa como identificador temporal.
        /// </summary>
        public string EmailCliente { get; set; } = string.Empty;

        /// <summary>
        /// Lista de productos seleccionados en el carrito.
        /// </summary>
        public List<VentaDetalleDto> Detalles { get; set; } = new();

    }

    public class VentaDetalleDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        // El precio lo recuperaremos en el Backend por seguridad 🛡️
    }
}
