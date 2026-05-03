using NetSupport.Shared.Models;
using NetSupport.Student.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetSupport.Student.Forms;

public sealed class StudentLoginForm : Form
{
    private readonly TextBox txtFullName;
    private readonly TextBox txtStudentId;
    private readonly TextBox txtTutorUrl;
    private readonly Button btnConnect;
    private readonly Label lblStatus;

    public StudentLoginForm()
    {
        Text = "NetSupport Student - Login";
        Width = 700;
        Height = 420;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;

        int centerX = (700 - 450) / 2;

        Label lblTitle = new Label { Text = "Student Registration", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(centerX, 30), AutoSize = true, ForeColor = Color.DarkSlateBlue };

        Label lblName = new Label { Text = "Full Name:", Font = new Font("Segoe UI", 10), Location = new Point(centerX, 90), AutoSize = true };
        txtFullName = new TextBox { Location = new Point(centerX, 115), Width = 450, Font = new Font("Segoe UI", 11) };

        Label lblId = new Label { Text = "Student ID:", Font = new Font("Segoe UI", 10), Location = new Point(centerX, 155), AutoSize = true };
        txtStudentId = new TextBox { Location = new Point(centerX, 180), Width = 450, Font = new Font("Segoe UI", 11) };

        Label lblUrl = new Label { Text = "Tutor Server URL:", Font = new Font("Segoe UI", 10), Location = new Point(centerX, 220), AutoSize = true };
        txtTutorUrl = new TextBox { Location = new Point(centerX, 245), Width = 450, Font = new Font("Segoe UI", 11), Text = "http://127.0.0.1:5000/tutorHub" };
    
        btnConnect = new Button { 
            Text = "Connect & Login", 
            Location = new Point(centerX, 300), 
            Width = 450, 
            Height = 45, 
            BackColor = Color.LightGreen,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnConnect.Click += BtnConnect_Click;


        lblStatus = new Label { 
            Text = "Ready", 
            Location = new Point(centerX, 355), 
            Width = 450, 
            TextAlign = ContentAlignment.MiddleCenter, 
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9, FontStyle.Italic)
        };

        var controlsArray = new Control[] { 
            lblTitle, lblName, txtFullName, lblId, 
            txtStudentId, lblUrl, txtTutorUrl, btnConnect, lblStatus 
        };
        Controls.AddRange(controlsArray);
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
        lblStatus.ForeColor = Color.Blue;

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

            var client = new StudentClient();
            await client.ConnectAsync(txtTutorUrl.Text, studentInfo);

            var homeForm = new StudentHomeForm(client, studentInfo);
            homeForm.FormClosed += (_, __) => Close();
            homeForm.Show();
            this.Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Connection failed.";
            lblStatus.ForeColor = Color.Red;
            btnConnect.Enabled = true;
        }
    }
}