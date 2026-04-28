namespace NetSupport.Student.Forms;

public sealed class StudentLoginForm : Form
{
    public StudentLoginForm()
    {
        Text = "Student Login";
        Width = 700;
        Height = 420;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(new Label
        {
            Text = "Member 4 will implement student name, id, tutor URL, and connection.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
