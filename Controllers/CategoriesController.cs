using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly MusicDbContext _db;

    public CategoriesController(MusicDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .Include(c => c.Tracks)
            .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name, TrackCount = c.Tracks.Count })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Tracks)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        return Ok(new CategoryResponse { Id = category.Id, Name = category.Name, TrackCount = category.Tracks.Count });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var category = new Category { Name = request.Name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            new CategoryResponse { Id = category.Id, Name = category.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        category.Name = request.Name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
