using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreakyFashion.Domain;

[Table("Categories")] // matchar dbo.Categories i SQL
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string UrlSlug { get; set; } = string.Empty;

    // Navigation (för many-to-many). EF fyller dessa via join-tabellen.
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
