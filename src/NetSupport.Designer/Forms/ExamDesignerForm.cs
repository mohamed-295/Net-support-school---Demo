namespace NetSupport.Designer.Forms;

public sealed class ExamDesignerForm : Form
{
    public ExamDesignerForm()
    {
        Text = "MCQ Exam Designer";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(new Label
        {
            Text = "Member 6 will implement MCQ exam creation and JSON save/load.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
