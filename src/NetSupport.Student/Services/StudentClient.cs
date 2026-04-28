namespace NetSupport.Student.Services;

public sealed class StudentClient
{
    public bool IsConnected { get; private set; }

    public Task ConnectAsync(string tutorServerUrl)
    {
        IsConnected = !string.IsNullOrWhiteSpace(tutorServerUrl);
        return Task.CompletedTask;
    }
}
