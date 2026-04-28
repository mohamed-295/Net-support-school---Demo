namespace NetSupport.Tutor.Forms;

public sealed class ReportForm : Form
{
    public ReportForm()
    {
        Text = "Reports";
        Width = 900;
        Height = 550;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label
        {
            Text = "Member 9 will implement printable test reports.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
