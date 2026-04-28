namespace NetSupport.Student.Services;

public sealed class HeartbeatService
{
    public TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);
}
