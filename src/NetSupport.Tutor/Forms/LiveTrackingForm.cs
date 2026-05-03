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
    private readonly DataGridView _gridAnswered;
    private readonly Label _lblAnsweredTitle;
    private readonly BindingList<StudentProgressDisplay> _bindingList = new();
    private readonly BindingList<AnsweredQuestionDisplay> _answeredBinding = new();
    private readonly Dictionary<string, List<StudentAnswer>> _answersByStudent = new();
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

        _gridTracking.SelectionChanged += (_, _) => RefreshAnsweredGridForSelectedStudent();

        _gridAnswered = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        _gridAnswered.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "#", DataPropertyName = "QuestionOrder", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Question", DataPropertyName = "QuestionText", FillWeight = 45 },
            new DataGridViewTextBoxColumn { HeaderText = "Selected Answer", DataPropertyName = "SelectedAnswer", FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Result", DataPropertyName = "Result", FillWeight = 13 }
        );
        _gridAnswered.RowPrePaint += GridAnswered_RowPrePaint;

        _lblAnsweredTitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(Font.FontFamily, 10f, FontStyle.Bold),
            Text = "Answered Questions"
        };

        var answeredPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        answeredPanel.Controls.Add(_gridAnswered);
        answeredPanel.Controls.Add(_lblAnsweredTitle);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            FixedPanel = FixedPanel.None,
            SplitterDistance = 240
        };
        split.Panel1.Controls.Add(_gridTracking);
        split.Panel2.Controls.Add(answeredPanel);

        Controls.Add(split);
        _gridTracking.DataSource = _bindingList;
        _gridAnswered.DataSource = _answeredBinding;

        InitializeStudents();

        _tutorServer.OnProgressUpdated += HandleProgressUpdated;
        _tutorServer.OnAnswersSubmitted += HandleAnswersSubmitted;

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
            _answersByStudent[progress.StudentId] = progress.Answers ?? new List<StudentAnswer>();

            _bindingList.ResetItem(index);
            RefreshAnsweredGridForSelectedStudent();
        }
    }

    private void HandleAnswersSubmitted(string studentId, List<StudentAnswer> answers)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HandleAnswersSubmitted(studentId, answers)));
            return;
        }

        _answersByStudent[studentId] = answers ?? new List<StudentAnswer>();
        RefreshAnsweredGridForSelectedStudent();
    }

    private void RefreshAnsweredGridForSelectedStudent()
    {
        _answeredBinding.Clear();
        if (_gridTracking.CurrentRow?.DataBoundItem is not StudentProgressDisplay student)
        {
            _lblAnsweredTitle.Text = "Answered Questions";
            return;
        }

        _lblAnsweredTitle.Text = $"Answered Questions - {student.StudentName}";
        if (!_answersByStudent.TryGetValue(student.StudentId, out var answers) || answers.Count == 0)
        {
            return;
        }

        var exam = _sessionManager.ActiveSession?.Exam;
        if (exam == null)
        {
            return;
        }

        foreach (var answer in answers)
        {
            var questionIndex = exam.Questions.FindIndex(q => q.Id == answer.QuestionId);
            if (questionIndex < 0)
            {
                continue;
            }

            var question = exam.Questions[questionIndex];
            var selected = question.Choices.FirstOrDefault(c => c.Id == answer.ChoiceId);
            var isCorrect = selected?.IsCorrect ?? false;

            _answeredBinding.Add(new AnsweredQuestionDisplay
            {
                QuestionOrder = questionIndex + 1,
                QuestionText = question.Text,
                SelectedAnswer = selected?.Text ?? "(unknown choice)",
                Result = isCorrect ? "Correct" : "Incorrect",
                IsCorrect = isCorrect
            });
        }
    }

    private void GridAnswered_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _answeredBinding.Count)
        {
            return;
        }

        var row = _gridAnswered.Rows[e.RowIndex];
        var item = _answeredBinding[e.RowIndex];
        row.DefaultCellStyle.BackColor = item.IsCorrect ? Color.FromArgb(225, 245, 229) : Color.FromArgb(255, 232, 232);
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
            _tutorServer.OnAnswersSubmitted -= HandleAnswersSubmitted;
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

    private class AnsweredQuestionDisplay
    {
        public int QuestionOrder { get; set; }
        public string QuestionText { get; set; } = "";
        public string SelectedAnswer { get; set; } = "";
        public string Result { get; set; } = "";
        public bool IsCorrect { get; set; }
    }
}
