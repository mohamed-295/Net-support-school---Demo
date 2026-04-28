namespace NetSupport.Designer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Forms.ExamDesignerForm());
    }
}
