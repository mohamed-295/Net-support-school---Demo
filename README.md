# NetSupport School Demo

This repository contains a MVP demo for a classroom management system inspired by NetSupport School.

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
- Arabic support is implemented with RTL layout and Arabic labels throughout the UI.

## Arabic Support

This demo includes full Arabic language support:

- **Language Toggle**: Each main form (Tutor Dashboard, Student Home) has an "العربية" (Arabic) button to switch between English and Arabic.
- **RTL Layout**: When Arabic is selected, the UI automatically switches to right-to-left layout (RightToLeft and RightToLeftLayout enabled).
- **Localized Labels**: All UI labels, buttons, and messages are translated to Arabic using the `LocalizationResources` class in `NetSupport.Shared.Localization`.
- **Arabic Sample Exam**: The Designer app can load the sample exam `اختبار_تجريبي_في_الحاسب_واللغة_العربية.json` which contains Arabic questions and choices.
- **Lock Screen Bilingual**: The lock screen displays both English and Arabic messages.

### How to Use Arabic

1. Open any main app (Tutor, Student, or Designer).
2. Click the "العربية" button in the header to switch to Arabic mode.
3. The entire UI will switch to Arabic with RTL layout.
4. Click again to return to English.

### Arabic Sample Exams

Located in `samples/exams/`:

- `اختبار_تجريبي_في_الحاسب_واللغة_العربية.json` - Arabic Computer and Language exam with 3+ questions in Arabic

## Team Plan

See `TEAM_PLAN.md` for the 10-member task split, branch names, AI-agent prompts, and 4-hour schedule.
