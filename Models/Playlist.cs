public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}
