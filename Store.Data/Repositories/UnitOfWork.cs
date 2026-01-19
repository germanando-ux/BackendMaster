using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Data.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Store.Data.Repositories
{
    /// <summary>
    /// Implementación de la Unidad de Trabajo que gestiona el DbContext, 
    /// los repositorios dinámicos y el acceso a la caché.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreDbContext _context;
        private Hashtable? _repositories;
        /// Servicio de caché inyectado para gestionar datos temporales en Redis.v
        public ICacheService Cache { get; }


        public UnitOfWork(StoreDbContext context, ICacheService cache)
        {
            _context = context;
            Cache = cache;
            _repositories = new Hashtable();
        }


        /// <summary>
        /// Implementación de la conexión compartida. 
        /// EF Core gestiona el ciclo de vida (abrir/cerrar), Dapper solo la usa.
        /// </summary>
        public IDbConnection Connection => _context.Database.GetDbConnection();

        /// <summary>
        /// Crea o recupera una instancia de repositorio para la entidad solicitada.
        /// </summary>
        /// <typeparam name="T">Entidad de dominio.</typeparam>
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
                _repositories.Add(type, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[type]!;
        }
        /// <summary>
        /// Guarda los cambios en la base de datos de forma asíncrona.
        /// </summary>
        /// <returns>Número de registros afectados.</returns>
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Libera los recursos del contexto de base de datos.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
