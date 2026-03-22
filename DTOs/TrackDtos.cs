public class TrackCreateRequest
{
    public int CategoryId { get; set; }
    public string Artist { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Duration { get; set; }
    public IFormFile AudioFile { get; set; } = null!;
    public IFormFile? CoverFile { get; set; }
}

public class TrackUpdateRequest
{
    public int? CategoryId { get; set; }
    public string? Artist { get; set; }
    public string? Title { get; set; }
    public int? Duration { get; set; }
    public IFormFile? AudioFile { get; set; }
    public IFormFile? CoverFile { get; set; }
}

public class TrackResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string? CoverUrl { get; set; }
}
