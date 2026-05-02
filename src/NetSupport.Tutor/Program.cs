using NetSupport.Tutor.Server;
using NetSupport.Tutor.Forms;
namespace NetSupport.Tutor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var tutorServer = TutorServer.Instance;

        // Start server WITHOUT await
        tutorServer.StartAsync().GetAwaiter().GetResult();
        Application.Run(new TutorDashboardForm());

        tutorServer.StopAsync().GetAwaiter().GetResult();
        
    }
}
