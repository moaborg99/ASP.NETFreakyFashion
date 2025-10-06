namespace FreakyFashion.Contracts.Categories;

public record CategoryResponse(
    int id,
    string name,
    string image,
    // produkter inuti kategorin
    IEnumerable<FreakyFashion.Contracts.Products.ProductResponse> products
);
