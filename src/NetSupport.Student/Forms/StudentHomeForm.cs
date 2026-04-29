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
        var lblWelcome = new Label 
        { 
            Text = $"Welcome, {_studentInfo.FullName} (ID: {_studentInfo.StudentId})", 
            ForeColor = Color.White, 
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(15, 22),
            AutoSize = true
        };
        pnlHeader.Controls.Add(lblWelcome);

        lblStatus = new Label 
        { 
            Text = "Status: Connected", 
            Dock = DockStyle.Bottom, 
            Height = 30, 
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            BackColor = Color.LightGray,
            Font = new Font("Segoe UI", 9)
        };

        lblCommandReceived = new Label
        {
            Text = "Waiting for Tutor commands...",
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

    private void UpdateStatus(string status)
    {
        if (lblStatus.InvokeRequired)
        {
            lblStatus.Invoke(new Action(() => UpdateStatus(status)));
            return;
        }
        lblStatus.Text = $"Status: {status}";
        lblStatus.ForeColor = status == "Connected" ? Color.Green : Color.Red;
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