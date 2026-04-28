namespace NetSupport.Tutor.Server;

public sealed class TutorServer
{
    public bool IsRunning { get; private set; }

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }
}
