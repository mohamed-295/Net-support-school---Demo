# NetSupport School Demo

This repository contains an MVP demo for a classroom management system inspired by NetSupport School.

The demo is split into three Windows apps:

- **Tutor**: embedded SignalR server, connected students grid, lock/unlock, test setup/start/stop, live tracking (per-question correctness), HTML reports (save location chosen by user), connection settings.
- **Student**: registers with the tutor hub, receives commands, exam UI with timer and question navigation, submits answers and progress.
- **Designer**: creates MCQ exams and saves them as JSON files (English/Arabic UI toggle).

## Tech Stack

- C# / .NET 8
- WinForms
- SignalR (Tutor hosts `TutorHub`; Student clients connect)
- JSON files for exams; HTML export for reports

## Repository Structure

```text
src/
  NetSupport.Shared/    Shared models, contracts, localization, connection settings storage
  NetSupport.Tutor/     Tutor desktop app + embedded Kestrel/SignalR server
  NetSupport.Student/   Student desktop app
  NetSupport.Designer/  MCQ exam designer app
docs/
  TEAM_PLAN.md          (stub linking to root TEAM_PLAN.md)
  DEMO_SCRIPT.md
  SUBMISSION_FORM_DATA.md
samples/
  exams/
INSTALL.txt             End-user install steps (included in submission zip; no build commands)
Pack-SubmissionSetup.ps1  Developers: builds small framework-dependent setup + optional zip
TEAM_PLAN.md            Full team plan and member tasks (root)
```

## Requirements

Install the .NET 8 SDK before building from source:

https://dotnet.microsoft.com/download/dotnet/8.0

Published **self-contained** executables (see below) do not require a separate runtime install on the target PC.

## Build

```powershell
dotnet restore
dotnet build NetSupportSchool.sln
```

## Run (development)

Start **Tutor** first so the hub is listening, then **Student** (and **Designer** anytime for exam authoring):

```powershell
dotnet run --project src/NetSupport.Tutor
dotnet run --project src/NetSupport.Student
dotnet run --project src/NetSupport.Designer
```

## Connection settings (Tutor / Student)

- The tutor configures **listen URL** and **student hub URL** in **Tutor → Settings** (saved under `%LocalAppData%\NetSupportSchool\connection-settings.json`).
- The **Student** login form does not ask for a URL; it uses the same settings file. For a **second PC**, copy that file from the tutor machine or deploy matching `StudentHubUrl` (must reach the tutor host, e.g. `http://TUTOR_IP:5000/tutorHub`).
- Ensure Windows Firewall allows inbound TCP on the chosen port on the tutor machine when students connect over the LAN.

## Publish (portable EXEs for demo / submission)

**Small submission zip (~100 MB limit):** from repo root, with .NET 8 SDK:

```powershell
.\Pack-SubmissionSetup.ps1 -Zip
```

