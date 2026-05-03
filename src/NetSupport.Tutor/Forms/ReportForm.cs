using System.ComponentModel;
using System.Diagnostics;
using NetSupport.Shared.Models;
using NetSupport.Tutor.Services;

namespace NetSupport.Tutor.Forms;

public sealed class ReportForm : Form
{
    private readonly StudentRegistry _studentRegistry;
    private readonly TestSessionManager _sessionManager;
    private readonly DataGridView _gridReports;
    private readonly BindingList<ReportRow> _bindingList = new();

    public ReportForm(StudentRegistry registry, TestSessionManager sessionManager)
    {
        _studentRegistry = registry;
        _sessionManager = sessionManager;

        Text = "Test Reports";
        Width = 1000;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.Absolute, 60)
            }
        };

        _gridReports = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        _gridReports.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Student Name", DataPropertyName = "StudentName" },
            new DataGridViewTextBoxColumn { HeaderText = "Score", DataPropertyName = "Score" },
            new DataGridViewTextBoxColumn { HeaderText = "Answered", DataPropertyName = "AnsweredQuestions" },
            new DataGridViewTextBoxColumn { HeaderText = "Total Questions", DataPropertyName = "TotalQuestions" }
        );

        mainLayout.Controls.Add(_gridReports, 0, 0);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var btnExport = new Button { Text = "Export to HTML", Width = 150, Height = 35 };
        btnExport.Click += ExportToHtml;
        buttonPanel.Controls.Add(btnExport);

        mainLayout.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(mainLayout);
        _gridReports.DataSource = _bindingList;

        LoadReportData();
    }

    private void LoadReportData()
    {
        var activeSession = _sessionManager.ActiveSession;
        var totalQuestions = activeSession?.Exam.Questions.Count ?? 0;

        foreach (var student in _studentRegistry.Students)
        {
            // Only include students who were part of the session or have a score
            if (activeSession != null && !activeSession.StudentIds.Contains(student.StudentId))
                continue;

            _bindingList.Add(new ReportRow
            {
                StudentId = student.StudentId,
                StudentName = student.FullName,
                Score = student.Score,
                AnsweredQuestions = student.AnsweredCount,
                TotalQuestions = totalQuestions
            });
        }
    }

    private void ExportToHtml(object? sender, EventArgs e)
    {
        if (_bindingList.Count == 0)
        {
            MessageBox.Show("No data to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var html = ReportService.CreateHtmlReport(_bindingList);
        var filePath = Path.Combine(Path.GetTempPath(), $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        File.WriteAllText(filePath, html);

        MessageBox.Show($"Report exported to: {filePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Open the report in default browser
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }
}
