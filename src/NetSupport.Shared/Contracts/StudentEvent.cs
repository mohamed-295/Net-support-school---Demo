using NetSupport.Shared.Models;

namespace NetSupport.Shared.Contracts;

public sealed class StudentEvent
{
    public string EventType { get; set; } = "";
    public StudentInfo Student { get; set; } = new();
    public StudentProgress? Progress { get; set; }
    public List<StudentAnswer> Answers { get; set; } = new();
}
