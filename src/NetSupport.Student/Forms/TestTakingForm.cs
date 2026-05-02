namespace NetSupport.Student.Forms;

using NetSupport.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class TestTakingForm : Form
{
    private readonly Exam _exam;
    private readonly string _studentId;
    private readonly string _sessionId;
    private int _currentIndex = 0;

    private Dictionary<int, int> _answers = new();

    // UI Controls
    private Label lblQuestion = new();
    private Label lblProgress = new();
    private Label lblAnswered = new();

    private RadioButton radio1 = new();
    private RadioButton radio2 = new();
    private RadioButton radio3 = new();
    private RadioButton radio4 = new();

    private Button btnNext = new();
    private Button btnPrev = new();
    private Button btnSubmit = new();

    public TestTakingForm(Exam exam, string studentId, string sessionId)
    {
        _exam = exam;
        _studentId = studentId;
        _sessionId = sessionId;

        Text = "Student Test";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        InitializeUI();
        LoadQuestion();
        UpdateAnsweredCount();
    }

    private void InitializeUI()
    {
        lblQuestion.SetBounds(50, 30, 800, 60);
        lblQuestion.Font = new Font("Arial", 14, FontStyle.Bold);

        radio1.SetBounds(50, 120, 700, 30);
        radio2.SetBounds(50, 160, 700, 30);
        radio3.SetBounds(50, 200, 700, 30);
        radio4.SetBounds(50, 240, 700, 30);

        lblProgress.SetBounds(50, 300, 200, 30);
        lblAnswered.SetBounds(300, 300, 200, 30);

        btnPrev.Text = "Previous";
        btnPrev.SetBounds(200, 400, 100, 40);
        btnPrev.Click += btnPrev_Click;

        btnNext.Text = "Next";
        btnNext.SetBounds(320, 400, 100, 40);
        btnNext.Click += btnNext_Click;

        btnSubmit.Text = "Submit";
        btnSubmit.SetBounds(440, 400, 120, 40);
        btnSubmit.Click += btnSubmit_Click;

        Controls.AddRange(new Control[]
        {
            lblQuestion,
            radio1, radio2, radio3, radio4,
            lblProgress, lblAnswered,
            btnPrev, btnNext, btnSubmit
        });
    }

    private void LoadQuestion()
    {
        var q = _exam.Questions[_currentIndex];

        lblQuestion.Text = q.Text;

        radio1.Text = q.Choices[0].Text;
        radio2.Text = q.Choices[1].Text;
        radio3.Text = q.Choices[2].Text;
        radio4.Text = q.Choices[3].Text;

        foreach (var r in new[] { radio1, radio2, radio3, radio4 })
            r.Checked = false;

        if (_answers.ContainsKey(_currentIndex))
        {
            GetRadio(_answers[_currentIndex]).Checked = true;
        }

        lblProgress.Text = $"Q {_currentIndex + 1} / {_exam.Questions.Count}";
    }

    private RadioButton GetRadio(int i)
    {
        return i switch
        {
            0 => radio1,
            1 => radio2,
            2 => radio3,
            3 => radio4,
            _ => null
        };
    }

    private void SaveAnswer()
    {
        if (radio1.Checked) _answers[_currentIndex] = 0;
        else if (radio2.Checked) _answers[_currentIndex] = 1;
        else if (radio3.Checked) _answers[_currentIndex] = 2;
        else if (radio4.Checked) _answers[_currentIndex] = 3;

        UpdateAnsweredCount();

        // send progress
        _ = Services.TestAnswerService.SendProgressAsync(_studentId, _sessionId, _answers.Count, _exam.Questions.Count);
    }

    private void UpdateAnsweredCount()
    {
        lblAnswered.Text = $"Answered: {_answers.Count}";
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
        SaveAnswer();

        if (_currentIndex < _exam.Questions.Count - 1)
        {
            _currentIndex++;
            LoadQuestion();
        }
    }

    private void btnPrev_Click(object sender, EventArgs e)
    {
        SaveAnswer();

        if (_currentIndex > 0)
        {
            _currentIndex--;
            LoadQuestion();
        }
    }

    private async void btnSubmit_Click(object sender, EventArgs e)
    {
        SaveAnswer();
        await SubmitExamAsync();
    }

    public void SubmitExam()
    {
        _ = SubmitExamAsync();
    }

    public async Task SubmitExamAsync()
    {
        SaveAnswer();
        var answers = _answers.Select(entry => new StudentAnswer
        {
            StudentId = _studentId,
            SessionId = _sessionId,
            QuestionId = _exam.Questions[entry.Key].Id,
            ChoiceId = _exam.Questions[entry.Key].Choices[entry.Value].Id,
            AnsweredAtUtc = DateTime.UtcNow
        }).ToList();

        await Services.TestAnswerService.SubmitAsync(_studentId, _sessionId, answers);
        MessageBox.Show("Exam submitted!");
        Close();
    }
}