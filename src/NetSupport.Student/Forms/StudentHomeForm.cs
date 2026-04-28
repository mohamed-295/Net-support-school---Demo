namespace NetSupport.Student.Forms;

public sealed class StudentHomeForm : Form
{
    public StudentHomeForm()
    {
        Text = "Student Home";
        Width = 800;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(new Label
        {
            Text = "Student connected. Member 8 will add test navigation here.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
