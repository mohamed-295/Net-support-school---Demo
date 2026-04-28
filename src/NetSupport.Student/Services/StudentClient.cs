namespace NetSupport.Student.Services;

// Placeholder — Member 4 will implement the real SignalR client here.
// See docs/TutorHub_Guide.md for how to connect to the TutorHub.
public sealed class StudentClient
{
    public bool IsConnected { get; private set; }

    public Task ConnectAsync(string tutorServerUrl)
    {
        IsConnected = !string.IsNullOrWhiteSpace(tutorServerUrl);
        return Task.CompletedTask;
    }
}
