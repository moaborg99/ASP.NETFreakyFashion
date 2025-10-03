namespace FreakyFashion.Contracts.Products;

public record ProductResponse(
    int id,
    string name,
    string description,
    decimal price,
    string image
);
