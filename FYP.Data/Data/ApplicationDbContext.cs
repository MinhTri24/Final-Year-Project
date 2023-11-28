using FYP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FYP.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryRequest> CategoryRequests { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Crops" },
                new Category { Id = 2, Name = "Livestock" },
                new Category { Id = 3, Name = "Aquaculture" },
                new Category { Id = 4, Name = "Cereal Grains" },
                new Category { Id = 5, Name = "Oilseeds" },
                new Category { Id = 6, Name = "Miscellaneous Agricultural Products" },
                new Category { Id = 7, Name = "Other" }
            );
        }
    }
}