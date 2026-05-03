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
        Application.Run(new TutorDashboardForm());

        if (tutorServer.IsRunning)
        {
            tutorServer.StopAsync().GetAwaiter().GetResult();
        }
        
    }
}
