using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MangaDL.Forms
{
    public partial class BatchAddByURL: Form
    {
        public string[] links;
        public BatchAddByURL(string InputTxt = "")
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(InputTxt))
            {
                button1.Enabled = true;
                richTextBox1.Text = InputTxt;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            links = richTextBox1.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = !string.IsNullOrEmpty(richTextBox1.Text);
        }
    }
}
