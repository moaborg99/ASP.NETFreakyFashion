using FreakyFashion.Contracts.Categories;
using FreakyFashion.Contracts.Products;
using FreakyFashion.Data;
using FreakyFashion.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FreakyFashion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext db;

    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim().ToLowerInvariant();
        s = s.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", "-");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\-]", "");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"-+", "-").Trim('-');
        return s;
    }


    public CategoriesController(AppDbContext db) => this.db = db;

    // GET /api/categories
    // GET /api/categories?slug=...
    [HttpGet]
    public ActionResult<IEnumerable<CategoryResponse>> GetCategories([FromQuery] string? slug)
    {
        // inkludera products via many-to-many
        var query = db.Categories
            .Include(c => c.Products)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(slug))
        {
            query = query.Where(c => c.UrlSlug == slug);
        }

        var result = query
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.ImageUrl,
                c.Products
                    .Select(p => new ProductResponse(
                        p.Id, p.Name, p.Description, p.Price, p.ImageUrl
                    ))
            ))
            .ToList();

        return Ok(result); // alltid lista (kan vara tom)
    }

    // GET /api/categories/{id}
    [HttpGet("{id}")]
    public ActionResult<CategoryResponse> GetCategory(int id)
    {
        var dto = db.Categories
            .Include(c => c.Products)
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.ImageUrl,
                c.Products.Select(p => new ProductResponse(
                    p.Id, p.Name, p.Description, p.Price, p.ImageUrl
                ))
            ))
            .FirstOrDefault();

        if (dto is null) return NotFound();
        return Ok(dto);
    }

    // POST /api/categories
    [Authorize]
    [HttpPost]
    public ActionResult<CategoryResponse> Create([FromBody] CreateCategoryRequest dto)
    {
        // Enkel validering (håll den minimal nu)
        if (string.IsNullOrWhiteSpace(dto.name))
            return BadRequest("name is required");

        // Request -> domän
        var category = new Category
        {
            Name = dto.name.Trim(),
            ImageUrl = dto.image?.Trim() ?? string.Empty,
            UrlSlug = Slugify(dto.name)
        };

        db.Categories.Add(category);
        db.SaveChanges();

        // Domän -> response (produkter är tom lista vid skapande)
        var response = new CategoryResponse(
            category.Id,
            category.Name,
            category.ImageUrl,
            Enumerable.Empty<ProductResponse>()
        );

        // 201 Created + Location: /api/categories/{id}
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
    }

    // PUT /api/categories/{categoryId}/products/{productId}
    // => 204 No Content
    [Authorize]
    [HttpPut("{categoryId}/products/{productId}")]
    public IActionResult AddProductToCategory(int categoryId, int productId)
    {
        // 1) Hämta kategori + dess produkter
        var category = db.Categories
            .Include(c => c.Products)
            .FirstOrDefault(c => c.Id == categoryId);
        if (category is null) return NotFound();

        // 2) Hämta produkt
        var product = db.Products.Find(productId);
        if (product is null) return NotFound();

        // 3) Lägg till koppling om den inte redan finns (idempotent)
        if (!category.Products.Any(p => p.Id == productId))
        {
            category.Products.Add(product);
            db.SaveChanges();
        }

        return NoContent();
    }

    // DELETE /api/categories/{categoryId}/products/{productId}
    // => 204 No Content
    [Authorize]
    [HttpDelete("{categoryId}/products/{productId}")]
    public IActionResult RemoveProductFromCategory(int categoryId, int productId)
    {
        // 1) Hämta kategori + dess produkter
        var category = db.Categories
            .Include(c => c.Products)
            .FirstOrDefault(c => c.Id == categoryId);
        if (category is null) return NotFound();

        // 2) Finns kopplingen? Ta bort idempotent
        var existing = category.Products.FirstOrDefault(p => p.Id == productId);
        if (existing != null)
        {
            category.Products.Remove(existing);
            db.SaveChanges();
        }

        return NoContent();
    }

    // DELETE /api/categories/{id}  => 204 No Content eller 404
    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        var category = db.Categories.Find(id);
        if (category is null) return NotFound();

        db.Categories.Remove(category);
        db.SaveChanges(); // ON DELETE CASCADE tar bort länkar i CategoryProducts

        return NoContent();
    }

    // PATCH /api/categories/{id}
    // Content-Type: application/json-patch+json
    [Authorize]
    [HttpPatch("{id}")]
    public IActionResult PatchCategory(int id, [FromBody] JsonPatchDocument<CategoryPatch> patch)
    {
        if (patch is null) return BadRequest("Patch document is required.");

        var entity = db.Categories.Find(id);
        if (entity is null) return NotFound();

        // DTO att patcha mot
        var dto = new CategoryPatch
        {
            name = entity.Name,
            image = entity.ImageUrl,
            urlSlug = entity.UrlSlug
        };

        patch.ApplyTo(dto, ModelState);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // Mappar tillbaka
        if (dto.name is not null) entity.Name = dto.name;
        if (dto.image is not null) entity.ImageUrl = dto.image;

        if (dto.urlSlug is not null)
        {
            entity.UrlSlug = dto.urlSlug;
        }
        else if (dto.name is not null)
        {
            // Om namnet ändrats men slug ej skickats → generera om
            entity.UrlSlug = Slugify(entity.Name);
        }

        db.SaveChanges();
        return NoContent(); // 204 (du har lärar-ok på 204 eller 200)
    }



}
