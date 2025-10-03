namespace FreakyFashion.Contracts.Products;

public class ProductDto
{
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string image { get; set; } = string.Empty;
}
