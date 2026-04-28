namespace NetSupport.Shared.Models;

public sealed class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Text { get; set; } = "";
    public List<Choice> Choices { get; set; } = new();
}
