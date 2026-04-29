using System;
using System.Threading.Tasks;


namespace NetSupport.Student.Services;

public sealed class HeartbeatService
{
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Func<Task> _onTick;

    public HeartbeatService(Func<Task> onTick)
    {
        _onTick = onTick;
        _timer = new System.Windows.Forms.Timer { Interval = 5000 }; 
        _timer.Tick += async (s, e) => await _onTick();
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
}