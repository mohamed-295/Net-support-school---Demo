namespace NetSupport.Designer.Forms;

public sealed class QuestionEditorForm : Form
{
    public QuestionEditorForm()
    {
        Text = "Question Editor";
        Width = 800;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label
        {
            Text = "Member 6 will implement question text, choices, and correct answer selection.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
