using NetSupport.Shared.Models;

namespace NetSupport.Designer.Forms;

public sealed class QuestionEditorForm : Form
{
    private TextBox txtQuestion = null!;
    private TextBox[] txtChoices = null!;
    private ComboBox cmbCorrect = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;

    public Question? Result { get; private set; }

    public QuestionEditorForm(Question? existing = null)
    {
        Text = "Question Editor";
        Width = 500;
        Height = 400;
        StartPosition = FormStartPosition.CenterParent;

        InitializeUI();

        if (existing != null)
            LoadQuestion(existing);
    }

    private void InitializeUI()
    {
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(10),
            AutoScroll = true
        };

        txtQuestion = new TextBox { Width = 400, PlaceholderText = "Question" };

        txtChoices = new TextBox[4];
        for (int i = 0; i < 4; i++)
        {
            txtChoices[i] = new TextBox
            {
                Width = 400,
                PlaceholderText = $"Choice {i + 1}"
            };
            layout.Controls.Add(txtChoices[i]);
        }

        cmbCorrect = new ComboBox { Width = 400 };
        cmbCorrect.Items.AddRange(new[] { "1", "2", "3", "4" });

        btnOk = new Button { Text = "Save", Width = 100 };
        btnCancel = new Button { Text = "Cancel", Width = 100 };

        btnOk.Click += Save;
        btnCancel.Click += (s, e) => Close();

        layout.Controls.Add(new Label { Text = "Question" });
        layout.Controls.Add(txtQuestion);

        layout.Controls.Add(new Label { Text = "Correct Answer (1-4)" });
        layout.Controls.Add(cmbCorrect);

        layout.Controls.Add(btnOk);
        layout.Controls.Add(btnCancel);

        Controls.Add(layout);
    }

    private void LoadQuestion(Question q)
    {
        txtQuestion.Text = q.Text;

        for (int i = 0; i < 4; i++)
        {
            txtChoices[i].Text = q.Choices[i].Text;

            if (q.Choices[i].IsCorrect)
                cmbCorrect.SelectedIndex = i;
        }
    }

    private void Save(object? sender, EventArgs e)
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
        var errors = new List<string>();

        for (int i = 0; i < 4; i++)
        {
            if (txtChoices[i] == null ||
                string.IsNullOrWhiteSpace(txtChoices[i].Text))
            {
                errors.Add($"Choice {i + 1} is empty");
            }

            

            choices.Add(new Choice
            {
                Text = txtChoices[i].Text.Trim(),
                IsCorrect = (i == cmbCorrect.SelectedIndex)
            });
        }

        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join("\n", errors));
            return;
        }

        Result = new Question
        {
            Text = txtQuestion.Text.Trim(),
            Choices = choices
        };

        DialogResult = DialogResult.OK;
        Close();
    }


}