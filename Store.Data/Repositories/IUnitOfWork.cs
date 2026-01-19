using Store.Data.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Store.Data.Repositories
{
    /// <summary>
    /// Define el contrato para la Unidad de Trabajo, coordinando repositorios 
    /// y el manejo de caché en una sola transacción o contexto.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        
        
        //Acceso al servicio de caché genérico para operaciones de Redis.
        
        ICacheService Cache { get; }

        /// <summary>
        /// Obtiene un repositorio genérico para una entidad específica.
        /// </summary>
        /// <typeparam name="T">El tipo de la entidad de dominio.</typeparam>
        /// <returns>Una instancia de IGenericRepository para la entidad.</returns>
        IGenericRepository<T> Repository<T>() where T: class;


        /// <summary>
        /// Expone la conexión subyacente de la base de datos para operaciones de alto rendimiento con Dapper.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Confirma todos los cambios realizados en el contexto de la base de datos.
        /// </summary>
        /// <returns>El número de filas afectadas en la base de datos.</returns>
        Task<int> SaveAsync();
    }
}
