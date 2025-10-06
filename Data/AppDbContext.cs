using FreakyFashion.Domain;
using Microsoft.EntityFrameworkCore;

namespace FreakyFashion.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-many: Category <-> Product via befintlig tabell dbo.CategoryProducts
        modelBuilder.Entity<Product>()
            .HasMany(p => p.Categories)
            .WithMany(c => c.Products)
            .UsingEntity<Dictionary<string, object>>(
                "CategoryProducts", // exakt tabellnamn i SQL
                j => j
                    .HasOne<Category>()
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .HasConstraintName("FK_CategoryProducts_Categories")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Product>()
                    .WithMany()
                    .HasForeignKey("ProductId")
                    .HasConstraintName("FK_CategoryProducts_Products")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("CategoryProducts");
                    j.HasKey("CategoryId", "ProductId");
                });
    }
}
