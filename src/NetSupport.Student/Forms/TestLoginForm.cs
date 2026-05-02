using System;
using System.Windows.Forms;

namespace NetSupport.Student.Forms
{
    public partial class TestLoginForm : Form
    {
        
        private TextBox txtName;
        private Button btnStart;
        private Label lblPrompt;

        public string StudentName => txtName.Text;

        public TestLoginForm(string defaultName)
        {
            
            SetupManualUI(); 
            txtName.Text = defaultName;
        }

        private void SetupManualUI()
        {
            this.Text = "Test Login";
            this.Size = new System.Drawing.Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;

            lblPrompt = new Label { Text = "Enter your name:", Top = 20, Left = 20, Width = 200 };
            txtName = new TextBox { Top = 50, Left = 20, Width = 240 };
            btnStart = new Button { Text = "Start Test", Top = 90, Left = 80, Width = 120, DialogResult = DialogResult.OK };

            btnStart.Click += btnStart_Click;

            this.Controls.Add(lblPrompt);
            this.Controls.Add(txtName);
            this.Controls.Add(btnStart);
            this.AcceptButton = btnStart;
        }

        private void btnStart_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter your name to start.");
                return;
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}