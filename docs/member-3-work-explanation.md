# Member 3 Work Explanation — SignalR Hub and Communication Layer

## Overview

This document provides a detailed explanation of the work completed by Member 3 for the NetSupport School Demo project. The scope covers the entire real-time communication infrastructure that connects the Tutor application to Student applications using SignalR, a library for adding real-time web functionality to .NET applications.

The communication layer is the backbone of the system. Every feature that involves interaction between the Tutor and Student apps (lock/unlock, test start/stop, heartbeat monitoring, progress tracking, answer submission) relies on the components implemented here.

---

## Branch

`feature/member-03-communication`

---

## Files Created or Modified

### 1. TutorHub.cs

**Location:** `src/NetSupport.Tutor/Server/TutorHub.cs`

**Purpose:** This is the central SignalR hub that all Student applications connect to. It acts as a message broker between the Tutor and Students.

**What it does:**

The hub inherits from `Microsoft.AspNetCore.SignalR.Hub`, which gives it the ability to receive method calls from connected clients and send messages back to them.

It maintains a static `ConcurrentDictionary<string, string>` that maps each student's logical ID to their SignalR connection ID. This mapping is necessary because SignalR assigns a random connection ID to each client, but the Tutor app needs to address students by their student ID (the one they entered in the login form).

**Hub methods available for Student clients to call:**

- `RegisterStudent(StudentInfo student)` — Called once when a student connects. It stores the student-to-connection mapping, sets the student status to "Connected", records the current UTC time, and broadcasts a `StudentRegistered` event to all other clients (the Tutor dashboard listens for this to refresh its student grid).

- `SendHeartbeat(StudentInfo student)` — Called every 5 seconds by the student. Updates the last-seen timestamp and broadcasts a `HeartbeatReceived` event to the Tutor so it knows the student is still alive.

- `SendProgress(StudentProgress progress)` — Called during a test to report how many questions the student has answered, how much time remains, and the student's current status (e.g. "InProgress" or "Submitted"). The Tutor's live tracking form listens for `ProgressUpdated` events.

- `SubmitAnswers(string studentId, List<StudentAnswer> answers)` — Called when the student submits their test. Broadcasts an `AnswersSubmitted` event so the Tutor can store and score the answers.

**Hub methods available for the Tutor to call:**

- `SendCommandToStudent(string studentId, TutorCommand command)` — Sends a command to one specific student. Looks up the student's connection ID from the dictionary and uses `Clients.Client(connectionId)` to target them.

- `SendCommandToAll(TutorCommand command)` — Broadcasts a command to every connected student using `Clients.All`.

**Connection lifecycle:**

The hub overrides `OnDisconnectedAsync` to clean up when a student disconnects. It finds the student whose connection ID matches the one being disconnected, removes them from the dictionary, and broadcasts a `StudentDisconnected` event so the Tutor dashboard can mark that student as offline.

**Static helper methods:**

- `GetConnectionId(string studentId)` — Returns the SignalR connection ID for a given student. Used by `TutorServer` when sending commands through `IHubContext`.

- `GetConnectedStudentIds()` — Returns a snapshot of all currently connected student IDs. Useful for the Tutor dashboard to know who is online.

---

### 2. TutorServer.cs

**Location:** `src/NetSupport.Tutor/Server/TutorServer.cs`

**Purpose:** Hosts an embedded ASP.NET Core Kestrel web server inside the WinForms Tutor application. This server exposes the TutorHub at the `/tutorHub` endpoint.

**Why an embedded server:** The Tutor application is a WinForms desktop app, not a web application. However, SignalR requires an ASP.NET Core server to host the hub. Rather than running a separate web server process, the server is embedded directly into the Tutor app using `WebApplication.CreateBuilder()`. This keeps deployment simple (just run the Tutor exe) and aligns with the demo-ready nature of the project.

**Key properties:**

- `ListenUrl` — The URL and port the server binds to. Defaults to `http://0.0.0.0:5000`, which means it accepts connections from any network interface. This is important for lab demos where Student apps may run on different machines.

- `IsRunning` — Boolean flag indicating whether the server is currently active.

- `HubContext` — An `IHubContext<TutorHub>` reference obtained from the ASP.NET Core dependency injection container after the server starts. This is the primary mechanism for the Tutor app's forms and services to send commands to students without needing to go through a hub method invocation.

**StartAsync method:**

1. Creates a `WebApplication.Builder`.
2. Configures logging to only show warnings (to keep the demo console clean).
3. Sets the listen URL via `builder.WebHost.UseUrls()`.
4. Registers SignalR services with a 1 MB maximum message size (large enough to transmit exam JSON payloads).
5. Configures CORS to allow any origin, header, and method (necessary for cross-machine connections in a lab environment).
6. Builds the application, maps the TutorHub to `/tutorHub`, and starts listening.
7. Retrieves the `IHubContext<TutorHub>` from the service provider and stores it in the `HubContext` property.

**StopAsync method:**

Gracefully shuts down the Kestrel server, disposes the web application, and clears the `HubContext` reference.

**Convenience methods:**

- `SendCommandToStudentAsync(string studentId, TutorCommand command)` — Looks up the student's connection ID using `TutorHub.GetConnectionId()` and sends the command through `HubContext`.

- `SendCommandToAllAsync(TutorCommand command)` — Broadcasts a command to all connected students through `HubContext`.

These methods exist so that other team members (Member 2 for the dashboard, Member 7 for test flow) can send commands with a single method call without needing to understand the SignalR internals.

---

