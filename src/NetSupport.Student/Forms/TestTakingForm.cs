namespace NetSupport.Student.Forms;

public sealed class TestTakingForm : Form
{
    public TestTakingForm()
    {
        Text = "Student Test";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(new Label
        {
            Text = "Member 8 will implement MCQ navigation and submission.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
