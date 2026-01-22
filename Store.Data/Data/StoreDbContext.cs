using Microsoft.EntityFrameworkCore;
using Store.Domain.Models;
using MassTransit;

namespace Store.Data.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

        // Esta propiedad se convertirá en la tabla "Products" en PostgreSQL
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Category
            modelBuilder.Entity<Category>(entity => {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
                // Índice para búsquedas rápidas por nombre de categoría
                entity.HasIndex(c => c.Name).IsUnique();
            });
            // Configuración de Product y su Relación
            modelBuilder.Entity<Product>(entity => {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Price).HasPrecision(18, 2);

                // Configuramos la precisión del decimal para evitar avisos
                modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            base.OnModelCreating(modelBuilder);

                // Definición explícita de la relación 1:N
                entity.HasOne(p => p.Category)      // El producto tiene una categoría
                      .WithMany(c => c.Products)    // La categoría tiene muchos productos
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // Evitamos borrados accidentales en cascada
            });

            base.OnModelCreating(modelBuilder);

            // Esto añade las tablas necesarias para el patrón Outbox
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
