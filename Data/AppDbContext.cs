using FreakyFashion.Domain;
using Microsoft.EntityFrameworkCore;

namespace FreakyFashion.Data;

//Den här klassen representerar en session mot databasen. Kan ses som en bro mot databasen och via denna kan vi hämta, uppdatera, skapa och radera i databasen
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet representerar olika tabeller i databasen. Den kommer mappas till tabellen som har samma namn. 
    public DbSet<Product> Products => Set<Product>();
}
