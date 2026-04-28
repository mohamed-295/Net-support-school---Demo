namespace NetSupport.Shared.Models;

public sealed class TestSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public Exam Exam { get; set; } = new();
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public int DurationMinutes { get; set; }
    public List<string> StudentIds { get; set; } = new();
    public string Status { get; set; } = "Created";
}
