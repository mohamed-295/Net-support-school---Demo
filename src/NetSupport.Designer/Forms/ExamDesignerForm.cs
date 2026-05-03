using NetSupport.Designer.Services;
using NetSupport.Shared.Localization;
using NetSupport.Shared.Models;

namespace NetSupport.Designer.Forms;

public sealed class ExamDesignerForm : Form
{
    private readonly ExamDesignerService _service = new();
    private readonly List<Question> _questions = new();

    private AppLanguage _currentLanguage = AppLanguage.English;

    private const string ExamsFolder = "samples/exams";

    private Label _titleLabel = null!;
    private Label _durationLabel = null!;
    private Label _questionLabel = null!;
    private Label _correctLabel = null!;
    private GroupBox _examGroup = null!;
    private GroupBox _questionGroup = null!;
    private GroupBox _listGroup = null!;
    private Button _langToggleBtn = null!;

    private TextBox _txtTitle = null!;
    private NumericUpDown _numDuration = null!;
    private TextBox _txtQuestion = null!;
    private TextBox[] _txtChoices = null!;
    private ComboBox _cmbCorrect = null!;

    private ListView _lstQuestions = null!;
    private Button _btnAddQuestion = null!;
    private Button _btnSaveExam = null!;
    private Button _btnLoadExam = null!;
    private Button _btnEditQuestion = null!;
    private Button _btnDeleteQuestion = null!;

    public ExamDesignerForm()
    {
        Text = Local("Designer.Title");
        Width = 1100;
        Height = 740;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 620);

