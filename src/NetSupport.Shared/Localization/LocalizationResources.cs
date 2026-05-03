namespace NetSupport.Shared.Localization;

public static class LocalizationResources
{
    public static Dictionary<string, Dictionary<AppLanguage, string>> Resources = new()
    {
        // Tutor Dashboard
        { "Dashboard.Title", new() { { AppLanguage.English, "NetSupport Tutor - Dashboard" }, { AppLanguage.Arabic, "لوحة التحكم - معلم" } } },
        { "Dashboard.ConnectedStudents", new() { { AppLanguage.English, "Connected Students" }, { AppLanguage.Arabic, "الطلاب المتصلون" } } },
        { "Dashboard.Name", new() { { AppLanguage.English, "Name" }, { AppLanguage.Arabic, "الاسم" } } },
        { "Dashboard.Machine", new() { { AppLanguage.English, "Machine" }, { AppLanguage.Arabic, "الجهاز" } } },
        { "Dashboard.Status", new() { { AppLanguage.English, "Status" }, { AppLanguage.Arabic, "الحالة" } } },
        { "Dashboard.Answered", new() { { AppLanguage.English, "Answered" }, { AppLanguage.Arabic, "الإجابات" } } },
        { "Dashboard.Score", new() { { AppLanguage.English, "Score" }, { AppLanguage.Arabic, "الدرجة" } } },
        { "Dashboard.LastSeen", new() { { AppLanguage.English, "Last Seen" }, { AppLanguage.Arabic, "آخر ظهور" } } },

        // Button Labels
        { "Button.Lock", new() { { AppLanguage.English, "Lock" }, { AppLanguage.Arabic, "قفل" } } },
        { "Button.Unlock", new() { { AppLanguage.English, "Unlock" }, { AppLanguage.Arabic, "فتح القفل" } } },
        { "Button.SetupTest", new() { { AppLanguage.English, "Setup Test" }, { AppLanguage.Arabic, "إعداد الامتحان" } } },
        { "Button.StartTest", new() { { AppLanguage.English, "Start Test" }, { AppLanguage.Arabic, "بدء الامتحان" } } },
        { "Button.StopTest", new() { { AppLanguage.English, "Stop Test" }, { AppLanguage.Arabic, "إيقاف الامتحان" } } },
        { "Button.LiveTracking", new() { { AppLanguage.English, "Live Tracking" }, { AppLanguage.Arabic, "المتابعة المباشرة" } } },
        { "Button.Report", new() { { AppLanguage.English, "Report" }, { AppLanguage.Arabic, "التقرير" } } },
        { "Button.Close", new() { { AppLanguage.English, "Close" }, { AppLanguage.Arabic, "إغلاق" } } },
        { "Button.Arabic", new() { { AppLanguage.English, "العربية" }, { AppLanguage.Arabic, "العربية" } } },
        { "Button.English", new() { { AppLanguage.English, "English" }, { AppLanguage.Arabic, "English" } } },

        // Status Messages
        { "Status.Active", new() { { AppLanguage.English, "Active" }, { AppLanguage.Arabic, "نشط" } } },
        { "Status.Idle", new() { { AppLanguage.English, "Idle" }, { AppLanguage.Arabic, "عاطل" } } },
        { "Status.InTest", new() { { AppLanguage.English, "In Test" }, { AppLanguage.Arabic, "في الامتحان" } } },
        { "Status.Locked", new() { { AppLanguage.English, "Locked" }, { AppLanguage.Arabic, "مقفول" } } },
        { "Status.Completed", new() { { AppLanguage.English, "Completed" }, { AppLanguage.Arabic, "مكتمل" } } },

        // Messages
        { "Message.Locking", new() { { AppLanguage.English, "Locking {0}" }, { AppLanguage.Arabic, "جاري قفل {0}" } } },
        { "Message.Unlocking", new() { { AppLanguage.English, "Unlocking {0}" }, { AppLanguage.Arabic, "جاري فتح قفل {0}" } } },
        { "Message.ComputerLocked", new() { { AppLanguage.English, "Your computer is locked by the tutor" }, { AppLanguage.Arabic, "جهازك مقفول من قبل المعلم" } } },

        // Student Home
        { "StudentHome.Welcome", new() { { AppLanguage.English, "Welcome" }, { AppLanguage.Arabic, "مرحبا" } } },
        { "StudentHome.Status", new() { { AppLanguage.English, "Status" }, { AppLanguage.Arabic, "الحالة" } } },
        { "StudentHome.Connected", new() { { AppLanguage.English, "Status: Connected to Tutor" }, { AppLanguage.Arabic, "الحالة: متصل بالمعلم" } } },
        { "StudentHome.Disconnected", new() { { AppLanguage.English, "Status: Disconnected" }, { AppLanguage.Arabic, "الحالة: غير متصل" } } },
        { "StudentHome.Waiting", new() { { AppLanguage.English, "Waiting for Tutor commands..." }, { AppLanguage.Arabic, "في انتظار تعليمات المعلم..." } } },
        { "StudentHome.StudentName", new() { { AppLanguage.English, "Student Name" }, { AppLanguage.Arabic, "اسم الطالب" } } },
        { "StudentHome.StudentId", new() { { AppLanguage.English, "Student ID" }, { AppLanguage.Arabic, "رقم الطالب" } } },

        // Test Setup
        { "TestSetup.Title", new() { { AppLanguage.English, "Test Setup" }, { AppLanguage.Arabic, "إعداد الامتحان" } } },
        { "TestSetup.SelectStudents", new() { { AppLanguage.English, "Select Students" }, { AppLanguage.Arabic, "اختر الطلاب" } } },
        { "TestSetup.SelectExam", new() { { AppLanguage.English, "Select Exam" }, { AppLanguage.Arabic, "اختر الامتحان" } } },
        { "TestSetup.Duration", new() { { AppLanguage.English, "Duration (minutes)" }, { AppLanguage.Arabic, "المدة (بالدقائق)" } } },
        { "TestSetup.Start", new() { { AppLanguage.English, "Start" }, { AppLanguage.Arabic, "ابدأ" } } },
        { "TestSetup.StartTest", new() { { AppLanguage.English, "Start Test" }, { AppLanguage.Arabic, "بدء الامتحان" } } },
        { "TestSetup.StopTest", new() { { AppLanguage.English, "Stop Test" }, { AppLanguage.Arabic, "إيقاف الامتحان" } } },
        { "TestSetup.Close", new() { { AppLanguage.English, "Close" }, { AppLanguage.Arabic, "إغلاق" } } },
        { "TestSetup.StudentsGroup", new() { { AppLanguage.English, "Students" }, { AppLanguage.Arabic, "الطلاب" } } },
        { "TestSetup.NoStudentsHint", new() { { AppLanguage.English, "No students connected yet. Open the Student app, sign in to this tutor, then click Refresh." }, { AppLanguage.Arabic, "لا يوجد طلاب متصلون بعد. افتح تطبيق الطالب وسجّل الدخول لهذا المعلم، ثم اضغط تحديث." } } },
        { "TestSetup.ExamGroup", new() { { AppLanguage.English, "Exam & Timing" }, { AppLanguage.Arabic, "الامتحان والتوقيت" } } },
        { "TestSetup.Refresh", new() { { AppLanguage.English, "Refresh" }, { AppLanguage.Arabic, "تحديث" } } },
        { "TestSetup.SelectAll", new() { { AppLanguage.English, "Select All" }, { AppLanguage.Arabic, "تحديد الكل" } } },
        { "TestSetup.Clear", new() { { AppLanguage.English, "Clear" }, { AppLanguage.Arabic, "مسح" } } },
        { "TestSetup.Browse", new() { { AppLanguage.English, "Browse..." }, { AppLanguage.Arabic, "استعراض..." } } },
        { "TestSetup.Reload", new() { { AppLanguage.English, "Reload" }, { AppLanguage.Arabic, "إعادة التحميل" } } },
        { "TestSetup.SelectExamHint", new() { { AppLanguage.English, "Select an exam to see details." }, { AppLanguage.Arabic, "اختر امتحاناً لعرض التفاصيل." } } },
        { "TestSetup.DurationMinutes", new() { { AppLanguage.English, "Duration (minutes):" }, { AppLanguage.Arabic, "المدة (بالدقائق):" } } },
        { "TestSetup.ExamSummaryFormat", new() { { AppLanguage.English, "{0} (Questions: {1})" }, { AppLanguage.Arabic, "{0} (الأسئلة: {1})" } } },
        { "TestSetup.ExamFolderNotFound", new() { { AppLanguage.English, "Exam folder not found." }, { AppLanguage.Arabic, "مجلد الامتحانات غير موجود." } } },
        { "TestSetup.NoExamFiles", new() { { AppLanguage.English, "No exam files found." }, { AppLanguage.Arabic, "لا توجد ملفات امتحانات." } } },
        { "TestSetup.MsgSessionRunning", new() { { AppLanguage.English, "A test session is already running." }, { AppLanguage.Arabic, "هناك جلسة امتحان قيد التشغيل بالفعل." } } },
        { "TestSetup.MsgSelectStudent", new() { { AppLanguage.English, "Select at least one student." }, { AppLanguage.Arabic, "اختر طالباً واحداً على الأقل." } } },
        { "TestSetup.MsgSelectExam", new() { { AppLanguage.English, "Select an exam before starting." }, { AppLanguage.Arabic, "اختر امتحاناً قبل البدء." } } },
        { "TestSetup.MsgStartSent", new() { { AppLanguage.English, "Start command sent to selected students." }, { AppLanguage.Arabic, "تم إرسال أمر البدء للطلاب المحددين." } } },
        { "TestSetup.MsgTestStarted", new() { { AppLanguage.English, "Test Started" }, { AppLanguage.Arabic, "بدأ الامتحان" } } },
        { "TestSetup.MsgSelectStop", new() { { AppLanguage.English, "Select at least one student to stop." }, { AppLanguage.Arabic, "اختر طالباً واحداً على الأقل للإيقاف." } } },
        { "TestSetup.MsgStopSent", new() { { AppLanguage.English, "Stop command sent." }, { AppLanguage.Arabic, "تم إرسال أمر الإيقاف." } } },
        { "TestSetup.MsgTestStopped", new() { { AppLanguage.English, "Test Stopped" }, { AppLanguage.Arabic, "توقف الامتحان" } } },
        { "TestSetup.MsgServerUnavailable", new() { { AppLanguage.English, "Tutor server is not available." }, { AppLanguage.Arabic, "خادم المعلم غير متاح." } } },
        { "TestSetup.MsgExamLoadFailed", new() { { AppLanguage.English, "Could not load the selected exam file." }, { AppLanguage.Arabic, "تعذر تحميل ملف الامتحان المحدد." } } },
        { "TestSetup.MsgExamLoadCaption", new() { { AppLanguage.English, "Exam Load" }, { AppLanguage.Arabic, "تحميل الامتحان" } } },
        { "TestSetup.MsgTestSessionCaption", new() { { AppLanguage.English, "Test Session" }, { AppLanguage.Arabic, "جلسة الامتحان" } } },
        { "TestSetup.MsgTestSetupCaption", new() { { AppLanguage.English, "Test Setup" }, { AppLanguage.Arabic, "إعداد الامتحان" } } },
        { "TestSetup.MsgServerCaption", new() { { AppLanguage.English, "Server" }, { AppLanguage.Arabic, "الخادم" } } },
        { "TestSetup.MsgServerStartFailed", new() { { AppLanguage.English, "Failed to start the tutor server: {0}" }, { AppLanguage.Arabic, "فشل تشغيل خادم المعلم: {0}" } } },

        { "Dashboard.MsgInvalidExam", new() { { AppLanguage.English, "Selected exam is empty or invalid." }, { AppLanguage.Arabic, "الامتحان المحدد فارغ أو غير صالح." } } },
        { "Dashboard.MsgCaptionTest", new() { { AppLanguage.English, "Test Setup" }, { AppLanguage.Arabic, "إعداد الامتحان" } } },
        { "Dashboard.MsgCaptionTutor", new() { { AppLanguage.English, "Tutor" }, { AppLanguage.Arabic, "المعلم" } } },
        { "Dashboard.MsgNoStudentsConnected", new() { { AppLanguage.English, "No students are connected yet. Open a student app and sign in, then try again." }, { AppLanguage.Arabic, "لا يوجد طلاب متصلون بعد. افتح تطبيق الطالب وسجّل الدخول ثم أعد المحاولة." } } },
        { "Dashboard.MsgSelectStudentFirst", new() { { AppLanguage.English, "Select a student in the list first." }, { AppLanguage.Arabic, "اختر طالباً من القائمة أولاً." } } },
        { "Dashboard.MsgNoActiveTest", new() { { AppLanguage.English, "There is no active test session to stop." }, { AppLanguage.Arabic, "لا توجد جلسة امتحان نشطة لإيقافها." } } },
    };

    public static string GetString(string key, AppLanguage language)
    {
        if (Resources.TryGetValue(key, out var translations))
        {
            if (translations.TryGetValue(language, out var text))
                return text;
        }

        return key; // Fallback to key if translation not found
    }
}
