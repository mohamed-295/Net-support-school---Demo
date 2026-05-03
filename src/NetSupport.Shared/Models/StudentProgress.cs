namespace NetSupport.Shared.Models;

public sealed class StudentProgress
{
    public string StudentId { get; set; } = "";
    public string SessionId { get; set; } = "";
    public int AnsweredCount { get; set; }
    public int TotalQuestions { get; set; }
    public int RemainingSeconds { get; set; }
    public string Status { get; set; } = "Waiting";
    public List<StudentAnswer> Answers { get; set; } = new();
}