        BuildUi();
        ApplyLocalization();
    }

    private string Local(string key) => LocalizationResources.GetString(key, _currentLanguage);

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };
        _langToggleBtn = new Button { Width = 90, Height = 30, Margin = new Padding(0) };
        _langToggleBtn.Click += (_, _) => ToggleLanguage();
        header.Controls.Add(_langToggleBtn);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 0, 8, 0)
        };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _examGroup = new GroupBox { Dock = DockStyle.Fill };
        _questionGroup = new GroupBox { Dock = DockStyle.Fill };
        _listGroup = new GroupBox { Dock = DockStyle.Fill, Padding = new Padding(10) };

        left.Controls.Add(_examGroup, 0, 0);
        left.Controls.Add(_questionGroup, 0, 1);

        body.Controls.Add(left, 0, 0);
        body.Controls.Add(_listGroup, 1, 0);

        BuildExamDetailsUi();
        BuildQuestionBuilderUi();
        BuildQuestionListUi();

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(body, 0, 1);
        Controls.Add(root);
    }

    private void BuildExamDetailsUi()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(8)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        _titleLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        _durationLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

        _txtTitle = new TextBox { Dock = DockStyle.Fill };
        _numDuration = new NumericUpDown
        {
            Dock = DockStyle.Left,
            Width = 120,
            Minimum = 1,
            Maximum = 180,
            Value = 10
        };

        grid.Controls.Add(_titleLabel, 0, 0);
        grid.Controls.Add(_txtTitle, 1, 0);
        grid.Controls.Add(_durationLabel, 0, 1);
        grid.Controls.Add(_numDuration, 1, 1);
        _examGroup.Controls.Add(grid);
    }

    private void BuildQuestionBuilderUi()
    {
        var builder = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(8)
        };
        builder.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        builder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < 7; i++)
        {
            builder.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        }
        builder.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _questionLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        _correctLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

        _txtQuestion = new TextBox { Dock = DockStyle.Fill };

        _txtChoices = new TextBox[4];
        for (var i = 0; i < 4; i++)
        {
            _txtChoices[i] = new TextBox { Dock = DockStyle.Fill };
        }

        _cmbCorrect = new ComboBox
        {
            Dock = DockStyle.Left,
            Width = 140,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbCorrect.Items.AddRange(new object[] { "1", "2", "3", "4" });

        _btnAddQuestion = new Button { Width = 160, Height = 34, Anchor = AnchorStyles.Left };
        _btnAddQuestion.Click += AddQuestion;

        builder.Controls.Add(_questionLabel, 0, 0);
        builder.Controls.Add(_txtQuestion, 1, 0);

        for (var i = 0; i < 4; i++)
        {
            builder.Controls.Add(new Label { Text = $"Choice {i + 1}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Tag = $"ChoiceLabel.{i + 1}" }, 0, i + 1);
            builder.Controls.Add(_txtChoices[i], 1, i + 1);
        }

        builder.Controls.Add(_correctLabel, 0, 5);
        builder.Controls.Add(_cmbCorrect, 1, 5);
        builder.Controls.Add(_btnAddQuestion, 1, 6);

        _questionGroup.Controls.Add(builder);
    }

    private void BuildQuestionListUi()
    {
        var listLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        listLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        listLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        _lstQuestions = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false
        };
        _lstQuestions.Columns.Add("Question", -2);
        _lstQuestions.DoubleClick += EditQuestion;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };

        _btnSaveExam = new Button { Width = 110, Height = 34 };
        _btnLoadExam = new Button { Width = 110, Height = 34 };
        _btnEditQuestion = new Button { Width = 110, Height = 34 };
        _btnDeleteQuestion = new Button { Width = 110, Height = 34 };

        _btnSaveExam.Click += SaveExam;
        _btnLoadExam.Click += LoadExam;
        _btnEditQuestion.Click += EditQuestion;
        _btnDeleteQuestion.Click += DeleteQuestion;

        buttons.Controls.Add(_btnSaveExam);
        buttons.Controls.Add(_btnLoadExam);
        buttons.Controls.Add(_btnEditQuestion);
        buttons.Controls.Add(_btnDeleteQuestion);

        listLayout.Controls.Add(_lstQuestions, 0, 0);
        listLayout.Controls.Add(buttons, 0, 1);
        _listGroup.Controls.Add(listLayout);
    }

    private void ApplyLocalization()
    {
        Text = Local("Designer.Title");
        _examGroup.Text = Local("Designer.GroupExam");
        _questionGroup.Text = Local("Designer.GroupQuestion");
        _listGroup.Text = Local("Designer.GroupList");

        _titleLabel.Text = Local("Designer.LabelTitle");
        _durationLabel.Text = Local("Designer.LabelDuration");
        _questionLabel.Text = Local("Designer.LabelQuestion");
        _correctLabel.Text = Local("Designer.LabelCorrect");

        _txtTitle.PlaceholderText = Local("Designer.PlaceholderTitle");
        _txtQuestion.PlaceholderText = Local("Designer.PlaceholderQuestion");
        for (var i = 0; i < _txtChoices.Length; i++)
        {
            _txtChoices[i].PlaceholderText = string.Format(Local("Designer.PlaceholderChoice"), i + 1);
        }

        foreach (Control c in _questionGroup.Controls[0].Controls)
        {
            if (c is Label lbl && lbl.Tag is string tag && tag.StartsWith("ChoiceLabel."))
            {
                var num = tag.Split('.')[1];
                lbl.Text = string.Format(Local("Designer.PlaceholderChoice"), num);
            }
        }

        _lstQuestions.Columns[0].Text = Local("Designer.ColumnQuestion");
        _btnAddQuestion.Text = Local("Designer.ButtonAddQuestion");
        _btnSaveExam.Text = Local("Designer.ButtonSaveExam");
        _btnLoadExam.Text = Local("Designer.ButtonLoadExam");
        _btnEditQuestion.Text = Local("Designer.ButtonEditQuestion");
        _btnDeleteQuestion.Text = Local("Designer.ButtonDeleteQuestion");

        _langToggleBtn.Text = _currentLanguage == AppLanguage.English ? "العربية" : "English";

        RightToLeft = _currentLanguage == AppLanguage.Arabic ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = _currentLanguage == AppLanguage.Arabic;
    }

    private void ToggleLanguage()
    {
        _currentLanguage = _currentLanguage == AppLanguage.English ? AppLanguage.Arabic : AppLanguage.English;
        ApplyLocalization();
    }

    private void AddQuestion(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtQuestion.Text))
        {
            MessageBox.Show(this, Local("Designer.MsgQuestionRequired"), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cmbCorrect.SelectedIndex < 0)
        {
            MessageBox.Show(this, Local("Designer.MsgCorrectRequired"), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var choices = new List<Choice>();
        for (var i = 0; i < 4; i++)
        {
            if (string.IsNullOrWhiteSpace(_txtChoices[i].Text))
            {
                MessageBox.Show(this, string.Format(Local("Designer.MsgChoiceEmpty"), i + 1), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            choices.Add(new Choice
            {
                Text = _txtChoices[i].Text.Trim(),
                IsCorrect = i == _cmbCorrect.SelectedIndex
            });
        }

        var question = new Question
        {
            Text = _txtQuestion.Text.Trim(),
            Choices = choices
        };

        _questions.Add(question);
        _lstQuestions.Items.Add(new ListViewItem(question.Text));

        _txtQuestion.Clear();
        foreach (var c in _txtChoices) c.Clear();
        _cmbCorrect.SelectedIndex = -1;
    }

    private async void SaveExam(object? sender, EventArgs e)
    {
        if (!_service.IsValidExam(_txtTitle.Text, _questions))
        {
            MessageBox.Show(this, Local("Designer.MsgTitleOrQuestions"), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var exam = new Exam
        {
            Title = _txtTitle.Text.Trim(),
            DurationMinutes = (int)_numDuration.Value,
            Questions = _questions
        };

        var path = _service.BuildPath(ExamsFolder, _txtTitle.Text);
        await _service.SaveExamAsync(path, exam);
        MessageBox.Show(this, Local("Designer.MsgSaved"), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void LoadExam(object? sender, EventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = ExamsFolder,
            Filter = Local("Designer.FilterJson")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var exam = await _service.LoadExamAsync(dialog.FileName);
        if (exam is null)
        {
            MessageBox.Show(this, Local("Designer.MsgLoadFailed"), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _txtTitle.Text = exam.Title;
        _numDuration.Value = Math.Clamp(exam.DurationMinutes, 1, 180);

        _questions.Clear();
        _questions.AddRange(exam.Questions ?? new List<Question>());

        _lstQuestions.Items.Clear();
        foreach (var q in _questions)
        {
            _lstQuestions.Items.Add(new ListViewItem(q.Text));
        }
    }

    private void EditQuestion(object? sender, EventArgs e)
    {
        if (_lstQuestions.SelectedIndices.Count == 0)
        {
            MessageBox.Show(this, Local("Designer.MsgSelectQuestionEdit"), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var index = _lstQuestions.SelectedIndices[0];
        using var editor = new QuestionEditorForm(_questions[index], _currentLanguage);
        if (editor.ShowDialog(this) == DialogResult.OK && editor.Result is not null)
        {
            _questions[index] = editor.Result;
            _lstQuestions.Items[index].Text = editor.Result.Text;
        }
    }

    private void DeleteQuestion(object? sender, EventArgs e)
    {
        if (_lstQuestions.SelectedIndices.Count == 0)
        {
            MessageBox.Show(this, Local("Designer.MsgSelectQuestionDelete"), Local("Designer.Caption"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(this, Local("Designer.MsgDeleteConfirm"), Local("Designer.Caption"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
            return;

        var index = _lstQuestions.SelectedIndices[0];
        _questions.RemoveAt(index);
        _lstQuestions.Items.RemoveAt(index);
    }
}
