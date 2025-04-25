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
    public partial class AddSeries: Form
    {

        public MangaSeries _series;
        List<ILoader> loaders;


        //TESTING URLS

        //https://bato.to/series/183028/my-notoriously-naughty-lover
        //https://bato.to/series/158951/they-say-there-s-a-ghost-in-the-club-room
        //https://bato.to/series/145932/rooming-with-a-gamer-gal-official

        public AddSeries(List<ILoader> loaderList, string URL = "")
        {
            InitializeComponent();
            loaders = loaderList;
            richTextBox1.Text = URL;
            if (!string.IsNullOrEmpty(URL))
                CheckURL(URL);
        }

        public ILoader getLoaderForURL(string URI)
        {
            return loaders.FirstOrDefault(x => x.ServiceAddress.StartsWith(URI));
        }
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CheckChildren(e.Node, e.Node.Checked);
        }
        private void CheckChildren(TreeNode rootNode, bool isChecked)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                CheckChildren(node, isChecked);
                node.Checked = isChecked;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckURL(richTextBox1.Text);
        }

        private async void CheckURL(string URL)
        {
            await SanitizeBatotoV3();
            URL = richTextBox1.Text;
            button2.Enabled = false;
            MangaSeries series = await getLoaderForURL(URL).GetSeriesFromURL(URL, checkBox1.Checked);
            treeView1.Nodes.Clear();
            if (series == null) return;

            TreeNode seriesNode = new TreeNode(series.SeriesTitle + "[" + series.URL + "]");
            foreach (KeyValuePair<string, MangaSeries.MangaChapter> chap in series.chapters)
                seriesNode.Nodes.Add(chap.Key + "[" + chap.Value.URL + "]");
            treeView1.Nodes.Add(seriesNode);
            _series = series;
            treeView1.ExpandAll();
            button2.Enabled = true;
        }

        private async Task<bool> SanitizeBatotoV3()
        {
            string BatoV3 = richTextBox1.Text;
            if (BatoV3.StartsWith("https://bato.to/title/"))
            {
                string BatoV2 = BatoV3.Replace("https://bato.to/title/", "https://bato.to/series/");

                int index = BatoV2.IndexOf('-');
                if (index != -1)
                {
                    richTextBox1.Text = BatoV2.Substring(0, index) + '/' + BatoV2.Substring(index + 1);
                }
            }
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
