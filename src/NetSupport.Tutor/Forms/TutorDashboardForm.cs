namespace NetSupport.Tutor.Forms;

public sealed class TutorDashboardForm : Form
{
    public TutorDashboardForm()
    {
        Text = "NetSupport Tutor";
        Width = 1000;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;

        var title = new Label
        {
            Text = "Tutor Dashboard - Member 2 will implement connected students and controls",
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter
        };

        Controls.Add(title);
    }
}
