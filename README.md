# NetSupport School Demo

This repository contains a 4-hour MVP demo for a classroom management system inspired by NetSupport School.

The demo is split into three Windows apps:

- **Tutor**: detects connected students, sends lock/unlock commands, starts/stops tests, tracks progress, and prints reports.
- **Student**: connects to the Tutor app, receives commands, opens tests, submits answers, and shows a demo lock screen.
- **Designer**: creates MCQ exams and saves them as JSON files.

## Tech Stack

- C# / .NET 8
- WinForms
- SignalR planned for Tutor/Student live communication
- JSON files for exams and reports

## Repository Structure

```text
src/
  NetSupport.Shared/    Shared models, contracts, JSON helpers
  NetSupport.Tutor/     Tutor desktop app
  NetSupport.Student/   Student desktop app
  NetSupport.Designer/  MCQ exam designer app
docs/
  TEAM_PLAN.md
  DEMO_SCRIPT.md
  SUBMISSION_FORM_DATA.md
samples/
  exams/
```

## Requirements

Install the .NET 8 SDK before building:

https://dotnet.microsoft.com/download/dotnet/8.0

## Build

```powershell
dotnet restore
dotnet build NetSupportSchool.sln
```

## Run

```powershell
dotnet run --project src/NetSupport.Tutor
dotnet run --project src/NetSupport.Student
dotnet run --project src/NetSupport.Designer
```

## Publish For Submission

```powershell
dotnet publish src/NetSupport.Tutor -c Release -o output/setup/Tutor
dotnet publish src/NetSupport.Student -c Release -o output/setup/Student
dotnet publish src/NetSupport.Designer -c Release -o output/setup/Designer
```

Then zip the `output/setup` folder with this README and `samples/exams`.

## Important Demo Notes

- Student auto-detection is implemented by students registering with the Tutor server.
- Lock/unlock is a safe demo simulation using a full-screen Student form.
- The Student app can be manually opened for the demo; real Windows service installation is listed as a future improvement.
- Arabic support should include RTL layout and Arabic labels/sample exam.

## Team Plan

See `TEAM_PLAN.md` for the 10-member task split, branch names, AI-agent prompts, and 4-hour schedule.
