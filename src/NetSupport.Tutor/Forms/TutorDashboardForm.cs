using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NetSupport.Shared.Contracts;
using NetSupport.Shared.Localization;
using NetSupport.Shared.Models;
using NetSupport.Shared.Storage;
using NetSupport.Tutor.Server;
using NetSupport.Tutor.Services;

namespace NetSupport.Tutor.Forms;

public sealed class TutorDashboardForm : Form
{
    private readonly StudentRegistry _studentRegistry;
    private readonly TutorServer _tutorServer;
    private readonly TestSessionManager _sessionManager;
    private readonly DataGridView _gridStudents;
    private readonly System.Windows.Forms.Timer _refreshTimer;
    private Label _titleLabel;
    private AppLanguage _currentLanguage = AppLanguage.English;

    private BindingList<StudentInfo> _bindingList = new();
    private StudentInfo? _selectedStudent;
    private bool _isRefreshing = false;
    private string? _lastExamPath;

    public TutorDashboardForm()
    {
        Text = "NetSupport Tutor - Dashboard";
        Width = 1800;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        _studentRegistry = new StudentRegistry();
        _tutorServer = TutorServer.Instance;
        _sessionManager = new TestSessionManager();

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
                Enabled = true,
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

        _tutorServer.OnStudentRegistered += HandleStudentRegistered;
        _tutorServer.OnHeartbeatReceived += HandleHeartbeatReceived;
        _tutorServer.OnStudentDisconnected += HandleStudentDisconnected;
        _tutorServer.OnProgressUpdated += HandleProgressUpdated;
        _tutorServer.OnAnswersSubmitted += HandleAnswersSubmitted;

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

    private void HandleStudentRegistered(StudentInfo student) => UpsertStudent(student);

    private void HandleHeartbeatReceived(StudentInfo student) => UpsertStudent(student);

    private void HandleStudentDisconnected(StudentInfo student) => RemoveStudent(student.StudentId);

    private void HandleProgressUpdated(StudentProgress progress) => UpdateStudentProgress(progress);

    private void HandleAnswersSubmitted(string studentId, List<StudentAnswer> answers) => UpdateStudentScore(studentId, answers);

    private void UpsertStudent(StudentInfo student)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpsertStudent(student)));
            return;
        }

        var existing = GetOrCreateStudent(student.StudentId, student.FullName, student.MachineName);

        if (!string.IsNullOrWhiteSpace(student.FullName))
        {
            existing.FullName = student.FullName;
        }

        if (!string.IsNullOrWhiteSpace(student.MachineName))
        {
            existing.MachineName = student.MachineName;
        }

        if (!string.IsNullOrWhiteSpace(student.Status))
        {
            existing.Status = student.Status;
        }

        existing.LastSeenUtc = student.LastSeenUtc;
        _studentRegistry.Upsert(existing);
        ResetStudentRow(existing.StudentId);
    }

    private void UpdateStudentProgress(StudentProgress progress)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateStudentProgress(progress)));
            return;
        }

        var existing = GetOrCreateStudent(progress.StudentId, string.Empty, string.Empty);
        existing.AnsweredCount = progress.AnsweredCount;
        existing.Status = progress.Status;
        existing.LastSeenUtc = DateTime.UtcNow;

        _studentRegistry.Upsert(existing);
        ResetStudentRow(existing.StudentId);
    }

    private void UpdateStudentScore(string studentId, List<StudentAnswer> answers)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateStudentScore(studentId, answers)));
            return;
        }

        var existing = GetOrCreateStudent(studentId, string.Empty, string.Empty);
        existing.AnsweredCount = answers.Count;
        existing.Status = "Submitted";
        existing.LastSeenUtc = DateTime.UtcNow;

        var exam = _sessionManager.ActiveSession?.Exam;
        if (exam is not null && exam.Questions.Count > 0)
        {
            existing.Score = ReportService.CalculateScore(exam, answers);
        }

        _studentRegistry.Upsert(existing);
        ResetStudentRow(existing.StudentId);
    }

    private void RemoveStudent(string studentId)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => RemoveStudent(studentId)));
            return;
        }

        _studentRegistry.Remove(studentId);

        for (var i = 0; i < _bindingList.Count; i++)
        {
            if (_bindingList[i].StudentId == studentId)
            {
                _bindingList.RemoveAt(i);
                break;
            }
        }

        if (_selectedStudent?.StudentId == studentId)
        {
            _selectedStudent = null;
        }
    }

    private StudentInfo GetOrCreateStudent(string studentId, string fullName, string machineName)
    {
        for (var i = 0; i < _bindingList.Count; i++)
        {
            if (_bindingList[i].StudentId == studentId)
            {
                return _bindingList[i];
            }
        }

        var created = new StudentInfo
        {
            StudentId = studentId,
            FullName = fullName,
            MachineName = machineName,
            Status = "Connected",
            LastSeenUtc = DateTime.UtcNow
        };

        _bindingList.Add(created);
        return created;
    }

    private void ResetStudentRow(string studentId)
    {
        for (var i = 0; i < _bindingList.Count; i++)
        {
            if (_bindingList[i].StudentId == studentId)
            {
                _bindingList.ResetItem(i);
                break;
            }
        }
    }

    private void ShowDashboardValidation(string messageKey)
    {
        MessageBox.Show(this,
            LocalizationResources.GetString(messageKey, _currentLanguage),
            LocalizationResources.GetString("Dashboard.MsgCaptionTutor", _currentLanguage),
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    /// <summary>Requires at least one connected student and a selected grid row (for commands sent to one student).</summary>
    private bool TryRequireStudentForPerStudentCommand([NotNullWhen(true)] out StudentInfo? student)
    {
        student = null;
        if (_bindingList.Count == 0)
        {
            ShowDashboardValidation("Dashboard.MsgNoStudentsConnected");
            return false;
        }

        if (_selectedStudent == null)
        {
            ShowDashboardValidation("Dashboard.MsgSelectStudentFirst");
            return false;
        }

        student = _selectedStudent;
        return true;
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
    private async void LockStudent(object? sender, EventArgs e)
    {
        if (!TryRequireStudentForPerStudentCommand(out var student))
        {
            return;
        }

        if (!await EnsureServerRunningAsync())
        {
            return;
        }

        var command = new TutorCommand
        {
            CommandType = "Lock"
        };

        await _tutorServer.SendCommandToStudentAsync(student.StudentId, command);
    }

    private async void UnlockStudent(object? sender, EventArgs e)
    {
        if (!TryRequireStudentForPerStudentCommand(out var student))
        {
            return;
        }

        if (!await EnsureServerRunningAsync())
        {
            return;
        }

        var command = new TutorCommand
        {
            CommandType = "Unlock"
        };

        await _tutorServer.SendCommandToStudentAsync(student.StudentId, command);
    }

    private void SetupTest(object? sender, EventArgs e)
    {
        using var form = new TestSetupForm(_studentRegistry, _tutorServer, _sessionManager, _currentLanguage);
        form.ShowDialog(this);
    }

    private async void StartTest(object? sender, EventArgs e)
    {
        if (!TryRequireStudentForPerStudentCommand(out var student))
        {
            return;
        }

        if (!await EnsureServerRunningAsync())
        {
            return;
        }

        var examPath = PromptForExamPath();
        if (string.IsNullOrWhiteSpace(examPath))
        {
            return;
        }

        var exam = await JsonFileStore.LoadAsync<Exam>(examPath);
        if (exam is null || exam.Questions.Count == 0)
        {
            MessageBox.Show(this,
                LocalizationResources.GetString("Dashboard.MsgInvalidExam", _currentLanguage),
                LocalizationResources.GetString("Dashboard.MsgCaptionTest", _currentLanguage),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var durationMinutes = exam.DurationMinutes > 0 ? exam.DurationMinutes : 10;
        exam.DurationMinutes = durationMinutes;

        var session = _sessionManager.CreateSession(exam, durationMinutes, new[] { student.StudentId });

        var command = new TutorCommand
        {
            CommandType = "StartTest",
            SessionId = session.Id,
            Exam = exam,
            DurationMinutes = durationMinutes
        };

        await _tutorServer.SendCommandToStudentAsync(student.StudentId, command);
    }

    private async void StopTest(object? sender, EventArgs e)
    {
        if (!TryRequireStudentForPerStudentCommand(out var student))
        {
            return;
        }

        if (!_sessionManager.HasActiveSession)
        {
            ShowDashboardValidation("Dashboard.MsgNoActiveTest");
            return;
        }

        if (!await EnsureServerRunningAsync())
        {
            return;
        }

        var command = new TutorCommand
        {
            CommandType = "StopTest",
            SessionId = _sessionManager.ActiveSession?.Id ?? string.Empty
        };

        await _tutorServer.SendCommandToStudentAsync(student.StudentId, command);
        _sessionManager.StopSession();
    }

    private void OpenLiveTracking(object? sender, EventArgs e)
    {
        if (!_sessionManager.HasActiveSession)
        {
            ShowDashboardValidation("Dashboard.MsgNoActiveTest");
            return;
        }

        var form = new LiveTrackingForm(_studentRegistry, _sessionManager);
        form.Show(this);
    }

    private void OpenReport(object? sender, EventArgs e)
    {
        using var form = new ReportForm(_studentRegistry, _sessionManager);
        form.ShowDialog(this);
    }


    private async Task<bool> EnsureServerRunningAsync()
    {
        if (_tutorServer.IsRunning)
        {
            return true;
        }

        try
        {
            await _tutorServer.StartAsync();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                string.Format(LocalizationResources.GetString("TestSetup.MsgServerStartFailed", _currentLanguage), ex.Message),
                LocalizationResources.GetString("TestSetup.MsgServerCaption", _currentLanguage),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private string? PromptForExamPath()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Exam JSON (*.json)|*.json|All files (*.*)|*.*",
            InitialDirectory = FindInitialExamDirectory()
        };

        if (!string.IsNullOrWhiteSpace(_lastExamPath) && File.Exists(_lastExamPath))
        {
            dialog.FileName = _lastExamPath;
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        _lastExamPath = dialog.FileName;
        return dialog.FileName;
    }

    private string FindInitialExamDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_lastExamPath))
        {
            var lastDir = Path.GetDirectoryName(_lastExamPath);
            if (!string.IsNullOrWhiteSpace(lastDir) && Directory.Exists(lastDir))
            {
                return lastDir;
            }
        }

        var current = AppContext.BaseDirectory;

        for (var i = 0; i < 6; i++)
        {
            var candidate = Path.Combine(current, "samples", "exams");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        return Environment.CurrentDirectory;
    }

    // ================= Sample Data =================
    private void AddSampleStudents()
    {
        var students = new[]
        {
            new StudentInfo { FullName="Ahmed Hassan", MachineName="PC-LAB-01", Status="Active", AnsweredCount=12, Score="10/12", LastSeenUtc=DateTime.UtcNow.AddSeconds(-5)},
            new StudentInfo { FullName="Fatima Mohamed", MachineName="PC-LAB-02", Status="In Test", AnsweredCount=8, Score="6/10", LastSeenUtc=DateTime.UtcNow.AddSeconds(-30)},
            new StudentInfo { FullName="Omar Ali", MachineName="PC-LAB-03", Status="Idle", AnsweredCount=0, Score="", LastSeenUtc=DateTime.UtcNow.AddMinutes(-2)},
            new StudentInfo { FullName="Noor Khalid", MachineName="PC-LAB-04", Status="Active", AnsweredCount=20, Score="19/20", LastSeenUtc=DateTime.UtcNow.AddSeconds(-15)}
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
        {
            _tutorServer.OnStudentRegistered -= HandleStudentRegistered;
            _tutorServer.OnHeartbeatReceived -= HandleHeartbeatReceived;
            _tutorServer.OnStudentDisconnected -= HandleStudentDisconnected;
            _tutorServer.OnProgressUpdated -= HandleProgressUpdated;
            _tutorServer.OnAnswersSubmitted -= HandleAnswersSubmitted;
            _refreshTimer?.Dispose();
        }

        base.Dispose(disposing);
    }
}
