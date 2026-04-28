using NetSupport.Shared.Models;

namespace NetSupport.Shared.Contracts;

public sealed class TutorCommand
{
    public string CommandType { get; set; } = "";
    public string SessionId { get; set; } = "";
    public Exam? Exam { get; set; }
    public int DurationMinutes { get; set; }
}
