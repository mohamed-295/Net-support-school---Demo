namespace NetSupport.Shared.Models;

public sealed class StudentAnswer
{
    public string StudentId { get; set; } = "";
    public string SessionId { get; set; } = "";
    public string QuestionId { get; set; } = "";
    public string ChoiceId { get; set; } = "";
    public DateTime AnsweredAtUtc { get; set; } = DateTime.UtcNow;
}
