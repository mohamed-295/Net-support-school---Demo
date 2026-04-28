namespace NetSupport.Shared.Models;

public sealed class ReportRow
{
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public int Score { get; set; }
    public int AnsweredQuestions { get; set; }
    public int TotalQuestions { get; set; }
}