### 3. NetSupport.Tutor.csproj

**Location:** `src/NetSupport.Tutor/NetSupport.Tutor.csproj`

**Change:** Added a `FrameworkReference` to `Microsoft.AspNetCore.App`.

**Rationale:** The Tutor project uses `Microsoft.NET.Sdk` (not `Microsoft.NET.Sdk.Web`) because it is a WinForms application. To gain access to ASP.NET Core APIs (WebApplication, SignalR Hub, IHubContext, Kestrel), the project needs an explicit framework reference. This approach adds ASP.NET Core capabilities without changing the project SDK, which would break WinForms support.

---

### 4. NetSupport.Student.csproj

**Location:** `src/NetSupport.Student/NetSupport.Student.csproj`

**Change:** Added the `Microsoft.AspNetCore.SignalR.Client` NuGet package (version 10.0.7).

**Rationale:** The Student application needs the SignalR client library to connect to the TutorHub. This is the official Microsoft client package that provides the `HubConnectionBuilder` and `HubConnection` classes.

---

### 5. StudentClient.cs and HeartbeatService.cs (Placeholders)

**Location:** `src/NetSupport.Student/Services/`

**Status:** These files were left as placeholders for Member 4 to implement. Each file contains a comment pointing to `docs/TutorHub_Guide.md`, which provides full reference implementations, method signatures, and wiring examples.

---

## Files NOT Modified

The following files were intentionally left untouched to avoid conflicts with other team members' work:

- All shared models in `NetSupport.Shared/Models/` (StudentInfo, Exam, Question, Choice, TestSession, StudentAnswer, StudentProgress, ReportRow)
- All shared contracts in `NetSupport.Shared/Contracts/` (TutorCommand, StudentEvent) — these were already well-structured for the communication layer
- All Tutor forms (TutorDashboardForm, TestSetupForm, LiveTrackingForm, ReportForm)
- All Tutor services (StudentRegistry, TestSessionManager, ReportService)
- All Student forms (StudentLoginForm, StudentHomeForm, LockScreenForm, TestTakingForm)
- TestAnswerService.cs
- The entire Designer project
- All documentation files
- Program.cs in both Tutor and Student projects

---

## SignalR Events Reference

The following table lists every event name used in the communication layer. Other team members need to subscribe to these events in their code to react to real-time updates.

### Events sent FROM the hub TO the Tutor app

| Event Name | Payload | Triggered When |
|------------|---------|---------------|
| `StudentRegistered` | `StudentInfo` | A student connects and registers for the first time |
| `HeartbeatReceived` | `StudentInfo` | A student sends a periodic heartbeat |
| `ProgressUpdated` | `StudentProgress` | A student reports test progress |
| `AnswersSubmitted` | `string studentId, List<StudentAnswer>` | A student submits their final answers |
| `StudentDisconnected` | `StudentInfo` | A student's connection drops |

### Events sent FROM the hub TO Student apps

| Event Name | Payload | Triggered When |
|------------|---------|---------------|
| `ReceiveCommand` | `TutorCommand` | The Tutor sends a command (Lock, Unlock, StartTest, StopTest) |

---

## Command Types Reference

The `TutorCommand.CommandType` string determines what action the Student app should take:

| CommandType | Additional Data | Expected Student Behavior |
|-------------|----------------|--------------------------|
| `"Lock"` | None | Display a full-screen topmost lock form |
| `"Unlock"` | None | Close the lock form |
| `"StartTest"` | `Exam` object, `DurationMinutes` integer | Show the test-taking form with the provided exam |
| `"StopTest"` | `SessionId` | Auto-submit current answers and close the test form |

---

## Technical Decisions

### Why ConcurrentDictionary for connection mapping

The hub methods can be called concurrently from multiple student connections. A regular `Dictionary` would not be thread-safe and could corrupt data under concurrent access. `ConcurrentDictionary` provides lock-free reads and thread-safe writes.

### Why static connection mapping

The `StudentConnections` dictionary is declared as `static` because ASP.NET Core creates a new hub instance for every method call. If the dictionary were an instance field, it would be empty on every call. Making it static ensures the mapping persists across all hub invocations.

### Why IHubContext instead of direct hub calls

The Tutor dashboard and other forms run outside the SignalR pipeline. They cannot call hub methods directly because those methods require an active SignalR connection context. `IHubContext<TutorHub>` is the standard ASP.NET Core pattern for sending messages to hub clients from outside the hub class.

### Why CORS is configured to allow any origin

In a lab demo environment, Student apps may run on different machines with different IP addresses. Restricting CORS origins would require knowing all student machine IPs in advance, which is impractical for a demo. Since this is not a production system, allowing all origins is the simplest and most reliable approach.

### Why the message size limit is set to 1 MB

The `StartTest` command includes the full exam JSON (questions, choices, correct answers). A 10-question exam with 4 choices each produces a JSON payload of roughly 5-10 KB. The 1 MB limit provides generous headroom for larger exams without risking message truncation.

---

## How to Verify the Communication Layer

1. Build the entire solution: `dotnet build NetSupportSchool.sln`
2. Run the Tutor application. It should start without errors and the embedded server should begin listening on port 5000.
3. Once Member 4 implements `StudentClient.cs`, run a Student application and connect to `http://localhost:5000`. The student should appear in the Tutor dashboard (once Member 2 implements the grid).
4. If port 5000 is blocked by Windows Firewall, either allow it through the firewall settings or change the `ListenUrl` property on `TutorServer` before calling `StartAsync()`.
