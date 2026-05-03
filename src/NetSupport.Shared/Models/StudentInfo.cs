namespace NetSupport.Shared.Models;

public sealed class StudentInfo
{
    public string StudentId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string MachineName { get; set; } = "";
    public string Status { get; set; } = "Disconnected";
    public int AnsweredCount { get; set; }
    /// <summary>Graded result as correct/total (e.g. "7/10"), empty until submission is scored.</summary>
    public string Score { get; set; } = "";
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
}
