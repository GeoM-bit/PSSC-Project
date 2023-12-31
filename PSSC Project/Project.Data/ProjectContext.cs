using Microsoft.EntityFrameworkCore;
using Project.Data.Models;

namespace Project
{
    public class ProjectContext : DbContext
    {
        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users").HasKey(u => u.UserId);
            modelBuilder.Entity<Product>().ToTable("Products").HasKey(p => p.ProductId);
            modelBuilder.Entity<Order>().ToTable("Orders").HasKey(o => o.OrderId);
            modelBuilder.Entity<OrderDetails>().ToTable("OrderDetails").HasKey(o => o.OrderDetailId);
        }
    }
}
