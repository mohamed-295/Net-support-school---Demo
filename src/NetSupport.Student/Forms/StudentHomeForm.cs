using NetSupport.Shared.Localization;
using NetSupport.Shared.Models;
using NetSupport.Student.Services;
using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client; 

namespace NetSupport.Student.Forms;

public sealed class StudentHomeForm : Form
{
    private readonly StudentClient _client;
    private readonly StudentInfo _studentInfo;
    private readonly Label lblStatus;
    private readonly Label lblCommandReceived;
    private readonly Label lblWelcome;
    private AppLanguage _currentLanguage = AppLanguage.English;

    public StudentHomeForm(StudentClient client, StudentInfo studentInfo)
    {
        _client = client;
        _studentInfo = studentInfo;

        Text = "NetSupport Student - Active Session";
        Width = 600;
        Height = 400;

        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(240, 240, 240);

        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.DarkSlateBlue };
        
        var headerLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(15, 0, 15, 0)
        };

        lblWelcome = new Label 
        { 
            Text = $"{LocalizationResources.GetString("StudentHome.Welcome", _currentLanguage)}, {_studentInfo.FullName}", 
            ForeColor = Color.White, 
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true
        };
        headerLayout.Controls.Add(lblWelcome);

        var langToggleBtn = new Button
        {
            Text = "العربية",
            Width = 80,
            Height = 30,
            ForeColor = Color.White,
            BackColor = Color.DarkSlateGray,
            FlatStyle = FlatStyle.Flat
        };
        langToggleBtn.Click += (s, e) => ToggleLanguage();
        headerLayout.Controls.Add(langToggleBtn);

        pnlHeader.Controls.Add(headerLayout);

        lblStatus = new Label 
        { 
            Text = LocalizationResources.GetString("StudentHome.Connected", _currentLanguage), 
            Dock = DockStyle.Bottom, 
            Height = 30, 
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            BackColor = Color.LightGray,
            Font = new Font("Segoe UI", 9)
        };

        lblCommandReceived = new Label
        {
            Text = LocalizationResources.GetString("StudentHome.Waiting", _currentLanguage),
            Location = new Point(0, 160),
            Size = new Size(600, 80),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11, FontStyle.Italic),
            ForeColor = Color.Gray
        };


        _client.OnStatusChanged += UpdateStatus;

        // _client.HubConnection.On<string>("ReceiveCommand", (cmd) => ShowCommand(cmd));
        var homeControls = new Control[] { pnlHeader, lblCommandReceived, lblStatus };
        Controls.AddRange(homeControls);
    }

    private void ToggleLanguage()
    {
        _currentLanguage = _currentLanguage == AppLanguage.English ? AppLanguage.Arabic : AppLanguage.English;

        lblWelcome.Text = $"{LocalizationResources.GetString("StudentHome.Welcome", _currentLanguage)}, {_studentInfo.FullName}";
        UpdateStatusLabel();

        this.RightToLeft = _currentLanguage == AppLanguage.Arabic ? RightToLeft.Yes : RightToLeft.No;
        this.RightToLeftLayout = _currentLanguage == AppLanguage.Arabic;
    }

    private void UpdateStatus(string status)
    {
        if (lblStatus.InvokeRequired)
        {
            lblStatus.Invoke(new Action(() => UpdateStatus(status)));
            return;
        }
        UpdateStatusLabel();
        lblStatus.ForeColor = status == "Connected" ? Color.Green : Color.Red;
    }

    private void UpdateStatusLabel()
    {
        string statusKey = lblStatus.Text.Contains("Connected") ? "StudentHome.Connected" : "StudentHome.Disconnected";
        lblStatus.Text = LocalizationResources.GetString(statusKey, _currentLanguage);
    }

    private void ShowCommand(string command)
    {
        if (lblCommandReceived.InvokeRequired)
        {
            lblCommandReceived.Invoke(new Action(() => ShowCommand(command)));
            return;
        }

        lblCommandReceived.Text = $"Last Command from Tutor: {command}";
        lblCommandReceived.ForeColor = Color.Blue;
    }
}