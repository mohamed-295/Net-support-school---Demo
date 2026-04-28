namespace NetSupport.Shared.Models;

public sealed class Choice
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}
