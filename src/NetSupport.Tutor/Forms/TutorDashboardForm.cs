using System.ComponentModel;
using NetSupport.Shared.Localization;
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
    private Label _titleLabel;
    private AppLanguage _currentLanguage = AppLanguage.English;

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
        _tutorServer = TutorServer.Instance;
        _testSessionManager = new TestSessionManager();

        // ================= Layout =================
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.Absolute, 50),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.Absolute, 70)
            }
        };

        // ================= Header with Language Toggle =================
        var headerPanel = new Panel { Dock = DockStyle.Fill };
        var headerLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(10)
        };

        _titleLabel = new Label
        {
            Text = LocalizationResources.GetString("Dashboard.ConnectedStudents", _currentLanguage),
            AutoSize = true,
            Font = new Font("Segoe UI", 14, FontStyle.Bold)
        };
        headerLayout.Controls.Add(_titleLabel);

        var langToggleBtn = new Button
        {
            Text = "العربية",
            Width = 80,
            Height = 30
        };
        langToggleBtn.Click += (s, e) => ToggleLanguage();
        headerLayout.Controls.Add(langToggleBtn);

        headerPanel.Controls.Add(headerLayout);
        mainPanel.Controls.Add(headerPanel, 0, 0);

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
                new DataGridViewTextBoxColumn { HeaderText=LocalizationResources.GetString("Dashboard.Name", _currentLanguage), DataPropertyName="FullName" },
                new DataGridViewTextBoxColumn { HeaderText=LocalizationResources.GetString("Dashboard.Machine", _currentLanguage), DataPropertyName="MachineName" },
                new DataGridViewTextBoxColumn { HeaderText=LocalizationResources.GetString("Dashboard.Status", _currentLanguage), DataPropertyName="Status" },
                new DataGridViewTextBoxColumn { HeaderText=LocalizationResources.GetString("Dashboard.Answered", _currentLanguage), DataPropertyName="AnsweredCount" },
                new DataGridViewTextBoxColumn { HeaderText=LocalizationResources.GetString("Dashboard.Score", _currentLanguage), DataPropertyName="Score" },
                new DataGridViewTextBoxColumn { Name="LastSeenUtc", HeaderText=LocalizationResources.GetString("Dashboard.LastSeen", _currentLanguage), DataPropertyName="LastSeenUtc" }
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

        var buttons = new (string Key, EventHandler? Click)[]
        {
            ("Button.Lock", LockStudent),
            ("Button.Unlock", UnlockStudent),
            ("Button.SetupTest", SetupTest),
            ("Button.StartTest", StartTest),
            ("Button.StopTest", StopTest),
            ("Button.LiveTracking", OpenLiveTracking),
            ("Button.Report", OpenReport)
        };

        var buttonList = new List<Button>();
        foreach (var (key, click) in buttons)
        {
            var btn = new Button
            {
                Text = LocalizationResources.GetString(key, _currentLanguage),
                Width = 110,
                Height = 35,
                Enabled = false,
                Tag = key // Store key for retranslation
            };
            btn.Click += click;
            buttonList.Add(btn);
            flowLayout.Controls.Add(btn);
        }

        buttonPanel.Controls.Add(flowLayout);
        mainPanel.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(mainPanel);

        // ================= Data =================
        _bindingList = new BindingList<StudentInfo>();
        _gridStudents.DataSource = _bindingList;

        AddSampleStudents();

        // ================= Real-time Linking =================
        _tutorServer.OnStudentRegistered += (student) =>
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() => 
                {
                
                    var exists = _bindingList.Any(s => s.StudentId == student.StudentId);
                    if (!exists)
                    {
                        _bindingList.Add(student);
                        _studentRegistry.Upsert(student);
                    }
                }));
            }
        };

        _tutorServer.OnHeartbeatReceived += (student) =>
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() => 
                {
                    _studentRegistry.Upsert(student);
                    _gridStudents.Refresh(); 
                }));
            }
        };


        // ================= Timer =================
        _refreshTimer = new System.Windows.Forms.Timer();
        _refreshTimer.Interval = 1000;
        _refreshTimer.Tick += (s, e) => RefreshLastSeenColumn();
        _refreshTimer.Start();
    }

    // ================= Language Toggle =================
    private void ToggleLanguage()
    {
        _currentLanguage = _currentLanguage == AppLanguage.English ? AppLanguage.Arabic : AppLanguage.English;

        // Update UI labels
        Text = LocalizationResources.GetString("Dashboard.Title", _currentLanguage);
        _titleLabel.Text = LocalizationResources.GetString("Dashboard.ConnectedStudents", _currentLanguage);

        // Update grid column headers
        _gridStudents.Columns[0].HeaderText = LocalizationResources.GetString("Dashboard.Name", _currentLanguage);
        _gridStudents.Columns[1].HeaderText = LocalizationResources.GetString("Dashboard.Machine", _currentLanguage);
        _gridStudents.Columns[2].HeaderText = LocalizationResources.GetString("Dashboard.Status", _currentLanguage);
        _gridStudents.Columns[3].HeaderText = LocalizationResources.GetString("Dashboard.Answered", _currentLanguage);
        _gridStudents.Columns[4].HeaderText = LocalizationResources.GetString("Dashboard.Score", _currentLanguage);
        _gridStudents.Columns[5].HeaderText = LocalizationResources.GetString("Dashboard.LastSeen", _currentLanguage);

        // Update button texts
        foreach (Control control in GetAllControls(this))
        {
            if (control is Button btn && btn.Tag is string key && key.StartsWith("Button."))
            {
                btn.Text = LocalizationResources.GetString(key, _currentLanguage);
            }
        }

        // Set RTL if Arabic
        this.RightToLeft = _currentLanguage == AppLanguage.Arabic ? RightToLeft.Yes : RightToLeft.No;
        this.RightToLeftLayout = _currentLanguage == AppLanguage.Arabic;
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