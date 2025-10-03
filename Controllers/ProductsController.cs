using FreakyFashion.Contracts.Products;
using FreakyFashion.Data;
using FreakyFashion.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // för DbUpdateException

namespace FreakyFashion.Controllers;

//api/Products
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public ProductsController(AppDbContext dbContext)
    { this.dbContext = dbContext; }

    //Slugify
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

    // GET /api/products
    // GET /api/products?slug=svart-t-shirt  => returnera LISTA (0 eller 1), alltid 200 OK
    [HttpGet]
    public ActionResult<IEnumerable<ProductResponse>> GetProducts([FromQuery] string? slug)
    {
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var filtered = dbContext.Products
                .Where(p => p.UrlSlug == slug)
                .Select(p => new ProductResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl
                ))
                .ToList();

            return Ok(filtered); // 200 + [] om ingen träff
        }

        var all = dbContext.Products
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageUrl
            ))
            .ToList();

        return Ok(all);
    }

    // GET /api/products/{id}
    [HttpGet("{id}")]
    public ActionResult<ProductResponse> GetProduct(int id)
    {
        var dto = dbContext.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageUrl
            ))
            .FirstOrDefault();

        if (dto == null) return NotFound();
        return Ok(dto);
    }

    // POST /api/products
    [HttpPost]
    public ActionResult<ProductResponse> Create([FromBody] CreateProductRequest dto)
    {
        // (valfritt men smart) enkel validering
        if (string.IsNullOrWhiteSpace(dto.name)) return BadRequest("name is required");
        if (dto.price < 0) return BadRequest("price cannot be negative");

        // request -> domän
        var product = new Product
        {
            Name = dto.name.Trim(),
            Description = dto.description?.Trim() ?? string.Empty,
            Price = dto.price,
            ImageUrl = dto.image?.Trim() ?? string.Empty,
            UrlSlug = Slugify(dto.name)
        };

        dbContext.Products.Add(product);
        dbContext.SaveChanges(); // EF INSERT + sätter product.Id

        // domän -> response (positional record -> konstruktorn)
        var response = new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl
        );

        return CreatedAtAction(
            nameof(GetProduct),        // pekar på GET /api/products/{id}
            new { id = product.Id },   // route values
            response                   // body
        );

    }

    // DELETE /api/products/1
    [HttpDelete("{id}")]

    public IActionResult Delete(int id)
    {
        var product = dbContext.Products.Find(id);

        if (product == null)
        { 
            return NotFound(); 
        }

        dbContext.Products.Remove(product);

        dbContext.SaveChanges();    

        return NoContent();
            
    }
}
