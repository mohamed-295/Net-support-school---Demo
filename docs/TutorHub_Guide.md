# Member 4 Guide — How to Use the SignalR Communication Layer

This guide explains how Member 4 should implement `StudentClient.cs` and `HeartbeatService.cs` in `NetSupport.Student/Services/` to connect to the Tutor's SignalR hub.

---

## What Member 3 Already Set Up

### Tutor Side (you don't need to touch these)

**`TutorHub.cs`** — The SignalR hub running on the Tutor. It has these methods your client will call:

| Hub Method | Parameters | What it Does |
|------------|-----------|--------------|
| `RegisterStudent` | `StudentInfo student` | Registers the student and notifies the Tutor dashboard |
| `SendHeartbeat` | `StudentInfo student` | Updates last-seen timestamp on the Tutor |
| `SendProgress` | `StudentProgress progress` | Reports test progress (answered count, time left, status) |
| `SubmitAnswers` | `string studentId, List<StudentAnswer> answers` | Submits final answers for scoring |

**`TutorServer.cs`** — Embedded Kestrel server. Default URL: `http://0.0.0.0:5000`. The hub endpoint is at `/tutorHub`.

### What Commands You Will Receive

The hub sends commands to students via a client callback named `"ReceiveCommand"`. The payload is a `TutorCommand` object with a `CommandType` string:

| CommandType | When It Happens | What Your App Should Do |
|-------------|----------------|------------------------|
| `"Lock"` | Tutor locks a student | Show the `LockScreenForm` (Member 5) |
| `"Unlock"` | Tutor unlocks a student | Close the `LockScreenForm` |
| `"StartTest"` | Tutor starts a test | Show test UI — the command also has `Exam` and `DurationMinutes` |
| `"StopTest"` | Tutor stops the test | Auto-submit answers and close the test form |

---

## What You Need to Implement

### 1. NuGet Package (already added)

The `Microsoft.AspNetCore.SignalR.Client` package is already in `NetSupport.Student.csproj`. No action needed.

### 2. StudentClient.cs

Replace the placeholder in `src/NetSupport.Student/Services/StudentClient.cs`.

Here is a reference implementation:

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using NetSupport.Shared.Contracts;
using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

public sealed class StudentClient : IAsyncDisposable
{
    private HubConnection? _connection;

    // True when the SignalR connection is active.
    public bool IsConnected =>
        _connection?.State == HubConnectionState.Connected;

    // Events — subscribe to these in your forms.
    public event Action<TutorCommand>? OnCommandReceived;
    public event Action<bool>? OnConnectionChanged;

    // Connect to the Tutor hub.
    // Example url: "http://192.168.1.100:5000" or "http://localhost:5000"
    public async Task ConnectAsync(string tutorServerUrl)
    {
        if (_connection is not null)
            await DisconnectAsync();

        // Make sure the URL ends with the hub path.
        var hubUrl = tutorServerUrl.TrimEnd('/');
        if (!hubUrl.EndsWith("/tutorHub", StringComparison.OrdinalIgnoreCase))
            hubUrl += "/tutorHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect(new[] {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            })
            .Build();

        // Listen for commands from the Tutor (Lock, Unlock, StartTest, StopTest).
        _connection.On<TutorCommand>("ReceiveCommand", command =>
        {
            OnCommandReceived?.Invoke(command);
        });

        // Track reconnection.
        _connection.Reconnected += _ =>
        {
            OnConnectionChanged?.Invoke(true);
            return Task.CompletedTask;
        };
        _connection.Reconnecting += _ =>
        {
            OnConnectionChanged?.Invoke(false);
            return Task.CompletedTask;
        };
        _connection.Closed += _ =>
        {
            OnConnectionChanged?.Invoke(false);
            return Task.CompletedTask;
        };

        await _connection.StartAsync();
        OnConnectionChanged?.Invoke(true);
    }

    // Disconnect from the hub.
    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
            OnConnectionChanged?.Invoke(false);
        }
    }

    // Register this student with the Tutor dashboard.
    public async Task RegisterAsync(StudentInfo student)
    {
        if (_connection is not null)
            await _connection.InvokeAsync("RegisterStudent", student);
    }

    // Send a heartbeat so the Tutor knows this student is alive.
    public async Task SendHeartbeatAsync(StudentInfo student)
    {
        if (_connection is not null)
            await _connection.InvokeAsync("SendHeartbeat", student);
    }

    // Report test progress.
    public async Task SendProgressAsync(StudentProgress progress)
    {
        if (_connection is not null)
            await _connection.InvokeAsync("SendProgress", progress);
    }

    // Submit final answers.
    public async Task SubmitAnswersAsync(string studentId, List<StudentAnswer> answers)
    {
        if (_connection is not null)
            await _connection.InvokeAsync("SubmitAnswers", studentId, answers);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
```

### 3. HeartbeatService.cs

Replace the placeholder in `src/NetSupport.Student/Services/HeartbeatService.cs`.

Reference implementation:

```csharp
using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

public sealed class HeartbeatService : IDisposable
{
    private readonly StudentClient _client;
    private readonly StudentInfo _student;
    private System.Threading.Timer? _timer;

    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

    public HeartbeatService(StudentClient client, StudentInfo student)
    {
        _client = client;
        _student = student;
    }

    public void Start()
    {
        Stop();
        _timer = new System.Threading.Timer(
            callback: _ => SendHeartbeat(),
            state: null,
            dueTime: Interval,
            period: Interval);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private async void SendHeartbeat()
    {
        try
        {
            if (_client.IsConnected)
            {
                _student.LastSeenUtc = DateTime.UtcNow;
                await _client.SendHeartbeatAsync(_student);
            }
        }
        catch
        {
            // Swallow so the timer keeps running through brief network hiccups.
        }
    }

    public void Dispose() => Stop();
}
```

---

## How to Wire It Up in StudentLoginForm

After the student fills in their name/id and clicks Connect:

```csharp
// 1. Create the client and connect.
var client = new StudentClient();
await client.ConnectAsync(tutorUrlTextBox.Text);   // e.g. "http://localhost:5000"

// 2. Register the student.
var student = new StudentInfo
{
    StudentId = idTextBox.Text,
    FullName = nameTextBox.Text,
    MachineName = Environment.MachineName
};
await client.RegisterAsync(student);

// 3. Start heartbeat.
var heartbeat = new HeartbeatService(client, student);
heartbeat.Start();

// 4. Listen for Tutor commands.
client.OnCommandReceived += command =>
{
    // This fires on a background thread — use Invoke to touch UI.
    this.Invoke(() =>
    {
        switch (command.CommandType)
        {
            case "Lock":
                new LockScreenForm().Show();
                break;
            case "Unlock":
                // Close the lock form.
                break;
            case "StartTest":
                // command.Exam has the exam data.
                // command.DurationMinutes has the time limit.
                break;
            case "StopTest":
                // Auto-submit and close test.
                break;
        }
    });
};
```

---

## Quick Test

1. Run the **Tutor** app first — it starts the SignalR server on port 5000.
2. Run the **Student** app and connect to `http://localhost:5000`.
3. The student should appear in the Tutor's dashboard (once Member 2 implements the grid).

If you see a connection error, make sure port 5000 isn't blocked by Windows Firewall.
