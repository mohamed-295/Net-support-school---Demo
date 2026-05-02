public partial class TestLoginForm : Form
{
    public string StudentName => txtName.Text;

    public TestLoginForm(string defaultName)
    {
        InitializeComponent();
        txtName.Text = defaultName;
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Enter your name");
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}