using System.ComponentModel.DataAnnotations;

namespace FreakyFashion.Contracts.Products;

public record CreateProductRequest(
    [Required] string name,
    [Required] string description,
    [Range(0, double.MaxValue)] decimal price,
    [Required] string image
);
