using NetSupport.Shared.Contracts;
using NetSupport.Shared.Models;
using NetSupport.Shared.Storage;
using NetSupport.Tutor.Server;
using NetSupport.Tutor.Services;

namespace NetSupport.Tutor.Forms;

public sealed class TestSetupForm : Form
{
    private readonly StudentRegistry? _studentRegistry;
    private readonly TutorServer? _tutorServer;
    private readonly TestSessionManager _sessionManager;
    private readonly string _examsFolder;

    private readonly CheckedListBox _studentsList;
    private readonly ComboBox _examCombo;
    private readonly Label _examSummaryLabel;
    private readonly Label _examPathLabel;
    private readonly NumericUpDown _durationMinutes;
    private readonly Button _startButton;
    private readonly Button _stopButton;
    private readonly Button _refreshStudentsButton;
    private readonly Button _selectAllButton;
    private readonly Button _clearSelectionButton;
    private readonly Button _browseExamButton;
    private readonly Button _refreshExamsButton;

    public TestSetupForm()
        : this(null, null, new TestSessionManager(), FindDefaultExamFolder())
    {
    }

    public TestSetupForm(
        StudentRegistry? studentRegistry,
        TutorServer? tutorServer,
        TestSessionManager sessionManager)
        : this(studentRegistry, tutorServer, sessionManager, FindDefaultExamFolder())
    {
    }

