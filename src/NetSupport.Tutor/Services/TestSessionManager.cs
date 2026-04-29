using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

public sealed class TestSessionManager
{
    public TestSession? ActiveSession { get; private set; }

    public bool HasActiveSession =>
        ActiveSession is not null &&
        string.Equals(ActiveSession.Status, "Running", StringComparison.OrdinalIgnoreCase);

    public TestSession CreateSession(Exam exam, int durationMinutes, IEnumerable<string> studentIds)
    {
        ActiveSession = new TestSession
        {
            Exam = exam,
            DurationMinutes = durationMinutes,
            StudentIds = studentIds.ToList(),
            Status = "Running",
            StartedAtUtc = DateTime.UtcNow
        };

        return ActiveSession;
    }

    public void StopSession()
    {
        if (ActiveSession is not null)
        {
            ActiveSession.Status = "Stopped";
        }
    }
}
