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
