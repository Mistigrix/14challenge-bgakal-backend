using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly MusicDbContext _db;

    public PlaylistsController(MusicDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var playlists = await _db.Playlists
            .Include(p => p.PlaylistTracks).ThenInclude(pt => pt.Track).ThenInclude(t => t.Category)
            .ToListAsync();
        var baseUrl = GetBaseUrl();
        return Ok(playlists.Select(p => MapToResponse(p, baseUrl)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var playlist = await _db.Playlists
            .Include(p => p.PlaylistTracks).ThenInclude(pt => pt.Track).ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (playlist == null) return NotFound();
        return Ok(MapToResponse(playlist, GetBaseUrl()));
    }

    [HttpPost]
    public async Task<IActionResult> Create(PlaylistRequest request)
    {
        var playlist = new Playlist { Name = request.Name };
        _db.Playlists.Add(playlist);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = playlist.Id }, MapToResponse(playlist, GetBaseUrl()));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PlaylistRequest request)
    {
        var playlist = await _db.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        playlist.Name = request.Name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var playlist = await _db.Playlists.FindAsync(id);
        if (playlist == null) return NotFound();
        _db.Playlists.Remove(playlist);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/playlists/{id}/tracks/{trackId}
    [HttpPost("{id}/tracks/{trackId}")]
    public async Task<IActionResult> AddTrack(int id, int trackId)
    {
        if (!await _db.Playlists.AnyAsync(p => p.Id == id))
            return NotFound("Playlist introuvable");
        if (!await _db.Tracks.AnyAsync(t => t.Id == trackId))
            return NotFound("Morceau introuvable");
        if (await _db.PlaylistTracks.AnyAsync(pt => pt.PlaylistId == id && pt.TrackId == trackId))
            return Conflict("Ce morceau est déjà dans la playlist");

        _db.PlaylistTracks.Add(new PlaylistTrack { PlaylistId = id, TrackId = trackId });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/playlists/{id}/tracks/{trackId}
    [HttpDelete("{id}/tracks/{trackId}")]
    public async Task<IActionResult> RemoveTrack(int id, int trackId)
    {
        var pt = await _db.PlaylistTracks.FindAsync(id, trackId);
        if (pt == null) return NotFound();
        _db.PlaylistTracks.Remove(pt);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private string GetBaseUrl() => $"{Request.Scheme}://{Request.Host}";

    private static PlaylistResponse MapToResponse(Playlist playlist, string baseUrl) => new()
    {
        Id = playlist.Id,
        Name = playlist.Name,
        TotalDuration = playlist.PlaylistTracks.Sum(pt => pt.Track?.Duration ?? 0),
        Tracks = playlist.PlaylistTracks.Select(pt => new TrackResponse
        {
            Id = pt.Track.Id,
            CategoryId = pt.Track.CategoryId,
            CategoryName = pt.Track.Category?.Name ?? string.Empty,
            Artist = pt.Track.Artist,
            Title = pt.Track.Title,
            Duration = pt.Track.Duration,
            CoverUrl = pt.Track.Cover != null ? $"{baseUrl}{pt.Track.Cover}" : null
        }).ToList()
    };
}
