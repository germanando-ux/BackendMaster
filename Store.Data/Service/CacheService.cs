using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Store.Data.Service
{
    /// <summary>
    /// Implementación del servicio de caché utilizando IDistributedCache (StackExchange.Redis).
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Recupera un valor de la caché de Redis.
        /// </summary>
        /// <typeparam name="T">El tipo al que se desea convertir el JSON almacenado.</typeparam>
        /// <param name="key">Identificador único de la llave en Redis.</param>
        /// <returns>El objeto deserializado o el valor por defecto (null) si no se encuentra.</returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            return data == null ? default : JsonSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// Guarda un objeto en Redis convirtiéndolo previamente a formato JSON.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a guardar.</typeparam>
        /// <param name="key">Identificador único de la llave.</param>
        /// <param name="value">El objeto que se desea persistir.</param>
        /// <param name="expiration">Tiempo de vida en caché. Si es null, se asignan 5 minutos por defecto.</param>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };
            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options);
        }

        /// <summary>
        /// Elimina una entrada de la caché de forma inmediata.
        /// Útil para invalidar datos cuando se detectan cambios en la base de datos.
        /// </summary>
        /// <param name="key">Llave que se desea borrar.</param>
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
