using Microsoft.EntityFrameworkCore;
using Store.Data.Data;
using Store.Data.DTO;
using System.Linq.Expressions;

namespace Store.Data.Repositories
{
    public class  GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly StoreDbContext _context;

        public GenericRepository(StoreDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los registros de la entidad, permitiendo incluir relaciones dinámicamente.
        /// </summary>
        /// <param name="includes">Expresiones lambda para incluir tablas relacionadas (Eager Loading).</param>
        /// <returns>Una lista de entidades del tipo T.</returns>
        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            // Aquí aplicamos el "Eager Loading" dinámico para los Includes
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Obtiene un registro por ID con soporte para carga ansiosa de relaciones.
        /// </summary>
        /// <param name="id">ID de la entidad.</param>
        /// <param name="includes">Relaciones a incluir.</param>
        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            // Aplicamos cada Include dinámicamente
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            // Buscamos por ID. Usamos la propiedad "Id" por convención.
            // EF Core nos obliga a usar un método de ejecución como FirstOrDefaultAsync
            // ya que FindAsync no funciona sobre IQueryable con Includes.
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        /// <summary>
        /// Busca una entidad específica por su identificador único.
        /// </summary>
        /// <param name="id">Clave primaria de la entidad.</param>
        /// <returns>La entidad encontrada o null si no existe.</returns>
        public async Task<T?> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);

        /// <summary>
        /// Agrega una nueva entidad al contexto de datos.
        /// </summary>
        /// <param name="entity">Instancia de la entidad a agregar.</param>
        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

        /// <summary>
        /// Marca una entidad existente para ser actualizada en la base de datos.
        /// </summary>
        /// <param name="entity">Entidad con los cambios realizados.</param>
        public void Update(T entity) => _context.Set<T>().Update(entity);

        /// <summary>
        /// Elimina un registro del contexto de datos.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        public void Delete(T entity) => _context.Set<T>().Remove(entity);
    }
}
