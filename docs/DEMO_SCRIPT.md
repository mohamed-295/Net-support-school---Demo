# 5-Minute Demo Script

## 0:00-0:30 - Team And Source Control

- Show the GitHub repository.
- Show branches named `feature/member-XX-topic`.
- Show commit history with all 10 GitHub usernames.
- Mention that every member implemented one focused part using AI-agent assistance.

## 0:30-1:00 - Designer

- Open the Designer app.
- Load or create an MCQ exam.
- Show question text, four choices, correct answer, and duration.
- Save the exam as JSON under `samples/exams`.

## 1:00-1:40 - Tutor And Student Detection

- Open Tutor.
- Open two Student app windows.
- Enter different student names/ids.
- Show both students appearing automatically in Tutor.

## 1:40-2:10 - Lock And Unlock

- Select one student in Tutor.
- Click Lock.
- Show the full-screen Student lock screen.
- Click Unlock.
- Show the Student screen returns to normal.

## 2:10-3:20 - Test Flow

- In Tutor, choose students, exam, and duration.
- Start the test.
- In Student, confirm login and navigate questions.
- Answer questions and submit.

## 3:20-4:10 - Live Tracking And Stop Test

- Show Tutor live tracking with status and answered count.
- Stop the test from Tutor.
- Show Student test closes/submits.

## 4:10-4:40 - Report

- Open the Tutor report.
- Show student names, scores, answered questions, and total questions.
- Show printable HTML/report view.

## 4:40-4:50 - Arabic Support

- Load the Arabic sample exam "اختبار تجريبي في الحاسب واللغة العربية" in Designer.
- Show Arabic question text and Arabic choices displayed correctly (right-to-left).
- Show Arabic labels on Tutor dashboard and Student forms if available.
- Demonstrate that the UI supports RTL layout for Arabic text.

## 4:50-5:00 - Setup And Future Features

- Show `output/setup` folder containing Tutor, Student, and Designer executables.
- Show sample exams in both English and Arabic.
- Show README with build, run, and packaging instructions.
- Mention future-year enhancements: Windows service installer, remote screen viewing, file distribution, chat, attendance analytics, browser lockdown, question bank import/export, role-based accounts, and cloud classroom features.

---

## Future-Year Feature Suggestions

1. **Real Windows Service Installer** - Student app runs as a system service at startup instead of manual launch
2. **Remote Screen Viewing** - Tutor can view student screen in real-time
3. **File Distribution** - Tutor can distribute files to connected students
4. **Tutor/Student Chat** - Real-time messaging between Tutor and Students
5. **Attendance Analytics** - Track student login time, session duration, test completion rates
6. **Browser Lockdown** - Restrict browser access during exams
7. **Question Bank Import/Export** - Bulk import questions from CSV or XML, export to learning management systems
8. **Role-Based Accounts** - Multiple tutors, assistant roles, admin dashboard
9. **Cloud Classroom Mode** - Online exam delivery, student roster from cloud storage
