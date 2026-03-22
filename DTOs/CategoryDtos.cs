public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TrackCount { get; set; }
}
