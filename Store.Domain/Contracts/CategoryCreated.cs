using System;
using System.Collections.Generic;
using System.Text;

namespace Store.Domain.Contracts
{
    // Contrato que representa el evento de creación de una nueva categoría.
    /// Se utiliza para notificar a otros servicios de forma asíncrona.
    /// </summary>
    public record CategoryCreated
    {
        /// <summary>
        /// Identificador único de la categoría creada.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Nombre de la categoría.
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}
