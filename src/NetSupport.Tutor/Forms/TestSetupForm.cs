using NetSupport.Shared.Contracts;
using NetSupport.Shared.Localization;
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
    private readonly AppLanguage _language;
    private readonly Label _headerLabel;
    private readonly GroupBox _studentsGroup;
    private readonly GroupBox _examGroup;
    private readonly Label _durationCaptionLabel;
    private readonly Button _closeButton;
    private readonly Label _noStudentsHintLabel;

    public TestSetupForm()
        : this(null, null, new TestSessionManager(), FindDefaultExamFolder(), AppLanguage.English)
    {
    }

    public TestSetupForm(
        StudentRegistry? studentRegistry,
        TutorServer? tutorServer,
        TestSessionManager sessionManager)
        : this(studentRegistry, tutorServer, sessionManager, FindDefaultExamFolder(), AppLanguage.English)
    {
    }

    public TestSetupForm(
        StudentRegistry? studentRegistry,
        TutorServer? tutorServer,
        TestSessionManager sessionManager,
        AppLanguage language)
        : this(studentRegistry, tutorServer, sessionManager, FindDefaultExamFolder(), language)
    {
    }

    public TestSetupForm(
        StudentRegistry? studentRegistry,
        TutorServer? tutorServer,
        TestSessionManager sessionManager,
        string examsFolder,
        AppLanguage language = AppLanguage.English)
    {
        _studentRegistry = studentRegistry;
        _tutorServer = tutorServer;
        _sessionManager = sessionManager;
        _examsFolder = examsFolder;
        _language = language;

        Text = LocalizationResources.GetString("TestSetup.Title", _language);
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

        _headerLabel = new Label
        {
            Text = LocalizationResources.GetString("TestSetup.Title", _language),
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

        _refreshStudentsButton = new Button { Text = LocalizationResources.GetString("TestSetup.Refresh", _language), AutoSize = true };
        _refreshStudentsButton.Click += (_, __) =>
        {
            LoadStudents();
            UpdateActionButtons();
        };

        _selectAllButton = new Button { Text = LocalizationResources.GetString("TestSetup.SelectAll", _language), AutoSize = true };
        _selectAllButton.Click += (_, __) =>
        {
            SetAllStudentChecks(true);
            UpdateActionButtons();
        };

        _clearSelectionButton = new Button { Text = LocalizationResources.GetString("TestSetup.Clear", _language), AutoSize = true };
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

        _noStudentsHintLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 8, 4, 4),
            ForeColor = SystemColors.GrayText,
            Visible = false
        };
        var listHost = new Panel { Dock = DockStyle.Fill };
        listHost.Controls.Add(_noStudentsHintLabel);
        _studentsList.Dock = DockStyle.Fill;
        listHost.Controls.Add(_studentsList);
        studentsLayout.Controls.Add(listHost, 0, 1);

        _studentsGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = LocalizationResources.GetString("TestSetup.StudentsGroup", _language)
        };
        _studentsGroup.Controls.Add(studentsLayout);

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

        _browseExamButton = new Button { Text = LocalizationResources.GetString("TestSetup.Browse", _language), AutoSize = true };
        _browseExamButton.Click += async (_, __) => await BrowseForExamAsync();

        _refreshExamsButton = new Button { Text = LocalizationResources.GetString("TestSetup.Reload", _language), AutoSize = true };
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
            Text = LocalizationResources.GetString("TestSetup.SelectExamHint", _language)
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
        _durationCaptionLabel = new Label
        {
            Text = LocalizationResources.GetString("TestSetup.DurationMinutes", _language),
            AutoSize = true,
            Padding = new Padding(0, 6, 4, 0)
        };
        durationLayout.Controls.Add(_durationCaptionLabel);
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

        _examGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = LocalizationResources.GetString("TestSetup.ExamGroup", _language)
        };
        _examGroup.Controls.Add(examLayout);

        contentLayout.Controls.Add(_studentsGroup, 0, 0);
        contentLayout.Controls.Add(_examGroup, 1, 0);

        _startButton = new Button
        {
            Text = LocalizationResources.GetString("TestSetup.StartTest", _language),
            Width = 120,
            Height = 36
        };
        _startButton.Click += StartTestClicked;

        _stopButton = new Button
        {
            Text = LocalizationResources.GetString("TestSetup.StopTest", _language),
            Width = 120,
            Height = 36
        };
        _stopButton.Click += StopTestClicked;

        _closeButton = new Button
        {
            Text = LocalizationResources.GetString("TestSetup.Close", _language),
            Width = 120,
            Height = 36
        };
        _closeButton.Click += (_, __) => Close();

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        actionPanel.Controls.Add(_closeButton);
        actionPanel.Controls.Add(_stopButton);
        actionPanel.Controls.Add(_startButton);

        mainLayout.Controls.Add(_headerLabel, 0, 0);
        mainLayout.Controls.Add(contentLayout, 0, 1);
        mainLayout.Controls.Add(actionPanel, 0, 2);

        Controls.Add(mainLayout);

        ApplyRtlLayout();

        Load += async (_, __) =>
        {
            LoadStudents();
            await LoadExamOptionsAsync();
            UpdateActionButtons();
        };
    }

    private void ApplyRtlLayout()
    {
        var rtl = _language == AppLanguage.Arabic;
        RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = rtl;
    }

    private void LoadStudents()
    {
        _studentsList.Items.Clear();

        var students = _studentRegistry?.Students?.ToList() ?? new List<StudentInfo>();

        _noStudentsHintLabel.Text = LocalizationResources.GetString("TestSetup.NoStudentsHint", _language);
        _noStudentsHintLabel.Visible = students.Count == 0;

        foreach (var student in students)
        {
            _studentsList.Items.Add(new StudentListItem(student), false);
        }
    }

    private async Task LoadExamOptionsAsync()
    {
        _examCombo.Items.Clear();

        if (!Directory.Exists(_examsFolder))
        {
            _examSummaryLabel.Text = LocalizationResources.GetString("TestSetup.ExamFolderNotFound", _language);
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
            _examSummaryLabel.Text = LocalizationResources.GetString("TestSetup.NoExamFiles", _language);
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
            MessageBox.Show(this,
                LocalizationResources.GetString("TestSetup.MsgExamLoadFailed", _language),
                LocalizationResources.GetString("TestSetup.MsgExamLoadCaption", _language),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            _examSummaryLabel.Text = LocalizationResources.GetString("TestSetup.SelectExamHint", _language);
            _examPathLabel.Text = string.Empty;
            return;
        }

        var exam = option.Exam;
        var questionCount = exam.Questions?.Count ?? 0;
        var title = string.IsNullOrWhiteSpace(exam.Title)
            ? Path.GetFileName(option.FilePath)
            : exam.Title;

        _examSummaryLabel.Text = string.Format(
            LocalizationResources.GetString("TestSetup.ExamSummaryFormat", _language),
            title,
            questionCount);
        _examPathLabel.Text = option.FilePath;

        var duration = Math.Clamp(exam.DurationMinutes, 1, 240);
        _durationMinutes.Value = duration;
    }

    private async void StartTestClicked(object? sender, EventArgs e)
    {
        if (_sessionManager.HasActiveSession)
        {
            MessageBox.Show(this,
                LocalizationResources.GetString("TestSetup.MsgSessionRunning", _language),
                LocalizationResources.GetString("TestSetup.MsgTestSessionCaption", _language),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var studentIds = GetSelectedStudentIds();
        if (studentIds.Count == 0)
        {
            MessageBox.Show(this,
                LocalizationResources.GetString("TestSetup.MsgSelectStudent", _language),
                LocalizationResources.GetString("TestSetup.MsgTestSetupCaption", _language),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_examCombo.SelectedItem is not ExamFileOption option)
        {
            MessageBox.Show(this,
                LocalizationResources.GetString("TestSetup.MsgSelectExam", _language),
                LocalizationResources.GetString("TestSetup.MsgTestSetupCaption", _language),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        MessageBox.Show(this,
            LocalizationResources.GetString("TestSetup.MsgStartSent", _language),
            LocalizationResources.GetString("TestSetup.MsgTestStarted", _language),
            MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            MessageBox.Show(this,
                LocalizationResources.GetString("TestSetup.MsgSelectStop", _language),
                LocalizationResources.GetString("TestSetup.MsgTestSetupCaption", _language),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        MessageBox.Show(this,
            LocalizationResources.GetString("TestSetup.MsgStopSent", _language),
            LocalizationResources.GetString("TestSetup.MsgTestStopped", _language),
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        UpdateActionButtons();
    }

    private async Task<bool> EnsureServerRunningAsync()
    {
        if (_tutorServer is null)
        {
            MessageBox.Show(this,
                LocalizationResources.GetString("TestSetup.MsgServerUnavailable", _language),
                LocalizationResources.GetString("TestSetup.MsgServerCaption", _language),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            MessageBox.Show(this,
                string.Format(LocalizationResources.GetString("TestSetup.MsgServerStartFailed", _language), ex.Message),
                LocalizationResources.GetString("TestSetup.MsgServerCaption", _language),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
