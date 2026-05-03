using System.ComponentModel;
using NetSupport.Shared.Models;
using NetSupport.Tutor.Server;
using NetSupport.Tutor.Services;

namespace NetSupport.Tutor.Forms;

public sealed class LiveTrackingForm : Form
{
    private readonly TutorServer _tutorServer;
    private readonly StudentRegistry _studentRegistry;
    private readonly TestSessionManager _sessionManager;
    private readonly DataGridView _gridTracking;
    private readonly BindingList<StudentProgressDisplay> _bindingList = new();
    private readonly System.Windows.Forms.Timer _refreshTimer;

    public LiveTrackingForm(StudentRegistry registry, TestSessionManager sessionManager)
    {
        _studentRegistry = registry;
        _sessionManager = sessionManager;
        _tutorServer = TutorServer.Instance;

        Text = "Live Test Tracking";
        Width = 1000;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        _gridTracking = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        _gridTracking.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Student Name", DataPropertyName = "StudentName" },
            new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status" },
            new DataGridViewTextBoxColumn { HeaderText = "Answered", DataPropertyName = "Answered" },
            new DataGridViewTextBoxColumn { HeaderText = "Remaining Time", DataPropertyName = "RemainingTimeDisplay" }
        );

        Controls.Add(_gridTracking);
        _gridTracking.DataSource = _bindingList;

        InitializeStudents();

        _tutorServer.OnProgressUpdated += HandleProgressUpdated;

        _refreshTimer = new System.Windows.Forms.Timer();
        _refreshTimer.Interval = 1000;
        _refreshTimer.Tick += (s, e) => RefreshGrid();
        _refreshTimer.Start();
    }

    private void InitializeStudents()
    {
        var activeSession = _sessionManager.ActiveSession;
        if (activeSession == null) return;

        foreach (var studentId in activeSession.StudentIds)
        {
            var student = _studentRegistry.Students.FirstOrDefault(s => s.StudentId == studentId);
            _bindingList.Add(new StudentProgressDisplay
            {
                StudentId = studentId,
                StudentName = student?.FullName ?? studentId,
                Status = "Waiting",
                AnsweredCount = 0,
                TotalQuestions = activeSession.Exam.Questions.Count,
                RemainingSeconds = activeSession.DurationMinutes * 60
            });
        }
    }

    private void HandleProgressUpdated(StudentProgress progress)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HandleProgressUpdated(progress)));
            return;
        }

        var index = -1;
        for (var i = 0; i < _bindingList.Count; i++)
        {
            if (_bindingList[i].StudentId == progress.StudentId)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            var item = _bindingList[index];
            item.Status = progress.Status;
            item.AnsweredCount = progress.AnsweredCount;
            item.TotalQuestions = progress.TotalQuestions;
            item.RemainingSeconds = progress.RemainingSeconds;

            _bindingList.ResetItem(index);
        }
    }

    private void RefreshGrid()
    {
        // Periodic refresh to update "Remaining Time" display
        _gridTracking.Invalidate();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tutorServer.OnProgressUpdated -= HandleProgressUpdated;
            _refreshTimer?.Dispose();
        }
        base.Dispose(disposing);
    }

    private class StudentProgressDisplay
    {
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string Status { get; set; } = "";
        public int AnsweredCount { get; set; }
        public int TotalQuestions { get; set; }
        public int RemainingSeconds { get; set; }

        public string Answered => $"{AnsweredCount}/{TotalQuestions}";
        public string RemainingTimeDisplay
        {
            get
            {
                if (RemainingSeconds <= 0) return "Time's up";
                var span = TimeSpan.FromSeconds(RemainingSeconds);
                return $"{(int)span.TotalMinutes}:{span.Seconds:D2}";
            }
        }
    }
}
