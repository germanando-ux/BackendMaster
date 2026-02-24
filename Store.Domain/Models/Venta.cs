using System;
using System.Collections.Generic;
using System.Text;

namespace Store.Domain.Models
{
    /// <summary>
    /// Representa la cabecera de una transacción de venta en el sistema.
    /// </summary>
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string EmailCliente { get; set; } = string.Empty;

        /// <summary>
        /// Colección de productos y cantidades asociados a esta venta.
        /// </summary>

        public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();

    }
}
