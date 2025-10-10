using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // för [Precision]

namespace FreakyFashion.Entities;

[Table("Products")] // tabellnamnet i SQL
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Precision(10, 2)] // matchar DECIMAL(10,2) i SQL
    public decimal Price { get; set; } = decimal.Zero;

    [Required, MaxLength(255)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string UrlSlug { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; set; } = new List<Category>();

}
