using FreakyFashion.Contracts.Products;
using FreakyFashion.Data;
using FreakyFashion.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // för DbUpdateException
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;


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

    // GET /api/products[?page=1&pageSize=10][&slug=...]
    [HttpGet]
    public ActionResult<IEnumerable<ProductResponse>> GetProducts(
        [FromQuery] string? slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var bySlug = dbContext.Products
                .Where(p => p.UrlSlug == slug)
                .OrderBy(p => p.Id) // stabil ordning om du vill
                .Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.ImageUrl))
                .ToList();

            return Ok(bySlug); // lista med 0 eller 1
        }

        if (page < 1 || pageSize < 1) return BadRequest("page and pageSize must be >= 1");

        var items = dbContext.Products
            .OrderBy(p => p.Id)                 // stabil sortering FÖRE Skip/Take
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.ImageUrl))
            .ToList();

        return Ok(items); // paginerad lista
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
    [Authorize]
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

    // PATCH /api/products/{id}
    // Content-Type: application/json-patch+json
    // Få klarhet om det ska vara 200 OK eller 204 No Content, uppgiftens intruktioner är otydliga. 
    [Authorize]
    [HttpPatch("{id}")]
    public IActionResult Patch(int id, [FromBody] JsonPatchDocument<ProductPatch> patch)
    {
        if (patch is null) return BadRequest("Patch document is required.");

        // 1) Hämta entiteten
        var entity = dbContext.Products.Find(id);
        if (entity is null) return NotFound();

        // 2) Mappa entity -> DTO vi kan patcha
        var dto = new ProductPatch
        {
            name = entity.Name,
            description = entity.Description,
            price = entity.Price,
            image = entity.ImageUrl,
            urlSlug = entity.UrlSlug
        };

        // 3) Applicera patch mot DTO + fånga valideringsfel i ModelState
        patch.ApplyTo(dto, ModelState);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // 4) Enkla regler
        if (dto.price is < 0) return BadRequest("price cannot be negative");

        // 5) Mappa tillbaka DTO -> entity
        if (dto.name is not null) entity.Name = dto.name;
        if (dto.description is not null) entity.Description = dto.description;
        if (dto.price is not null) entity.Price = dto.price.Value;
        if (dto.image is not null) entity.ImageUrl = dto.image;

        if (dto.urlSlug is not null)
        {
            // Klienten har satt slug uttryckligen
            entity.UrlSlug = dto.urlSlug;
        }
        else if (dto.name is not null)
        {
            // Namn ändrades men ingen slug skickades -> generera om
            entity.UrlSlug = Slugify(entity.Name);
        }

        // 6) Spara
        dbContext.SaveChanges();

        // 7) Enligt krav: 204 No Content
        return NoContent();
    }


    // DELETE /api/products/1
    [Authorize]
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
