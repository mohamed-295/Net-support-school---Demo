namespace NetSupport.Shared.Models;

public sealed class Exam
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "";
    public int DurationMinutes { get; set; } = 10;
    public List<Question> Questions { get; set; } = new();
}
