using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

public sealed class TestSessionManager
{
    public TestSession? ActiveSession { get; private set; }

    public TestSession CreateSession(Exam exam, int durationMinutes, IEnumerable<string> studentIds)
    {
        ActiveSession = new TestSession
        {
            Exam = exam,
            DurationMinutes = durationMinutes,
            StudentIds = studentIds.ToList(),
            Status = "Running"
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
