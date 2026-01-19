using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Store.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Obtener todos con posibilidad de incluir tablas relacionadas (Eager Loading)
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

        // Obtener uno solo por su ID
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene una entidad por su ID, permitiendo incluir propiedades relacionadas.
        /// </summary>
        /// <param name="id">Identificador único.</param>
        /// <param name="includes">Expresiones para incluir entidades relacionadas (Eager Loading).</param>
        /// <returns>La entidad encontrada o null.</returns>
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

        // Métodos de modificación (Solo en memoria, no guardan en DB aún)
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
