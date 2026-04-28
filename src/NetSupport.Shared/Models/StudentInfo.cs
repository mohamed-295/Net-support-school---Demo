namespace NetSupport.Shared.Models;

public sealed class StudentInfo
{
    public string StudentId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string MachineName { get; set; } = "";
    public string Status { get; set; } = "Disconnected";
    public int AnsweredCount { get; set; }
    public int Score { get; set; }
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
}
