namespace NetSupport.Shared.Models;

public sealed class ReportRow
{
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    /// <summary>Correct out of total, e.g. "7/10".</summary>
    public string Score { get; set; } = "";
    public int AnsweredQuestions { get; set; }
    public int TotalQuestions { get; set; }
}
