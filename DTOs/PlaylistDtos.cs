public class PlaylistRequest
{
    public string Name { get; set; } = string.Empty;
}

public class PlaylistResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalDuration { get; set; }
    public List<TrackResponse> Tracks { get; set; } = new();
}
