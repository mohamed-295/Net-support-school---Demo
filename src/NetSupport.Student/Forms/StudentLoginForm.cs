using NetSupport.Shared.Models;
using NetSupport.Shared.Storage;
using NetSupport.Student.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetSupport.Student.Forms;

public sealed class StudentLoginForm : Form
{
    private readonly TextBox txtFullName;
    private readonly TextBox txtStudentId;
    private readonly Button btnConnect;
    private readonly Label lblStatus;

    public StudentLoginForm()
    {
        Text = "NetSupport Student - Login";
        Width = 900;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 560);
        BackColor = Color.FromArgb(244, 247, 252);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(24, 20, 24, 20)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 620));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 470));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(36, 32, 36, 28),
            BorderStyle = BorderStyle.FixedSingle
        };

        var formLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 10
        };
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lblTitle = new Label
        {
            Text = "Student Login",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = Color.FromArgb(24, 41, 94)
        };
        var lblSubtitle = new Label
        {
            Text = "Join your instructor session in seconds.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(108, 117, 134)
        };
        var lblName = new Label
        {
            Text = "Full Name",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(62, 68, 82)
        };

        txtFullName = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            PlaceholderText = "Enter your full name",
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 8, 0, 0)
        };
        var lblId = new Label
        {
            Text = "Student ID",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(62, 68, 82)
        };

        txtStudentId = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            PlaceholderText = "Enter your ID",
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 8, 0, 0)
        };

        var lblHint = new Label
        {
            Text = "Server address is managed by instructor settings.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            ForeColor = Color.FromArgb(116, 122, 136)
        };

        btnConnect = new Button
        {
            Text = "Connect & Login",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(53, 126, 240),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 8, 0, 0)
        };
        btnConnect.FlatAppearance.BorderSize = 0;
        btnConnect.Click += BtnConnect_Click;

        lblStatus = new Label
        {
            Text = "Ready",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(112, 118, 132),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
        };

        formLayout.Controls.Add(lblTitle, 0, 0);
        formLayout.Controls.Add(lblSubtitle, 0, 1);
        formLayout.Controls.Add(lblName, 0, 2);
        formLayout.Controls.Add(txtFullName, 0, 3);
        formLayout.Controls.Add(lblId, 0, 4);
        formLayout.Controls.Add(txtStudentId, 0, 5);
        formLayout.Controls.Add(lblHint, 0, 6);
        formLayout.Controls.Add(btnConnect, 0, 7);
        formLayout.Controls.Add(lblStatus, 0, 8);
        card.Controls.Add(formLayout);
        root.Controls.Add(card, 1, 1);
        Controls.Add(root);

        AcceptButton = btnConnect;
    }

    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtStudentId.Text))
        {
            MessageBox.Show("Please enter your name and ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnConnect.Enabled = false;
        lblStatus.Text = "Connecting to tutor...";
        lblStatus.ForeColor = Color.FromArgb(49, 96, 196);

        try
        {
            var studentInfo = new StudentInfo
            {
                StudentId = txtStudentId.Text,
                FullName = txtFullName.Text,
                MachineName = Environment.MachineName,
                Status = "Connected",
                LastSeenUtc = DateTime.UtcNow
            };

            var settings = TutorConnectionSettings.Load();
            var client = new StudentClient();
            await client.ConnectAsync(settings.StudentHubUrl, studentInfo);

            var homeForm = new StudentHomeForm(client, studentInfo);
            homeForm.FormClosed += (_, __) => Close();
            homeForm.Show();
            this.Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Connection failed.";
            lblStatus.ForeColor = Color.FromArgb(196, 49, 67);
            btnConnect.Enabled = true;
        }
    }
}