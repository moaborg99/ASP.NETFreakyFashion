namespace FreakyFashion.Contracts.Products;

// Alla fält nullable = valfria i en patch
public class ProductPatch
{
    public string? name { get; set; }
    public string? description { get; set; }
    public decimal? price { get; set; }
    public string? image { get; set; }    // mappar till Product.ImageUrl
    public string? urlSlug { get; set; }  // tillåter att klienten sätter slug explicit (valfritt)
}
