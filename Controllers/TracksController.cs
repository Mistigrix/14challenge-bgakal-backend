using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly MusicDbContext _db;
    private readonly string _uploadsPath;

    public TracksController(MusicDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _uploadsPath = Path.Combine(env.ContentRootPath, "uploads");
    }

    // GET /api/tracks?categoryId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId)
    {
        var query = _db.Tracks.Include(t => t.Category).AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);
        var tracks = await query.ToListAsync();
        var baseUrl = GetBaseUrl();
        return Ok(tracks.Select(t => MapToResponse(t, baseUrl)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var track = await _db.Tracks.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);
        if (track == null) return NotFound();
        return Ok(MapToResponse(track, GetBaseUrl()));
    }

    // GET /api/tracks/{id}/audio  — retourne le fichier audio directement
    [HttpGet("{id}/audio")]
    public async Task<IActionResult> GetAudio(int id)
    {
        var track = await _db.Tracks.FindAsync(id);
        if (track == null) return NotFound();

        var relative = track.FilePath.TrimStart('/');
        if (relative.StartsWith("uploads/")) relative = relative["uploads/".Length..];
        var fullPath = Path.Combine(_uploadsPath, relative);

        if (!System.IO.File.Exists(fullPath)) return NotFound("Fichier audio introuvable");

        var contentType = Path.GetExtension(fullPath).ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".flac" => "audio/flac",
            ".aac" => "audio/aac",
            ".m4a" => "audio/mp4",
            _ => "application/octet-stream"
        };

        // enableRangeProcessing permet la lecture partielle (seek dans le player)
        return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
    }

    // POST /api/tracks  (multipart/form-data)
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] TrackCreateRequest request)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return BadRequest("Catégorie introuvable");

        if (request.AudioFile == null || request.AudioFile.Length == 0)
            return BadRequest("Le fichier audio est requis");

        var audioPath = await SaveFile(request.AudioFile, "audio");
        string? coverPath = null;
        if (request.CoverFile != null && request.CoverFile.Length > 0)
            coverPath = await SaveFile(request.CoverFile, "covers");

        var track = new Track
        {
            CategoryId = request.CategoryId,
            Artist = request.Artist,
            Title = request.Title,
            Duration = request.Duration,
            FilePath = audioPath,
            Cover = coverPath
        };

        _db.Tracks.Add(track);
        await _db.SaveChangesAsync();
        await _db.Entry(track).Reference(t => t.Category).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = track.Id }, MapToResponse(track, GetBaseUrl()));
    }

    // PUT /api/tracks/{id}  (multipart/form-data, tous les champs optionnels)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] TrackUpdateRequest request)
    {
        var track = await _db.Tracks.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);
        if (track == null) return NotFound();

        if (request.CategoryId.HasValue)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value))
                return BadRequest("Catégorie introuvable");
            track.CategoryId = request.CategoryId.Value;
        }

        if (request.Artist != null) track.Artist = request.Artist;
        if (request.Title != null) track.Title = request.Title;
        if (request.Duration.HasValue) track.Duration = request.Duration.Value;

        if (request.AudioFile != null && request.AudioFile.Length > 0)
        {
            DeleteFile(track.FilePath);
            track.FilePath = await SaveFile(request.AudioFile, "audio");
        }

        if (request.CoverFile != null && request.CoverFile.Length > 0)
        {
            if (track.Cover != null) DeleteFile(track.Cover);
            track.Cover = await SaveFile(request.CoverFile, "covers");
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var track = await _db.Tracks.FindAsync(id);
        if (track == null) return NotFound();

        DeleteFile(track.FilePath);
        if (track.Cover != null) DeleteFile(track.Cover);

        _db.Tracks.Remove(track);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private string GetBaseUrl() => $"{Request.Scheme}://{Request.Host}";

    private async Task<string> SaveFile(IFormFile file, string subfolder)
    {
        var ext = Path.GetExtension(file.FileName);
        var filename = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_uploadsPath, subfolder, filename);
        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{subfolder}/{filename}";
    }

    private void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var relative = relativePath.TrimStart('/');
        if (relative.StartsWith("uploads/")) relative = relative["uploads/".Length..];
        var fullPath = Path.Combine(_uploadsPath, relative);
        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
    }

    private static TrackResponse MapToResponse(Track track, string baseUrl) => new()
    {
        Id = track.Id,
        CategoryId = track.CategoryId,
        CategoryName = track.Category?.Name ?? string.Empty,
        Artist = track.Artist,
        Title = track.Title,
        Duration = track.Duration,
        CoverUrl = track.Cover != null ? $"{baseUrl}{track.Cover}" : null
    };
}
