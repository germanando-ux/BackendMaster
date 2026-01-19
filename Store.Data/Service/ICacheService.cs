using System;
using System.Collections.Generic;
using System.Text;

namespace Store.Data.Service
{
    /// <summary>
    /// Interfaz para el manejo de caché distribuida (Redis).
    /// Proporciona métodos genéricos para almacenar, recuperar y eliminar datos.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Obtiene un objeto de la caché y lo deserializa al tipo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de dato a recuperar.</typeparam>
        /// <param name="key">Clave única del dato en Redis.</param>
        /// <returns>El objeto recuperado o null si no existe.</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Serializa un objeto y lo almacena en la caché con un tiempo de expiración.
        /// </summary>
        /// <typeparam name="T">Tipo de dato a almacenar.</typeparam>
        /// <param name="key">Clave única para el dato.</param>
        /// <param name="value">Objeto a almacenar.</param>
        /// <param name="expiration">Tiempo de vida del dato (opcional, por defecto 5 min).</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        /// <summary>
        /// Elimina una entrada específica de la caché por su clave.
        /// </summary>
        /// <param name="key">Clave del dato a eliminar.</param>
        Task RemoveAsync(string key);
    }
}
