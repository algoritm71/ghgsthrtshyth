using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ⚠️ Явно указываем тип timestamp without time zone
            modelBuilder.Entity<Category>()
                .Property(c => c.CreatedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Product>()
                .Property(p => p.CreatedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Product>()
                .Property(p => p.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("timestamp without time zone");
        }
    }
}