    public TestSetupForm(
        StudentRegistry? studentRegistry,
        TutorServer? tutorServer,
        TestSessionManager sessionManager,
        string examsFolder)
    {
        _studentRegistry = studentRegistry;
        _tutorServer = tutorServer;
        _sessionManager = sessionManager;
        _examsFolder = examsFolder;

        Text = "Test Setup";
        Width = 980;
        Height = 640;
        StartPosition = FormStartPosition.CenterParent;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(12),
            RowStyles =
            {
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.Absolute, 60)
            }
        };

        var header = new Label
        {
            Text = "Test Setup",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 2,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 60),
                new ColumnStyle(SizeType.Percent, 40)
            }
        };

        _studentsList = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            DisplayMember = nameof(StudentListItem.DisplayName)
        };
        _studentsList.ItemCheck += (_, __) => BeginInvoke(UpdateActionButtons);

        _refreshStudentsButton = new Button { Text = "Refresh", AutoSize = true };
        _refreshStudentsButton.Click += (_, __) =>
        {
            LoadStudents();
            UpdateActionButtons();
        };

        _selectAllButton = new Button { Text = "Select All", AutoSize = true };
        _selectAllButton.Click += (_, __) =>
        {
            SetAllStudentChecks(true);
            UpdateActionButtons();
        };

        _clearSelectionButton = new Button { Text = "Clear", AutoSize = true };
        _clearSelectionButton.Click += (_, __) =>
        {
            SetAllStudentChecks(false);
            UpdateActionButtons();
        };

        var studentActionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        studentActionPanel.Controls.AddRange(new Control[]
        {
            _refreshStudentsButton,
            _selectAllButton,
            _clearSelectionButton
        });

        var studentsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.Absolute, 36),
                new RowStyle(SizeType.Percent, 100)
            }
        };
        studentsLayout.Controls.Add(studentActionPanel, 0, 0);
        studentsLayout.Controls.Add(_studentsList, 0, 1);

        var studentsGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Students"
        };
        studentsGroup.Controls.Add(studentsLayout);

        _examCombo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(ExamFileOption.DisplayName)
        };
        _examCombo.SelectedIndexChanged += (_, __) =>
        {
            UpdateExamDetails();
            UpdateActionButtons();
        };

        _browseExamButton = new Button { Text = "Browse...", AutoSize = true };
        _browseExamButton.Click += async (_, __) => await BrowseForExamAsync();

        _refreshExamsButton = new Button { Text = "Reload", AutoSize = true };
        _refreshExamsButton.Click += async (_, __) => await LoadExamOptionsAsync();

        var examSelectorLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 60),
                new ColumnStyle(SizeType.Percent, 20),
                new ColumnStyle(SizeType.Percent, 20)
            }
        };
        examSelectorLayout.Controls.Add(_examCombo, 0, 0);
        examSelectorLayout.Controls.Add(_browseExamButton, 1, 0);
        examSelectorLayout.Controls.Add(_refreshExamsButton, 2, 0);

        _examSummaryLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Select an exam to see details."
        };

        _examPathLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ForeColor = SystemColors.GrayText
        };

        _durationMinutes = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 240,
            Value = 10,
            Dock = DockStyle.Left,
            Width = 100
        };
        _durationMinutes.ValueChanged += (_, __) => UpdateActionButtons();

        var durationLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        durationLayout.Controls.Add(new Label
        {
            Text = "Duration (minutes):",
            AutoSize = true,
            Padding = new Padding(0, 6, 4, 0)
        });
        durationLayout.Controls.Add(_durationMinutes);

        var examLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Percent, 100)
            }
        };
        examLayout.Controls.Add(examSelectorLayout, 0, 0);
        examLayout.Controls.Add(_examSummaryLabel, 0, 1);
        examLayout.Controls.Add(_examPathLabel, 0, 2);
        examLayout.Controls.Add(durationLayout, 0, 3);

        var examGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Exam & Timing"
        };
        examGroup.Controls.Add(examLayout);

        contentLayout.Controls.Add(studentsGroup, 0, 0);
        contentLayout.Controls.Add(examGroup, 1, 0);

        _startButton = new Button
        {
            Text = "Start Test",
            Width = 120,
            Height = 36
        };
        _startButton.Click += StartTestClicked;

        _stopButton = new Button
        {
            Text = "Stop Test",
            Width = 120,
            Height = 36
        };
        _stopButton.Click += StopTestClicked;

        var closeButton = new Button
        {
            Text = "Close",
            Width = 120,
            Height = 36
        };
        closeButton.Click += (_, __) => Close();

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        actionPanel.Controls.Add(closeButton);
        actionPanel.Controls.Add(_stopButton);
        actionPanel.Controls.Add(_startButton);

        mainLayout.Controls.Add(header, 0, 0);
        mainLayout.Controls.Add(contentLayout, 0, 1);
        mainLayout.Controls.Add(actionPanel, 0, 2);

        Controls.Add(mainLayout);

        Load += async (_, __) =>
        {
            LoadStudents();
            await LoadExamOptionsAsync();
            UpdateActionButtons();
        };
    }

    private void LoadStudents()
    {
        _studentsList.Items.Clear();

        var students = _studentRegistry?.Students?.ToList() ?? new List<StudentInfo>();
        if (students.Count == 0)
        {
            students = CreateSampleStudents();
        }

        foreach (var student in students)
        {
            _studentsList.Items.Add(new StudentListItem(student), false);
        }
    }

    private static List<StudentInfo> CreateSampleStudents()
    {
        return new List<StudentInfo>
        {
            new StudentInfo { StudentId = "ST-001", FullName = "Ahmed Hassan", MachineName = "PC-LAB-01", Status = "Connected" },
            new StudentInfo { StudentId = "ST-002", FullName = "Fatima Mohamed", MachineName = "PC-LAB-02", Status = "Connected" },
            new StudentInfo { StudentId = "ST-003", FullName = "Omar Ali", MachineName = "PC-LAB-03", Status = "Connected" }
        };
    }

    private async Task LoadExamOptionsAsync()
    {
        _examCombo.Items.Clear();

        if (!Directory.Exists(_examsFolder))
        {
            _examSummaryLabel.Text = "Exam folder not found.";
            _examPathLabel.Text = _examsFolder;
            return;
        }

        var examFiles = Directory.GetFiles(_examsFolder, "*.json")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var file in examFiles)
        {
            var exam = await JsonFileStore.LoadAsync<Exam>(file);
            if (exam is null)
            {
                continue;
            }

            _examCombo.Items.Add(new ExamFileOption(file, exam));
        }

        if (_examCombo.Items.Count > 0)
        {
            _examCombo.SelectedIndex = 0;
        }
        else
        {
            _examSummaryLabel.Text = "No exam files found.";
            _examPathLabel.Text = _examsFolder;
        }
    }

    private async Task BrowseForExamAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Exam JSON (*.json)|*.json|All files (*.*)|*.*",
            InitialDirectory = Directory.Exists(_examsFolder)
                ? _examsFolder
                : Environment.CurrentDirectory
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var exam = await JsonFileStore.LoadAsync<Exam>(dialog.FileName);
        if (exam is null)
        {
            MessageBox.Show(this, "Could not load the selected exam file.", "Exam Load", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var option = new ExamFileOption(dialog.FileName, exam);
        _examCombo.Items.Add(option);
        _examCombo.SelectedItem = option;
    }

    private void UpdateExamDetails()
    {
        if (_examCombo.SelectedItem is not ExamFileOption option)
        {
            _examSummaryLabel.Text = "Select an exam to see details.";
            _examPathLabel.Text = string.Empty;
            return;
        }

        var exam = option.Exam;
        var questionCount = exam.Questions?.Count ?? 0;
        var title = string.IsNullOrWhiteSpace(exam.Title)
            ? Path.GetFileName(option.FilePath)
            : exam.Title;

        _examSummaryLabel.Text = $"{title} (Questions: {questionCount})";
        _examPathLabel.Text = option.FilePath;

        var duration = Math.Clamp(exam.DurationMinutes, 1, 240);
        _durationMinutes.Value = duration;
    }

    private async void StartTestClicked(object? sender, EventArgs e)
    {
        if (_sessionManager.HasActiveSession)
        {
            MessageBox.Show(this, "A test session is already running.", "Test Session", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var studentIds = GetSelectedStudentIds();
        if (studentIds.Count == 0)
        {
            MessageBox.Show(this, "Select at least one student.", "Test Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_examCombo.SelectedItem is not ExamFileOption option)
        {
            MessageBox.Show(this, "Select an exam before starting.", "Test Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!await EnsureServerRunningAsync())
        {
            return;
        }

        var durationMinutes = (int)_durationMinutes.Value;
        option.Exam.DurationMinutes = durationMinutes;

        var session = _sessionManager.CreateSession(option.Exam, durationMinutes, studentIds);
        var command = new TutorCommand
        {
            CommandType = "StartTest",
            SessionId = session.Id,
            Exam = option.Exam,
            DurationMinutes = durationMinutes
        };

        await SendCommandToStudentsAsync(studentIds, command);

        MessageBox.Show(this, "Start command sent to selected students.", "Test Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
        UpdateActionButtons();
    }

    private async void StopTestClicked(object? sender, EventArgs e)
    {
        var selectedIds = GetSelectedStudentIds();
        var studentIds = selectedIds.Count > 0
            ? selectedIds
            : _sessionManager.ActiveSession?.StudentIds
                ?? new List<string>();

        if (studentIds.Count == 0)
        {
            MessageBox.Show(this, "Select at least one student to stop.", "Test Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        await SendCommandToStudentsAsync(studentIds, command);
        _sessionManager.StopSession();

        MessageBox.Show(this, "Stop command sent.", "Test Stopped", MessageBoxButtons.OK, MessageBoxIcon.Information);
        UpdateActionButtons();
    }

    private async Task<bool> EnsureServerRunningAsync()
    {
        if (_tutorServer is null)
        {
            MessageBox.Show(this, "Tutor server is not available.", "Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

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
            MessageBox.Show(this, $"Failed to start the tutor server: {ex.Message}", "Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private async Task SendCommandToStudentsAsync(IEnumerable<string> studentIds, TutorCommand command)
    {
        if (_tutorServer is null)
        {
            return;
        }

        foreach (var studentId in studentIds)
        {
            await _tutorServer.SendCommandToStudentAsync(studentId, command);
        }
    }

    private List<string> GetSelectedStudentIds()
    {
        var ids = new List<string>();

        foreach (var item in _studentsList.CheckedItems)
        {
            if (item is StudentListItem studentItem)
            {
                ids.Add(studentItem.Student.StudentId);
            }
        }

        return ids;
    }

    private void SetAllStudentChecks(bool isChecked)
    {
        for (var i = 0; i < _studentsList.Items.Count; i++)
        {
            _studentsList.SetItemChecked(i, isChecked);
        }
    }

    private void UpdateActionButtons()
    {
        var hasStudents = GetSelectedStudentIds().Count > 0;
        var hasExam = _examCombo.SelectedItem is ExamFileOption;
        var hasActive = _sessionManager.HasActiveSession;

        _startButton.Enabled = hasStudents && hasExam && !hasActive;
        _stopButton.Enabled = hasActive;
    }

    private static string FindDefaultExamFolder()
    {
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

        return Path.Combine(Environment.CurrentDirectory, "samples", "exams");
    }

    private sealed class StudentListItem
    {
        public StudentListItem(StudentInfo student)
        {
            Student = student;
        }

        public StudentInfo Student { get; }

        public string DisplayName =>
            string.IsNullOrWhiteSpace(Student.FullName)
                ? Student.StudentId
                : $"{Student.FullName} ({Student.StudentId})";
    }

    private sealed class ExamFileOption
    {
        public ExamFileOption(string filePath, Exam exam)
        {
            FilePath = filePath;
            Exam = exam;
        }

        public string FilePath { get; }
        public Exam Exam { get; }

        public string DisplayName =>
            string.IsNullOrWhiteSpace(Exam.Title)
                ? Path.GetFileName(FilePath)
                : Exam.Title;
    }
}
