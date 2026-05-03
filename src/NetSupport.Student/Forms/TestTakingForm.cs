namespace NetSupport.Student.Forms;

using NetSupport.Shared.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public sealed class TestTakingForm : Form
{
    private readonly Exam _exam;
    private readonly string _studentId;
    private readonly string _sessionId;
    private int _currentIndex;

    private readonly Dictionary<int, int> _answers = new();
    private readonly DateTime _examDeadlineUtc;
    private readonly System.Windows.Forms.Timer _countdownTimer = new() { Interval = 1000 };
    private readonly List<Button> _navButtons = new();

    private bool _suppressChoiceEvents;
    private bool _submitDone;

    private readonly Label _lblTimer = new();
    private readonly Label _lblProgress = new();
    private readonly Label _lblAnswered = new();
    private readonly TextBox _txtQuestion = new();
    private readonly FlowLayoutPanel _choicesPanel = new();
    private readonly Panel _navHost = new();
    private readonly Panel _navScroll = new();
    private readonly Button _btnPrev = new();
    private readonly Button _btnNext = new();
    private readonly Button _btnSubmit = new();

    public TestTakingForm(Exam exam, string studentId, string sessionId)
    {
        _exam = exam;
        _studentId = studentId;
        _sessionId = sessionId;

        var minutes = Math.Max(1, exam.DurationMinutes);
        _examDeadlineUtc = DateTime.UtcNow.AddMinutes(minutes);

        Text = "Student Test";
        Width = 960;
        Height = 640;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 480);

        BuildLayout();
        BuildQuestionNav();
        WireTimer();

        LoadQuestion();
        SaveAnswer();
        UpdateQuestionNav();
    }

    private void BuildLayout()
    {
        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 10, 12, 8),
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };

        _lblTimer.AutoSize = true;
        _lblTimer.Font = new Font(Font.FontFamily, 11f, FontStyle.Bold);
        _lblTimer.Margin = new Padding(0, 4, 24, 0);

        _lblProgress.AutoSize = true;
        _lblProgress.Margin = new Padding(0, 4, 24, 0);

        _lblAnswered.AutoSize = true;
        _lblAnswered.Margin = new Padding(0, 4, 0, 0);

        header.Controls.Add(_lblTimer);
        header.Controls.Add(_lblProgress);
        header.Controls.Add(_lblAnswered);

        _navHost.Dock = DockStyle.Fill;
        _navHost.Padding = Padding.Empty;

        _navScroll.Dock = DockStyle.Fill;
        _navScroll.AutoScroll = true;
        _navScroll.Padding = new Padding(6, 8, 6, 8);
        _navScroll.HorizontalScroll.Enabled = false;
        _navScroll.HorizontalScroll.Visible = false;
        _navScroll.AutoScrollMargin = new Size(0, 8);

        _navHost.Controls.Add(_navScroll);
        _navScroll.Resize += (_, _) => SyncNavButtonWidths();

        var centerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        centerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 148f));
        centerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        centerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));

        _txtQuestion.Dock = DockStyle.Fill;
        _txtQuestion.Multiline = true;
        _txtQuestion.ReadOnly = true;
        _txtQuestion.BorderStyle = BorderStyle.None;
        _txtQuestion.TabStop = false;
        _txtQuestion.ScrollBars = ScrollBars.Vertical;
        _txtQuestion.WordWrap = true;
        _txtQuestion.BackColor = SystemColors.Window;
        _txtQuestion.ForeColor = SystemColors.WindowText;
        _txtQuestion.Font = new Font(Font.FontFamily, 12f, FontStyle.Bold);

        _choicesPanel.Dock = DockStyle.Fill;
        _choicesPanel.FlowDirection = FlowDirection.TopDown;
        _choicesPanel.WrapContents = false;
        _choicesPanel.AutoScroll = true;
        _choicesPanel.Padding = new Padding(0, 8, 0, 8);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 8, 0, 0),
            WrapContents = false
        };

        _btnPrev.Text = "Previous";
        _btnPrev.Width = 100;
        _btnPrev.Height = 36;
        _btnPrev.Margin = new Padding(0, 0, 8, 0);
        _btnPrev.Click += (_, _) => MoveQuestion(-1);

        _btnNext.Text = "Next";
        _btnNext.Width = 100;
        _btnNext.Height = 36;
        _btnNext.Margin = new Padding(0, 0, 8, 0);
        _btnNext.Click += (_, _) => MoveQuestion(1);

        _btnSubmit.Text = "Submit";
        _btnSubmit.Width = 120;
        _btnSubmit.Height = 36;
        _btnSubmit.Click += async (_, _) => await SubmitExamAsync();

        buttonRow.Controls.Add(_btnPrev);
        buttonRow.Controls.Add(_btnNext);
        buttonRow.Controls.Add(_btnSubmit);

        centerLayout.Controls.Add(_txtQuestion, 0, 0);
        centerLayout.Controls.Add(_choicesPanel, 0, 1);
        centerLayout.Controls.Add(buttonRow, 0, 2);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118f));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        body.Controls.Add(_navHost, 0, 0);
        body.Controls.Add(centerLayout, 1, 0);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(body, 0, 1);

        Controls.Add(root);

        Shown += (_, _) =>
        {
            SyncNavButtonWidths();
            if (_navButtons.Count > 0)
            {
                _navScroll.ScrollControlIntoView(_navButtons[0]);
            }
        };

        FormClosed += (_, _) =>
        {
            _countdownTimer.Stop();
            _countdownTimer.Dispose();
        };
    }

    private void BuildQuestionNav()
    {
        const int btnHeight = 36;
        const int gap = 6;

        _navScroll.SuspendLayout();
        try
        {
            _navScroll.Controls.Clear();
            _navButtons.Clear();

            var n = _exam.Questions.Count;
            var padL = _navScroll.Padding.Left;
            var padT = _navScroll.Padding.Top;
            var y = padT;

            for (var i = 0; i < n; i++)
            {
                var idx = i;
                var btn = new Button
                {
                    Text = $"{i + 1}",
                    Tag = idx,
                    Height = btnHeight,
                    Location = new Point(padL, y),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter,
                    TabStop = true
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (_, _) => NavigateToQuestion(idx);
                _navScroll.Controls.Add(btn);
                _navButtons.Add(btn);
                y += btnHeight + gap;
            }

            SyncNavButtonWidths();
        }
        finally
        {
            _navScroll.ResumeLayout(true);
        }
    }

    private void SyncNavButtonWidths()
    {
        var padL = _navScroll.Padding.Left;
        var padR = _navScroll.Padding.Right;
        var w = _navScroll.ClientSize.Width - padL - padR;
        if (w < 48)
        {
            w = 48;
        }

        foreach (var btn in _navButtons)
        {
            btn.Left = padL;
            btn.Width = w;
        }
    }

    private void WireTimer()
    {
        _countdownTimer.Tick += CountdownTimer_Tick;
        _countdownTimer.Start();
        CountdownTimer_Tick(null, EventArgs.Empty);
    }

    private void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        if (_submitDone)
        {
            return;
        }

        var remaining = _examDeadlineUtc - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            _lblTimer.Text = "Time left: 00:00";
            _lblTimer.ForeColor = Color.DarkRed;
            _countdownTimer.Stop();
            _ = SubmitExamAsync(showTimeUpMessage: true);
            return;
        }

        _lblTimer.ForeColor = remaining.TotalMinutes <= 1 ? Color.DarkRed : Color.Black;
        _lblTimer.Text = $"Time left: {(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";

        _ = Services.TestAnswerService.SendProgressAsync(
            _studentId,
            _sessionId,
            _answers.Count,
            _exam.Questions.Count,
            "Testing",
            (int)remaining.TotalSeconds);
    }

    private void NavigateToQuestion(int index)
    {
        if (index < 0 || index >= _exam.Questions.Count)
        {
            return;
        }

        SaveAnswer();
        _currentIndex = index;
        LoadQuestion();
        UpdateQuestionNav();
    }

    private void MoveQuestion(int delta)
    {
        SaveAnswer();
        var next = _currentIndex + delta;
        if (next >= 0 && next < _exam.Questions.Count)
        {
            _currentIndex = next;
            LoadQuestion();
            UpdateQuestionNav();
        }
    }

    private void LoadQuestion()
    {
        var q = _exam.Questions[_currentIndex];

        _txtQuestion.Text = q.Text;

        _suppressChoiceEvents = true;
        _choicesPanel.Controls.Clear();
        try
        {
            for (var i = 0; i < q.Choices.Count; i++)
            {
                var choiceIndex = i;
                var rb = new RadioButton
                {
                    Text = q.Choices[i].Text,
                    AutoSize = true,
                    Margin = new Padding(0, 6, 0, 6),
                    MaximumSize = new Size(4096, 0),
                    UseMnemonic = false,
                    FlatStyle = FlatStyle.System
                };
                rb.CheckedChanged += (_, _) => OnChoiceCheckedChanged();
                _choicesPanel.Controls.Add(rb);

                if (_answers.TryGetValue(_currentIndex, out var saved) && saved == choiceIndex)
                {
                    rb.Checked = true;
                }
            }
        }
        finally
        {
            _suppressChoiceEvents = false;
        }

        _lblProgress.Text = $"Question {_currentIndex + 1} / {_exam.Questions.Count}";

        var isLast = _currentIndex >= _exam.Questions.Count - 1;
        _btnNext.Visible = !isLast;
        _btnPrev.Enabled = _currentIndex > 0;

        UpdateAnsweredLabel();
    }

    private void OnChoiceCheckedChanged()
    {
        if (_suppressChoiceEvents)
        {
            return;
        }

        SaveAnswer();
        UpdateQuestionNav();
    }

    private void SaveAnswer()
    {
        var q = _exam.Questions[_currentIndex];
        for (var i = 0; i < _choicesPanel.Controls.Count && i < q.Choices.Count; i++)
        {
            if (_choicesPanel.Controls[i] is RadioButton { Checked: true })
            {
                _answers[_currentIndex] = i;
                UpdateAnsweredLabel();
                _ = Services.TestAnswerService.SendProgressAsync(
                    _studentId,
                    _sessionId,
                    _answers.Count,
                    _exam.Questions.Count,
                    "Testing",
                    GetRemainingSeconds());
                return;
            }
        }
    }

    private int GetRemainingSeconds()
    {
        var remaining = _examDeadlineUtc - DateTime.UtcNow;
        return remaining <= TimeSpan.Zero ? 0 : (int)remaining.TotalSeconds;
    }

    private void UpdateAnsweredLabel()
    {
        _lblAnswered.Text = $"Answered: {_answers.Count} / {_exam.Questions.Count}";
    }

    private void UpdateQuestionNav()
    {
        for (var i = 0; i < _navButtons.Count; i++)
        {
            var btn = _navButtons[i];
            if (i == _currentIndex)
            {
                btn.BackColor = Color.LightSkyBlue;
                btn.FlatAppearance.BorderColor = Color.SteelBlue;
            }
            else if (_answers.ContainsKey(i))
            {
                btn.BackColor = Color.PaleGreen;
                btn.FlatAppearance.BorderColor = Color.ForestGreen;
            }
            else
            {
                btn.BackColor = SystemColors.Control;
                btn.FlatAppearance.BorderColor = SystemColors.ControlDark;
            }
        }
    }

    public void SubmitExam()
    {
        _ = SubmitExamAsync();
    }

    public async Task SubmitExamAsync(bool showTimeUpMessage = false)
    {
        if (_submitDone)
        {
            return;
        }

        _submitDone = true;

        SaveAnswer();
        _countdownTimer.Stop();

        var answers = _answers.Select(entry => new StudentAnswer
        {
            StudentId = _studentId,
            SessionId = _sessionId,
            QuestionId = _exam.Questions[entry.Key].Id,
            ChoiceId = _exam.Questions[entry.Key].Choices[entry.Value].Id,
            AnsweredAtUtc = DateTime.UtcNow
        }).ToList();

        await Services.TestAnswerService.SubmitAsync(_studentId, _sessionId, answers);

        if (showTimeUpMessage)
        {
            MessageBox.Show("Time is up. Your answers were submitted.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Exam submitted!", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        Close();
    }
}
