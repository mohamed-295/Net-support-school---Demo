namespace NetSupport.Student.Forms;

public sealed class LockScreenForm : Form
{
    public LockScreenForm()
    {
        Text = "Locked";
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        BackColor = Color.FromArgb(20, 20, 20);

        Controls.Add(new Label
        {
            Text = "Computer locked by Tutor\nتم قفل الجهاز بواسطة المعلم",
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 28, FontStyle.Bold)
        });
    }
}
