# Member 5 Work Explanation - Lock/Unlock Feature

## Overview

This document summarizes the Lock/Unlock feature implemented on the member-05 branch. The focus is strictly on the additions that enable a Tutor to lock or unlock a selected Student machine through the existing SignalR command pipeline.

---

## Branch

feature/member-05-lock-unlock

---

## Features Added

- Tutor dashboard buttons to trigger Lock and Unlock commands for the selected student.
- Student client handling of the ReceiveCommand event to show or hide a lock screen.
- Full-screen lock form that is topmost and cannot be closed unless an Unlock command is received.

---

## Files Modified

### 1. TutorDashboardForm.cs

Location: src/NetSupport.Tutor/Forms/TutorDashboardForm.cs

Changes:

- Added Lock and Unlock buttons to the dashboard button panel.
- Implemented LockStudent and UnlockStudent handlers that send TutorCommand with CommandType set to "Lock" or "Unlock".
- Uses TutorServer.SendCommandToStudentAsync to deliver the command to the selected student.

Snippet:

```csharp
private async void LockStudent(object? sender, EventArgs e)
{
	if (_selectedStudent == null) return;
	var command = new TutorCommand { CommandType = "Lock" };
	await _tutorServer.SendCommandToStudentAsync(_selectedStudent.StudentId, command);
}

private async void UnlockStudent(object? sender, EventArgs e)
{
	if (_selectedStudent == null) return;
	var command = new TutorCommand { CommandType = "Unlock" };
	await _tutorServer.SendCommandToStudentAsync(_selectedStudent.StudentId, command);
}
```

### 2. StudentClient.cs

Location: src/NetSupport.Student/Services/StudentClient.cs

Changes:

- Subscribed to ReceiveCommand from the SignalR hub.
- Added logic to react to "Lock" and "Unlock" commands by showing or hiding the lock screen.

Snippet:

```csharp
_connection.On<TutorCommand>("ReceiveCommand", (command) =>
{
	var hostForm = Application.OpenForms.Cast<Form>().FirstOrDefault();
	hostForm?.Invoke(new Action(async () =>
	{
		switch (command.CommandType)
		{
			case "Lock":
				ShowLockScreen();
				break;
			case "Unlock":
				HideLockScreen();
				break;
			case "StartTest":
				await HandleStartTestAsync(command);
				break;
			case "StopTest":
				HandleStopTest();
				break;
		}
	}));
});
```

### 3. LockScreenForm.cs

Location: src/NetSupport.Student/Forms/LockScreenForm.cs

Changes:

- Implemented a full-screen, topmost lock form.
- Prevents closing unless Unlock is called.
- Ensures the form stays in front via activation and topmost behavior.

Snippet:

```csharp
public void Unlock()
{
	if (IsDisposed)
	{
		return;
	}

	_allowClose = true;
	Close();
}

private void OnFormClosing(object? sender, FormClosingEventArgs e)
{
	if (!_allowClose)
	{
		e.Cancel = true;
	}
}
```

---

## How It Works

1. The Tutor selects a student row in the dashboard and clicks Lock or Unlock.
2. The dashboard sends a TutorCommand to the selected student through the server.
3. The Student client receives ReceiveCommand and either shows the lock screen or closes it.

---

## Quick Verification Steps

1. Run the Tutor app and the Student app.
2. Connect a student to the Tutor server.
3. Select the student in the Tutor dashboard and click Lock to show the lock screen.
4. Click Unlock to close the lock screen.
