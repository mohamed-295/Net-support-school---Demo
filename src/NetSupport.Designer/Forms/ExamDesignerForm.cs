using NetSupport.Shared.Models;
using NetSupport.Shared.Localization;
using NetSupport.Designer.Services;
using System.Text.Json;

namespace NetSupport.Designer.Forms;

public sealed class ExamDesignerForm : Form
{
    private TextBox txtTitle = null!;
    private NumericUpDown numDuration = null!;
    private TextBox txtQuestion = null!;
    private TextBox[] txtChoices = null!;
    private ComboBox cmbCorrect = null!;
    private ListBox lstQuestions = null!;

    private List<Question> questions = new();
    private readonly ExamDesignerService _service = new();

    private const string ExamsFolder = "samples/exams";

    public ExamDesignerForm()
    {
        Text = "Exam Designer";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        InitializeUI();
    }

    private void InitializeUI()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };

        // ===== Top Panel =====
        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Height = 300
        };

        txtTitle = new TextBox { Width = 300, PlaceholderText = "Exam Title" };
        numDuration = new NumericUpDown { Width = 100, Minimum = 1, Maximum = 180, Value = 10 };

        txtQuestion = new TextBox { Width = 400, PlaceholderText = "Question" };

        txtChoices = new TextBox[4];
        for (int i = 0; i < 4; i++)
        {
            txtChoices[i] = new TextBox
            {
                Width = 300,
                PlaceholderText = $"Choice {i + 1}"
            };
            topPanel.Controls.Add(txtChoices[i]);
        }

        cmbCorrect = new ComboBox { Width = 100 };
        cmbCorrect.Items.AddRange(new[] { "1", "2", "3", "4" });

        var btnAdd = new Button { Text = "Add Question", Width = 150 };
        btnAdd.Click += AddQuestion;

        topPanel.Controls.Add(new Label { Text = "Title" });
        topPanel.Controls.Add(txtTitle);

        topPanel.Controls.Add(new Label { Text = "Duration (minutes)" });
        topPanel.Controls.Add(numDuration);

        topPanel.Controls.Add(new Label { Text = "Question" });
        topPanel.Controls.Add(txtQuestion);

        topPanel.Controls.Add(new Label { Text = "Correct Answer (1-4)" });
        topPanel.Controls.Add(cmbCorrect);

        topPanel.Controls.Add(btnAdd);

        // ===== Bottom Panel =====
        var bottomPanel = new Panel { Dock = DockStyle.Fill };

        lstQuestions = new ListBox { Dock = DockStyle.Fill };

        var btnSave = new Button { Text = "Save Exam", Dock = DockStyle.Bottom };
        btnSave.Click += SaveExam;

        var btnLoad = new Button { Text = "Load Exam", Dock = DockStyle.Bottom };
        btnLoad.Click += LoadExam;

        bottomPanel.Controls.Add(lstQuestions);
        bottomPanel.Controls.Add(btnSave);
        bottomPanel.Controls.Add(btnLoad);

        main.Controls.Add(topPanel, 0, 0);
        main.Controls.Add(bottomPanel, 0, 1);

        Controls.Add(main);

    }

    private void AddQuestion(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtQuestion.Text))
        {
            MessageBox.Show("Enter question text");
            return;
        }

        if (cmbCorrect.SelectedIndex < 0)
        {
            MessageBox.Show("Select correct answer");
            return;
        }

        var choices = new List<Choice>();

        for (int i = 0; i < 4; i++)
        {
            if (string.IsNullOrWhiteSpace(txtChoices[i].Text))
            {
                MessageBox.Show($"Choice {i + 1} is empty");
                return;
            }

            choices.Add(new Choice
            {
                Text = txtChoices[i].Text,
                IsCorrect = (i == cmbCorrect.SelectedIndex)
            });
        }

        var question = new Question
        {
            Text = txtQuestion.Text,
            Choices = choices
        };

        questions.Add(question);
        lstQuestions.Items.Add(txtQuestion.Text);

        // Clear inputs
        txtQuestion.Clear();
        foreach (var c in txtChoices) c.Clear();
        cmbCorrect.SelectedIndex = -1;
    }

    private async void SaveExam(object? sender, EventArgs e)
    {
        if (!_service.IsValidExam(txtTitle.Text, questions))
        {
            MessageBox.Show("Enter title and add questions");
            return;
        }

        var exam = new Exam
        {
            Title = txtTitle.Text,
            DurationMinutes = (int)numDuration.Value,
            Questions = questions
        };

        var path = _service.BuildPath(ExamsFolder, txtTitle.Text);

        await _service.SaveExamAsync(path, exam);

        MessageBox.Show("Exam saved!");
    }

    private async void LoadExam(object? sender, EventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = ExamsFolder,
            Filter = "JSON Files (*.json)|*.json"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        var exam = await _service.LoadExamAsync(dialog.FileName);

        if (exam == null)
        {
            MessageBox.Show("Failed to load exam");
            return;
        }

        txtTitle.Text = exam.Title;
        numDuration.Value = exam.DurationMinutes;

        questions = exam.Questions ?? new();

        lstQuestions.Items.Clear();
        foreach (var q in questions)
            lstQuestions.Items.Add(q.Text);
    }
}