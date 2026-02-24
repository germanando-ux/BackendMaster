using System;
using System.Collections.Generic;
using System.Text;

namespace Store.Data.DTO
{
    public class VentaReadDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string? EmailCliente { get; set; }
        public decimal Total { get; set; }
        public List<VentaDetalleReadDto> Detalles { get; set; } = new();
    }

    public class VentaDetalleReadDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
