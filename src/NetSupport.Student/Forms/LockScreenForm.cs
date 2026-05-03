using NetSupport.Shared.Localization;

namespace NetSupport.Student.Forms;

public sealed class LockScreenForm : Form
{
    private bool _allowClose;

    public LockScreenForm()
    {
        Text = "Locked";
        StartPosition = FormStartPosition.Manual;
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.FromArgb(20, 20, 20);
        RightToLeft = RightToLeft.Yes; // Support RTL for Arabic text
        RightToLeftLayout = true;

        Bounds = Screen.PrimaryScreen.Bounds;

        Controls.Add(new Label
        {
            Text = $"{LocalizationResources.GetString("Message.ComputerLocked", AppLanguage.English)}\n{LocalizationResources.GetString("Message.ComputerLocked", AppLanguage.Arabic)}",
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 28, FontStyle.Bold)
        });

        FormClosing += OnFormClosing;
        Shown += (_, _) => BringLockToFront();
        Activated += (_, _) => BringLockToFront();
    }

    public void Unlock()
    {
        if (IsDisposed)
        {
            return;
        }

        _allowClose = true;
        Close();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
        }
    }

    private void BringLockToFront()
    {
        TopMost = true;
        BringToFront();
        Activate();
    }
}
