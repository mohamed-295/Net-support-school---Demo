using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetSupport.Shared.Models;
using System.Threading.Tasks;
using System;

namespace NetSupport.Tutor.Server;

// Hosts an embedded ASP.NET Core Kestrel server that exposes the
// TutorHub SignalR endpoint.  The Tutor WinForms app creates one
// instance, calls StartAsync(), and StopAsync() when shutting down.
//
// Other members can use the HubContext property to send commands
// to student clients from anywhere in the Tutor app.
public sealed class TutorServer : IAsyncDisposable
{
    private WebApplication? _app;

    public static TutorServer Instance { get; } = new TutorServer();

    public event Action<StudentInfo> OnStudentRegistered;
    public event Action<StudentInfo> OnHeartbeatReceived;

    // The URL the server listens on.
    // Default: http://0.0.0.0:5000 (accepts connections from any machine on the LAN).
    public string ListenUrl { get; set; } = "http://0.0.0.0:5000";

    // Whether the embedded server is currently running.
    public bool IsRunning { get; private set; }

    // Provides access to the SignalR hub context so forms and services
    // can send commands to connected students without going through a hub method.
    public IHubContext<TutorHub>? HubContext { get; private set; }

    private TutorServer() { }
    public void NotifyStudentRegistered(StudentInfo student)
    {
        OnStudentRegistered?.Invoke(student);
    }

    public void NotifyHeartbeatReceived(StudentInfo student)
    {
        OnHeartbeatReceived?.Invoke(student);
    }


    // Builds the ASP.NET Core pipeline, registers SignalR, maps the hub,
    // and starts Kestrel.  Safe to call from the WinForms UI thread.
    public async Task StartAsync()
    {
        if (IsRunning)
        {
            return;
        }

        var builder = WebApplication.CreateBuilder();

        // Keep console output quiet during the demo.
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.WebHost.UseUrls(ListenUrl);

        // Register SignalR with generous limits for exam JSON payloads.
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
        });

        // Allow any origin so Student apps on different machines can connect.
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        _app = builder.Build();
        _app.UseCors();
        _app.MapHub<TutorHub>("/tutorHub");

        // Grab the hub context so the rest of the Tutor app can use it.
        HubContext = _app.Services.GetRequiredService<IHubContext<TutorHub>>();

        await _app.StartAsync();
        IsRunning = true;
    }

    // Gracefully shuts down the embedded server.
    public async Task StopAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }

        HubContext = null;
        IsRunning = false;
    }

    // Sends a TutorCommand to a specific student by their studentId.
    public async Task SendCommandToStudentAsync(string studentId,
        Shared.Contracts.TutorCommand command)
    {
        var connectionId = TutorHub.GetConnectionId(studentId);
        if (connectionId is not null && HubContext is not null)
        {
            await HubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveCommand", command);
        }
    }

    // Broadcasts a TutorCommand to every connected student.
    public async Task SendCommandToAllAsync(Shared.Contracts.TutorCommand command)
    {
        if (HubContext is not null)
        {
            await HubContext.Clients.All.SendAsync("ReceiveCommand", command);
        }
    }



    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
