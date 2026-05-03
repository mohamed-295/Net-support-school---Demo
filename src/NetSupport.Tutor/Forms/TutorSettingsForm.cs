using NetSupport.Shared.Storage;

namespace NetSupport.Tutor.Forms;

public sealed class TutorSettingsForm : Form
{
    private readonly TextBox _txtListenUrl;
    private readonly TextBox _txtStudentHubUrl;

    public TutorConnectionSettings? Result { get; private set; }

    public TutorSettingsForm(TutorConnectionSettings current)
    {
        Text = "Tutor Settings";
        Width = 720;
        Height = 260;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        layout.Controls.Add(new Label
        {
            Text = "Tutor Listen URL",
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        }, 0, 0);
        _txtListenUrl = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = current.TutorListenUrl
        };
        layout.Controls.Add(_txtListenUrl, 1, 0);

        layout.Controls.Add(new Label
        {
            Text = "Student Hub URL",
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        }, 0, 1);
        _txtStudentHubUrl = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = current.StudentHubUrl
        };
        layout.Controls.Add(_txtStudentHubUrl, 1, 1);

        layout.Controls.Add(new Label
        {
            Text = "Students will use this server URL automatically in login.",
            ForeColor = Color.DimGray,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 1, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        var btnSave = new Button { Text = "Save", Width = 100, Height = 34 };
        var btnCancel = new Button { Text = "Cancel", Width = 100, Height = 34 };
        btnSave.Click += SaveClicked;
        btnCancel.Click += (_, _) => Close();
        actions.Controls.Add(btnSave);
        actions.Controls.Add(btnCancel);
        layout.Controls.Add(actions, 1, 3);

        Controls.Add(layout);
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var listen = _txtListenUrl.Text.Trim();
        var hub = _txtStudentHubUrl.Text.Trim();

        if (!Uri.TryCreate(listen, UriKind.Absolute, out _))
        {
            MessageBox.Show(this, "Tutor Listen URL is invalid.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Uri.TryCreate(hub, UriKind.Absolute, out _))
        {
            MessageBox.Show(this, "Student Hub URL is invalid.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new TutorConnectionSettings
        {
            TutorListenUrl = listen,
            StudentHubUrl = hub
        };
        DialogResult = DialogResult.OK;
        Close();
    }
}
