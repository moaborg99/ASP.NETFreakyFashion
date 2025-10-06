using FreakyFashion.Contracts.Categories;
using FreakyFashion.Contracts.Products;
using FreakyFashion.Data;
using FreakyFashion.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

}
