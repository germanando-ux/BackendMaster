using Microsoft.EntityFrameworkCore;
using Store.Domain.Models;
using MassTransit;

namespace Store.Data.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<User> Users => Set<User>(); 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Siempre llamar a la base primero
            base.OnModelCreating(modelBuilder);

            // 2. Configuración de Category
            modelBuilder.Entity<Category>(entity => {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // 3. Configuración de Product
            modelBuilder.Entity<Product>(entity => {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Price).HasPrecision(18, 2);

                // Definición de la relación 1:N
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            ///configuración de users
                modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 4. CONFIGURACIÓN DE MASSTRANSIT (Crucial que esté aquí al final)
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}