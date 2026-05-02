using NetSupport.Shared.Localization;

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
        RightToLeft = RightToLeft.Yes; // Support RTL for Arabic text
        RightToLeftLayout = true;

        Controls.Add(new Label
        {
            Text = $"{LocalizationResources.GetString("Message.ComputerLocked", AppLanguage.English)}\n{LocalizationResources.GetString("Message.ComputerLocked", AppLanguage.Arabic)}",
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 28, FontStyle.Bold)
        });
    }
}
