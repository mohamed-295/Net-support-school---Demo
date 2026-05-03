using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using NetSupport.Shared.Contracts;
using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Server;

// SignalR hub that handles all real-time communication between
// the Tutor app and connected Student apps.
//
// Students call methods here to register, heartbeat, report progress,
// and submit answers.  The Tutor sends commands (Lock, Unlock,
// StartTest, StopTest) through the hub to reach specific students.
public sealed class TutorHub : Hub
{
    // Maps StudentId -> SignalR ConnectionId so the Tutor can target specific students.
    private static readonly ConcurrentDictionary<string, string> StudentConnections = new();

    // ────────────────────────────────────────────────────────────────────────
    //  Student -> Hub  (called by Student apps)
    // ────────────────────────────────────────────────────────────────────────

    // Called by a Student app when it first connects.
    // Stores the studentId-to-connectionId mapping and notifies Tutor clients.
    public async Task RegisterStudent(StudentInfo student)
    {
        student.Status = "Connected";
        student.LastSeenUtc = DateTime.UtcNow;

        StudentConnections[student.StudentId] = Context.ConnectionId;
        TutorServer.Instance.NotifyStudentRegistered(student);

        // Tell all Tutor clients that a new student registered.
        await Clients.Others.SendAsync("StudentRegistered", student);
    }

    // Called by a Student app every few seconds to confirm it is still alive.
    public async Task SendHeartbeat(StudentInfo student)
    {
        student.LastSeenUtc = DateTime.UtcNow;
        student.Status = "Connected";

        StudentConnections[student.StudentId] = Context.ConnectionId;

        await Clients.Others.SendAsync("HeartbeatReceived", student);
    }

    // Called by a Student app to report test progress (answered count, remaining time, status).
    public async Task SendProgress(StudentProgress progress)
    {
        TutorServer.Instance.NotifyProgressUpdated(progress);
        await Clients.Others.SendAsync("ProgressUpdated", progress);
    }

    // Called by a Student app to submit its final answers for scoring.
    public async Task SubmitAnswers(string studentId, List<StudentAnswer> answers)
    {
        TutorServer.Instance.NotifyAnswersSubmitted(studentId, answers);
        await Clients.Others.SendAsync("AnswersSubmitted", studentId, answers);
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Tutor -> Student  (called by the Tutor app via IHubContext)
    // ────────────────────────────────────────────────────────────────────────

    // Sends a command (Lock, Unlock, StartTest, StopTest) to one specific student.
    public async Task SendCommandToStudent(string studentId, TutorCommand command)
    {
        if (StudentConnections.TryGetValue(studentId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveCommand", command);
        }
    }

    // Broadcasts a command to every connected student.
    public async Task SendCommandToAll(TutorCommand command)
    {
        await Clients.All.SendAsync("ReceiveCommand", command);
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Connection Lifecycle
    // ────────────────────────────────────────────────────────────────────────

    // Removes the student from the connection map when they disconnect
    // and notifies Tutor clients so the dashboard can mark them offline.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var entry = StudentConnections.FirstOrDefault(
            kvp => kvp.Value == Context.ConnectionId);

        if (!string.IsNullOrEmpty(entry.Key))
        {
            StudentConnections.TryRemove(entry.Key, out _);

            var disconnected = new StudentInfo
            {
                StudentId = entry.Key,
                Status = "Disconnected",
                LastSeenUtc = DateTime.UtcNow
            };

            TutorServer.Instance.NotifyStudentDisconnected(disconnected);

            await Clients.Others.SendAsync("StudentDisconnected", disconnected);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Static Helpers
    // ────────────────────────────────────────────────────────────────────────

    // Returns the SignalR connection ID for a given student, or null if not connected.
    public static string? GetConnectionId(string studentId)
    {
        StudentConnections.TryGetValue(studentId, out var connectionId);
        return connectionId;
    }

    // Returns a snapshot of all currently connected student IDs.
    public static IReadOnlyCollection<string> GetConnectedStudentIds()
    {
        return StudentConnections.Keys.ToList().AsReadOnly();
    }
}
