namespace FreakyFashion.Contracts.Categories;

public record class CategoryPatch
{
    public string? name { get; set; }
    public string? image { get; set; }
    public string? urlSlug { get; set; }
}
