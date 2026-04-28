using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

// Placeholder — Member 4 will implement the real heartbeat timer here.
// See docs/TutorHub_Guide.md for how to wire this up with StudentClient.
public sealed class HeartbeatService
{
    public TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);
}
