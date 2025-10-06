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
}
