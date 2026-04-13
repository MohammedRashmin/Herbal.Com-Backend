using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Web.Com.Entities;
using Web.Com.Entities.Identity;

namespace Web.Com.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure decimal precision
        builder.Entity<Product>()
            .Property(p => p.AverageRating)
            .HasColumnType("decimal(3,2)");
    }
}
