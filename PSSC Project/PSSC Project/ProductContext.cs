using Microsoft.EntityFrameworkCore;
using PSSC_Project.Models;

namespace PSSC_Project
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }


        public DbSet<User> Users { get; set; }
    }

}
