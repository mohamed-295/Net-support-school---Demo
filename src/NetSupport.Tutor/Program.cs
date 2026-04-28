namespace NetSupport.Tutor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Forms.TutorDashboardForm());
    }
}
