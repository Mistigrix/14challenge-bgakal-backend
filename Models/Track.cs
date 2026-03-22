public class Track
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Artist { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Cover { get; set; }
    public int Duration { get; set; } // en secondes
    public string FilePath { get; set; } = string.Empty;
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}