Creates `output\setup\` (Tutor, Student, Designer, `samples\exams`, `README.md`, **`INSTALL.txt`**) and `output\NetSupportSchool-Setup.zip`. **`INSTALL.txt` is only for people who receive the zip** (runtime + run order); it does not include build instructions.

**Option A — smaller output (requires .NET 8 Desktop Runtime on each PC):**

```powershell
dotnet publish src/NetSupport.Tutor/NetSupport.Tutor.csproj -c Release -r win-x64 --self-contained false -o publish/Tutor
dotnet publish src/NetSupport.Student/NetSupport.Student.csproj -c Release -r win-x64 --self-contained false -o publish/Student
dotnet publish src/NetSupport.Designer/NetSupport.Designer.csproj -c Release -r win-x64 --self-contained false -o publish/Designer
```

**Option B — self-contained single-file (larger, no separate runtime; 64-bit Windows):**

```powershell
dotnet publish src/NetSupport.Tutor/NetSupport.Tutor.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/Tutor
dotnet publish src/NetSupport.Student/NetSupport.Student.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/Student
dotnet publish src/NetSupport.Designer/NetSupport.Designer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/Designer
```

Zip each `publish\Tutor`, `publish\Student`, and `publish\Designer` folder (or a single zip that includes all three plus `samples\exams` and this README) for submission.

## Using the three EXE files

If you received a **submission zip**, open **`INSTALL.txt`** first (Desktop Runtime and run order).

You get one folder per app after publish (each contains `NetSupport.Tutor.exe`, `NetSupport.Student.exe`, or `NetSupport.Designer.exe`, plus any files the publish step placed next to them). **Keep each folder intact** when you copy or zip it; do not move only the `.exe` if other files are required.

### 1. NetSupport.Designer.exe (any time)

- Run this on a PC where you want to **create or edit** exam JSON files.
- It does **not** need the Tutor or Student apps running.
- Save exams where the Tutor can open them (for example copy the JSON into `samples\exams` or pick the file from **Test Setup** on the Tutor machine).

### 2. NetSupport.Tutor.exe (instructor — start first)

1. Run **`NetSupport.Tutor.exe`** on the computer that will host the session.
2. Open **Settings** and confirm **Tutor listen URL** (default is often `http://0.0.0.0:5000`) and **Student hub URL** (on the same PC this is often `http://127.0.0.1:5000/tutorHub`). Save if you change them.
3. The dashboard loads and the embedded server should start listening so students can connect.
4. Use **Setup Test** / **Start Test** / **Stop Test**, **Live Tracking**, **Report**, and **Lock** / **Unlock** as needed.

### 3. NetSupport.Student.exe (each student machine)

1. Run **`NetSupport.Student.exe`** after the Tutor is up (or at least before clicking **Connect & Login**).
2. Enter **Full Name** and **Student ID**, then connect. The app reads the hub address from `%LocalAppData%\NetSupportSchool\connection-settings.json` (same keys the Tutor saves from **Settings**).

**Same PC as Tutor (simplest demo):** run Tutor once and save **Settings** so the JSON exists; then run Student on that PC — no extra copy step.

**Another PC on the network:** on the Tutor machine, set **Student hub URL** to something students can reach (for example `http://TUTOR_COMPUTER_IP:5000/tutorHub`). Copy `connection-settings.json` from the Tutor profile’s `NetSupportSchool` folder to the **same path** on each student PC (or edit the file there so `StudentHubUrl` matches). Allow the port through **Windows Firewall** on the Tutor PC.

### Quick classroom flow

1. Instructor: **Tutor** → check **Settings** → students connect with **Student**.
2. Optional: build or load an exam with **Designer**; on **Tutor**, **Setup Test** → choose that exam and students → **Start Test**.
3. Students complete the test; instructor uses **Live Tracking** and **Report** as needed.

Self-contained builds target **64-bit Windows** (`win-x64`). Framework-dependent builds need the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed on each machine.

## Important demo notes

- **Student auto-detection**: students appear when they connect and register with the tutor hub.
- **Lock/unlock**: demo simulation via a full-screen topmost form on the Student app (not OS-level locking).
- **Tests**: tutor starts the test; students receive `StartTest` and open the exam UI; scores in reports use **correct/total** (e.g. `7/10`).
- **Live tracking**: per-student overview plus a detail list of **answered** questions with **Correct / Incorrect** row coloring (uses `Choice.IsCorrect` in exam JSON).
- **Reports**: HTML export via **Save As** (path chosen by the user, not a fixed temp folder).
- **Student as a service**: optional manual launch or startup shortcut; a real Windows service is a documented future improvement.

## Arabic support

- **Tutor** dashboard, test setup, and related messages; **Designer** and question editor; sample exams can be fully Arabic.
- **Language toggle** and **RTL** where implemented (`LocalizationResources` in `NetSupport.Shared.Localization`).
- **Lock screen** shows bilingual messaging.

### Arabic sample exams

Under `samples/exams/`, including files such as `اختبار_تجريبي_في_الحاسب_واللغة_العربية.json`.

## Team plan

See root [`TEAM_PLAN.md`](TEAM_PLAN.md) for the 10-member task split, branches, AI prompts, and demo checklist.
