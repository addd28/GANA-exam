using Microsoft.EntityFrameworkCore;
using SmartShop.API.Entities;

namespace SmartShop.API.Data;

public class SmartShopDbContext : DbContext
{
    public SmartShopDbContext(DbContextOptions<SmartShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.ProductName).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Features).HasMaxLength(512);
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");

            // Seed initial 4 products
            entity.HasData(
                new Product
                {
                    ProductId = 1,
                    ProductName = "Galaxy S25",
                    Category = "Phone",
                    Price = 22990000m,
                    Features = "6.2 inch AMOLED, Snapdragon 8 Elite, 50MP camera",
                    Description = null
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "MacBook Air M4",
                    Category = "Laptop",
                    Price = 28490000m,
                    Features = "13.6 inch, 16GB RAM, 18 hours battery",
                    Description = null
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "AirPods Pro 3",
                    Category = "Accessory",
                    Price = 6790000m,
                    Features = "Active noise cancellation, USB-C, IP57",
                    Description = null
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Xiaomi Redmi Note 15",
                    Category = "Phone",
                    Price = 6490000m,
                    Features = "120Hz display, 5500mAh battery, 108MP camera",
                    Description = null
                }
            );
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Review");
            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.ReviewId).UseIdentityColumn();
            entity.Property(e => e.CustomerName).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Sentiment).HasMaxLength(16).IsRequired();
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.Summary).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(r => r.Product)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => r.ProductId);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessage");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.SessionId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(16).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
