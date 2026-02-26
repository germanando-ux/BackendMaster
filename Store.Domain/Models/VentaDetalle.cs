using System;
using System.Collections.Generic;
using System.Text;

namespace Store.Domain.Models
{
    /// <summary>
    /// Representa el desglose de un producto específico dentro de una venta.
    /// </summary>
    public class VentaDetalle
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        //relacion con la cabecera
        public Venta Venta { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
