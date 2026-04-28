namespace NetSupport.Tutor.Forms;

public sealed class LiveTrackingForm : Form
{
    public LiveTrackingForm()
    {
        Text = "Live Tracking";
        Width = 900;
        Height = 550;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label
        {
            Text = "Member 9 will implement live student progress tracking.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
