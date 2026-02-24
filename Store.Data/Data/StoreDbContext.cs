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
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentaDetalle => Set<VentaDetalle>();

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

            modelBuilder.Entity<Venta>(entity =>
            {
                // Seguimos tu criterio de precisión 18,3
                entity.Property(v => v.Total)
                      .HasPrecision(18, 3);

                // Configuración de la relación uno a muchos con VentaDetalle
                entity.HasMany(v => v.Detalles)
                      .WithOne(d => d.Venta)
                      .HasForeignKey(d => d.VentaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(v => v.EmailCliente)
                       .HasMaxLength(150);
            });

            modelBuilder.Entity<VentaDetalle>()
            .Property(vd => vd.PrecioUnitario)
            .HasPrecision(18, 3);

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