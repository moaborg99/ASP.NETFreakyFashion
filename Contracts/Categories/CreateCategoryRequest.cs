namespace FreakyFashion.Contracts.Categories;

public record CreateCategoryRequest(
    string name,
    string image
// UrlSlug genereras från name när du sparar
);
