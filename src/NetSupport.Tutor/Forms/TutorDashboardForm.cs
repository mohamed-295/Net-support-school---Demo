using System.ComponentModel;
using NetSupport.Shared.Models;
using NetSupport.Tutor.Server;
using NetSupport.Tutor.Services;

namespace NetSupport.Tutor.Forms;

public sealed class TutorDashboardForm : Form
{
    private readonly StudentRegistry _studentRegistry;
    private readonly TutorServer _tutorServer;
    private readonly TestSessionManager _testSessionManager;
    private readonly DataGridView _gridStudents;
    private readonly System.Windows.Forms.Timer _refreshTimer;

    private BindingList<StudentInfo> _bindingList = new();
    private StudentInfo? _selectedStudent;
    private bool _isRefreshing = false;

    public TutorDashboardForm()
    {
        Text = "NetSupport Tutor - Dashboard";
        Width = 1800;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        _studentRegistry = new StudentRegistry();
        _tutorServer = new TutorServer();
        _testSessionManager = new TestSessionManager();

        // ================= Layout =================
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.Absolute, 70)
            }
        };

        var title = new Label
        {
            Text = "Connected Students",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Margin = new Padding(10, 0, 0, 0)
        };
        mainPanel.Controls.Add(title, 0, 0);

        // ================= Grid =================
        _gridStudents = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        _gridStudents.Columns.AddRange(
            new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText="Name", DataPropertyName="FullName" },
                new DataGridViewTextBoxColumn { HeaderText="Machine", DataPropertyName="MachineName" },
                new DataGridViewTextBoxColumn { HeaderText="Status", DataPropertyName="Status" },
                new DataGridViewTextBoxColumn { HeaderText="Answered", DataPropertyName="AnsweredCount" },
                new DataGridViewTextBoxColumn { HeaderText="Score", DataPropertyName="Score" },
                new DataGridViewTextBoxColumn { Name="LastSeenUtc", HeaderText="Last Seen", DataPropertyName="LastSeenUtc" }
            }
        );

        _gridStudents.CellFormatting += GridStudents_CellFormatting;

        // ✅ SAFE selection handling
        _gridStudents.SelectionChanged += (s, e) =>
        {
            if (_isRefreshing) return;

            if (_gridStudents.CurrentRow?.DataBoundItem is StudentInfo student)
            {
                _selectedStudent = student;
                UpdateButtonStates();
            }
        };

        mainPanel.Controls.Add(_gridStudents, 0, 1);

        // ================= Buttons =================
        var buttonPanel = new Panel { Dock = DockStyle.Fill };
        var flowLayout = new FlowLayoutPanel { Dock = DockStyle.Fill };

        var buttons = new (string Text, EventHandler? Click)[]
        {
            ("Lock", LockStudent),
            ("Unlock", UnlockStudent),
            ("Setup Test", SetupTest),
            ("Start Test", StartTest),
            ("Stop Test", StopTest),
            ("Live Tracking", OpenLiveTracking),
            ("Report", OpenReport)
        };

        foreach (var (text, click) in buttons)
        {
            var btn = new Button
            {
                Text = text,
                Width = 110,
                Height = 35,
                Enabled = false
            };
            btn.Click += click;
            flowLayout.Controls.Add(btn);
        }

        buttonPanel.Controls.Add(flowLayout);
        mainPanel.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(mainPanel);

        // ================= Data =================
        _bindingList = new BindingList<StudentInfo>();
        _gridStudents.DataSource = _bindingList;

        AddSampleStudents();

        // ================= Timer =================
        _refreshTimer = new System.Windows.Forms.Timer();
        _refreshTimer.Interval = 1000;
        _refreshTimer.Tick += (s, e) => RefreshLastSeenColumn();
        _refreshTimer.Start();
    }

    // 🔁 Smooth refresh (no rebinding)
    private void RefreshLastSeenColumn()
    {
        if (_gridStudents.Columns.Contains("LastSeenUtc"))
        {
            int colIndex = _gridStudents.Columns["LastSeenUtc"].Index;
            _gridStudents.InvalidateColumn(colIndex);
        }
    }

    private void GridStudents_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_gridStudents.Columns[e.ColumnIndex].Name != "LastSeenUtc")
            return;

        if (e.Value is DateTime lastSeen)
        {
            var diff = DateTime.UtcNow - lastSeen;

            if (diff.TotalSeconds < 60)
                e.Value = $"{(int)diff.TotalSeconds}s ago";
            else if (diff.TotalMinutes < 60)
                e.Value = $"{(int)diff.TotalMinutes}m ago";
            else
                e.Value = lastSeen.ToLocalTime().ToString("g");

            e.FormattingApplied = true;
        }
    }

    private void UpdateButtonStates()
    {
        bool enabled = _selectedStudent != null;

        foreach (Control control in GetAllControls(this))
        {
            if (control is Button btn)
                btn.Enabled = enabled;
        }
    }

    private IEnumerable<Control> GetAllControls(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            yield return c;
            foreach (var child in GetAllControls(c))
                yield return child;
        }
    }

    // ================= Actions =================
    private void LockStudent(object? sender, EventArgs e)
    {
        if (_selectedStudent == null) return;
        MessageBox.Show($"Locking {_selectedStudent.FullName}");
    }

    private void UnlockStudent(object? sender, EventArgs e)
    {
        if (_selectedStudent == null) return;
        MessageBox.Show($"Unlocking {_selectedStudent.FullName}");
    }

    private void SetupTest(object? sender, EventArgs e)
    {
        if (_selectedStudent == null) return;

        using var form = new TestSetupForm(_studentRegistry, _tutorServer, _testSessionManager);
        form.ShowDialog(this);
    }

    private void StartTest(object? sender, EventArgs e)
    {
        if (_selectedStudent == null) return;
        MessageBox.Show($"Starting test for {_selectedStudent.FullName}");
    }

    private void StopTest(object? sender, EventArgs e)
    {
        if (_selectedStudent == null) return;
        MessageBox.Show($"Stopping test for {_selectedStudent.FullName}");
    }

    private void OpenLiveTracking(object? sender, EventArgs e)
    {
        if (_selectedStudent == null) return;

        var form = new LiveTrackingForm();
        form.Show(this);
    }

    private void OpenReport(object? sender, EventArgs e)
    {
        if (_selectedStudent == null) return;

        using var form = new ReportForm();
        form.ShowDialog(this);
    }

    // ================= Sample Data =================
    private void AddSampleStudents()
    {
        var students = new[]
        {
            new StudentInfo { StudentId="ST-001", FullName="Ahmed Hassan", MachineName="PC-LAB-01", Status="Active", AnsweredCount=12, Score=85, LastSeenUtc=DateTime.UtcNow.AddSeconds(-5)},
            new StudentInfo { StudentId="ST-002", FullName="Fatima Mohamed", MachineName="PC-LAB-02", Status="In Test", AnsweredCount=8, Score=72, LastSeenUtc=DateTime.UtcNow.AddSeconds(-30)},
            new StudentInfo { StudentId="ST-003", FullName="Omar Ali", MachineName="PC-LAB-03", Status="Idle", AnsweredCount=0, Score=0, LastSeenUtc=DateTime.UtcNow.AddMinutes(-2)},
            new StudentInfo { StudentId="ST-004", FullName="Noor Khalid", MachineName="PC-LAB-04", Status="Active", AnsweredCount=20, Score=95, LastSeenUtc=DateTime.UtcNow.AddSeconds(-15)}
        };

        foreach (var s in students)
        {
            _studentRegistry.Upsert(s);
            _bindingList.Add(s);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _refreshTimer?.Dispose();

        base.Dispose(disposing);
    }
}