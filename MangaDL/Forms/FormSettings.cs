using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MangaDL.MangaObjects;

namespace MangaDL.Forms
{
    public partial class FormSettings: Form
    {

        public MangaDL.MangaObjects.Settings _settings;

        public FormSettings(MangaDL.MangaObjects.Settings settings)
        {
            InitializeComponent();
            _settings = settings;

            checkBox1.Checked = _settings.NeverUseShortFolderNames;
            comboBox1.SelectedIndex = ((int)_settings.ChapterSaveMode);
            textBox1.Text = _settings.OutputDirectory;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Checked =  (MessageBox.Show("Using Full folder names may result in directories with names too long to exist, continue?", "Contuinue", MessageBoxButtons.YesNo) == DialogResult.Yes);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _settings.NeverUseShortFolderNames = checkBox1.Checked;
            _settings.ChapterSaveMode = (MangaDL.MangaObjects.Settings.MangaSaveMode)comboBox1.SelectedIndex;
            _settings.OutputDirectory = textBox1.Text;
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
