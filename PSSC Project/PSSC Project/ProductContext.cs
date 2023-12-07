﻿using Microsoft.EntityFrameworkCore;
using PSSC_Project.Models;

namespace PSSC_Project
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }

        public DbSet<UserDto> Users { get; set; }
        public DbSet<ProductDto> Products { get; set; }
        public DbSet<OrderDto> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDto>().ToTable("Users").HasKey(u => u.UserId);
            modelBuilder.Entity<ProductDto>().ToTable("Products").HasKey(p => p.ProductId);
            modelBuilder.Entity<OrderDto>().ToTable("Orders").HasKey(o => o.OrderId);
            modelBuilder.Entity<OrderDetails>().ToTable("OrderDetails").HasKey(o => o.OrderDetailsId);
        }
    }
}
