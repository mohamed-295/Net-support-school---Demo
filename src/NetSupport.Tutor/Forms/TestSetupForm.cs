namespace NetSupport.Tutor.Forms;

public sealed class TestSetupForm : Form
{
    public TestSetupForm()
    {
        Text = "Test Setup";
        Width = 800;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label
        {
            Text = "Member 7 will implement student selection, exam selection, duration, start and stop.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
