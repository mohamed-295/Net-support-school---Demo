using NetSupport.Shared.Localization;
using NetSupport.Shared.Models;

namespace NetSupport.Designer.Forms;

public sealed class QuestionEditorForm : Form
{
    private readonly AppLanguage _language;

    private TextBox _txtQuestion = null!;
    private TextBox[] _txtChoices = null!;
    private ComboBox _cmbCorrect = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;
    private Label _questionLabel = null!;
    private Label _correctLabel = null!;

    public Question? Result { get; private set; }

    public QuestionEditorForm(Question? existing = null, AppLanguage language = AppLanguage.English)
    {
        _language = language;
        Text = Local("QuestionEditor.Title");
        Width = 620;
        Height = 460;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 420);

        BuildUi();
        ApplyLocalization();

        if (existing is not null)
            LoadQuestion(existing);
    }

    private string Local(string key) => LocalizationResources.GetString(key, _language);

    private void BuildUi()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < 7; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _questionLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        _correctLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

        _txtQuestion = new TextBox { Dock = DockStyle.Fill };
        _txtChoices = new TextBox[4];
        for (var i = 0; i < 4; i++)
        {
            _txtChoices[i] = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Tag = $"ChoiceLabel.{i + 1}" }, 0, i + 1);
            layout.Controls.Add(_txtChoices[i], 1, i + 1);
        }

        _cmbCorrect = new ComboBox
        {
            Dock = DockStyle.Left,
            Width = 140,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbCorrect.Items.AddRange(new object[] { "1", "2", "3", "4" });

        _btnOk = new Button { Width = 110, Height = 34 };
        _btnCancel = new Button { Width = 110, Height = 34 };
        _btnOk.Click += Save;
        _btnCancel.Click += (_, _) => Close();

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };
        buttons.Controls.Add(_btnOk);
        buttons.Controls.Add(_btnCancel);

        layout.Controls.Add(_questionLabel, 0, 0);
        layout.Controls.Add(_txtQuestion, 1, 0);
        layout.Controls.Add(_correctLabel, 0, 5);
        layout.Controls.Add(_cmbCorrect, 1, 5);
        layout.Controls.Add(buttons, 1, 6);

        Controls.Add(layout);
    }

    private void ApplyLocalization()
    {
        Text = Local("QuestionEditor.Title");
        _questionLabel.Text = Local("Designer.LabelQuestion");
        _correctLabel.Text = Local("Designer.LabelCorrect");
        _btnOk.Text = Local("QuestionEditor.ButtonSave");
        _btnCancel.Text = Local("QuestionEditor.ButtonCancel");

        _txtQuestion.PlaceholderText = Local("Designer.PlaceholderQuestion");
        for (var i = 0; i < _txtChoices.Length; i++)
        {
            _txtChoices[i].PlaceholderText = string.Format(Local("Designer.PlaceholderChoice"), i + 1);
        }

        var layout = (TableLayoutPanel)Controls[0];
        foreach (Control control in layout.Controls)
        {
            if (control is Label lbl && lbl.Tag is string tag && tag.StartsWith("ChoiceLabel."))
            {
                var number = tag.Split('.')[1];
                lbl.Text = string.Format(Local("Designer.PlaceholderChoice"), number);
            }
        }

        RightToLeft = _language == AppLanguage.Arabic ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = _language == AppLanguage.Arabic;
    }

    private void LoadQuestion(Question question)
    {
        _txtQuestion.Text = question.Text;
        for (var i = 0; i < 4 && i < question.Choices.Count; i++)
        {
            _txtChoices[i].Text = question.Choices[i].Text;
            if (question.Choices[i].IsCorrect)
            {
                _cmbCorrect.SelectedIndex = i;
            }
        }
    }

    private void Save(object? sender, EventArgs e)
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

        Result = new Question
        {
            Text = _txtQuestion.Text.Trim(),
            Choices = choices
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
