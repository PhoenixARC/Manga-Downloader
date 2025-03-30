using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sample;
using MangaDL.Manga;
using MangaDL.Manga.Sites;
using MangaDL.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using Newtonsoft.Json;

namespace MangaDL
{
    public partial class FormMain: Form
    {
        public Dictionary<string, MangaSeries> ActiveTrackers = new Dictionary<string, MangaSeries>();
        public Settings ProgramSettings = new Settings();

        public FormMain()
        {
            InitializeComponent();
        }

        private async void UpdateTrackers()
        {
            if (dataGridView1.Rows.Count == ActiveTrackers.Count)
                return;

            
            foreach (KeyValuePair<string, MangaSeries> tracker in ActiveTrackers)
            {
                await Task.Run(async () =>
                {
                    bool isIncl = false;

                    Invoke(new Action(() =>
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.Cells[0].Value == tracker.Key)
                            {
                                isIncl = true;
                                row.Cells[1].Value = tracker.Value.DownloadedChapters.Count.ToString();
                                row.Cells[2].Value = tracker.Value.chapters.Count.ToString();
                                row.Cells[3].Value = (int)Math.Round((float)(tracker.Value.DownloadedChapters.Count / tracker.Value.chapters.Count) * 100);
                            }
                        }
                    }));
                    if (!isIncl)
                        Invoke(new Action(() =>
                        {
                            dataGridView1.Rows.Add(tracker.Key, tracker.Value.DownloadedChapters.Count, tracker.Value.chapters.Count.ToString(), (int)Math.Round((float)(tracker.Value.DownloadedChapters.Count / tracker.Value.chapters.Count) * 100));
                        }));
                });
            }
        }

        private DataGridViewRow getRowByName(DataGridView view, string name)
        {
            foreach(DataGridViewRow row in view.Rows)
            {
                if (row.Cells[0].Value == name)
                    return row;
            }
            return null;
        }

        private async Task<bool> RefreshInfo()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (ActiveTrackers[row.Cells[0].Value.ToString()].checking)
                {
                    row.Cells[1].Value = "CHECKING";
                    row.Cells[2].Value = "CHECKING";
                    row.Cells[3].Value = 0;
                }
                else
                {
                    row.Cells[1].Value = ActiveTrackers[row.Cells[0].Value.ToString()].DownloadedChapters.Count.ToString();
                    row.Cells[2].Value = (ActiveTrackers[row.Cells[0].Value.ToString()].chapters.Count - ActiveTrackers[row.Cells[0].Value.ToString()].DownloadedChapters.Count).ToString();
                    float DonePercent = (((float)ActiveTrackers[row.Cells[0].Value.ToString()].DownloadedChapters.Count / (float)ActiveTrackers[row.Cells[0].Value.ToString()].chapters.Count) * 100);
                    row.Cells[3].Value = (int)Math.Round(DonePercent);
                }
            }
            if (dataGridView2.Rows.Count == 0) return true;
            foreach(DataGridViewRow row in dataGridView2.Rows)
            {
                MangaSeries _series = ActiveTrackers[dataGridView1.SelectedRows[0].Cells[0].Value.ToString()];
                if (_series.chapters[row.Cells[0].Value.ToString()].checking)
                {
                    row.Cells[1].Value = "CHECKING";
                    row.Cells[2].Value = "CHECKING";
                    row.Cells[3].Value = 0;
                }
                else
                {
                    row.Cells[1].Value = _series.chapters[row.Cells[0].Value.ToString()].DownloadedPages.Count.ToString();
                    row.Cells[2].Value = (_series.chapters[row.Cells[0].Value.ToString()].Pages.Count - _series.chapters[row.Cells[0].Value.ToString()].DownloadedPages.Count).ToString();
                    
                    float DonePercent = (((float)_series.chapters[row.Cells[0].Value.ToString()].DownloadedPages.Count / (float)_series.chapters[row.Cells[0].Value.ToString()].Pages.Count) * 100);
                    row.Cells[3].Value = 100;
                    if(DonePercent < 100.0)
                        row.Cells[3].Value = (int)Math.Round(DonePercent);
                    if (_series.chapters[row.Cells[0].Value.ToString()].DownloadedPages.Count == 0)
                        row.Cells[3].Value = 0;

                }
            }
            return true;
        }

        private async Task<bool> DLTracker(KeyValuePair<string, MangaSeries> tracker)
        {
            await Task.Run(async () =>
            {
                await CheckTracker(tracker.Key);
            });
            dataGridView1_SelectionChanged(dataGridView1, EventArgs.Empty);
            Task.Run(async () =>
            {
                await Batoto.DownloadSeries(tracker.Value, ProgramSettings.OutputDirectory, (int)ProgramSettings.ChapterSaveMode);
            });
            tracker.Value.checking = false;
            return true;
        }

        private async Task<bool> StartAll()
        {
            foreach (KeyValuePair<string, MangaSeries> tracker in ActiveTrackers)
            {
                tracker.Value.checking = true;
            }
            foreach (KeyValuePair<string, MangaSeries> tracker in ActiveTrackers)
            {
                DLTracker(tracker);

            }
            return true;
        }

        public async Task<bool> CheckTracker(string Key)
        {
            ActiveTrackers[Key].UseAltTitle = !ProgramSettings.NeverUseShortFolderNames;
            if (ActiveTrackers[Key].URL.StartsWith("https://bato.to/"))
            {
                ActiveTrackers[Key].DownloadedChapters.Clear();
                foreach (KeyValuePair<string, MangaSeries.MangaChapter> chap in ActiveTrackers[Key].chapters)
                {
                    chap.Value.DownloadedPages.Clear();
                    string DLPath = ProgramSettings.OutputDirectory + "\\" + ActiveTrackers[Key].SeriesTitle + "\\" + chap.Key;
                    string DLPath2 = ProgramSettings.OutputDirectory + "\\" + ActiveTrackers[Key].AltSeriesTitle + "\\" + chap.Key;

                    try
                    {
                        if (File.Exists(DLPath + ".zip") ||File.Exists(DLPath + ".cbz") || File.Exists(DLPath2 + ".zip")|| File.Exists(DLPath2 + ".cbz"))
                            chap.Value.finished = true;
                    }
                    catch { }

                    if (File.Exists(DLPath2 + ".zip") || File.Exists(DLPath2 + ".cbz"))
                        chap.Value.finished = true;
                }

                await Batoto.RescanChapters(ActiveTrackers[Key]);
                foreach(KeyValuePair<string, MangaSeries.MangaChapter> chap in ActiveTrackers[Key].chapters)
                {
                    if(!chap.Value.finished)
                    await Task.Run(async () =>
                    {
                        await Batoto.PopulatePages(chap.Value);

                    });
                }

            }
            return true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AddSeries adds = new AddSeries();
            if (adds.ShowDialog() == DialogResult.OK && !ActiveTrackers.ContainsKey(adds._series.SeriesTitle))
            {
                if (!ActiveTrackers.ContainsKey(adds._series.SeriesTitle))
                {
                    ActiveTrackers.Add(adds._series.SeriesTitle, adds._series);
                    UpdateTrackers();
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //this.Text = "Manga Downloader[Fake Download Speed]";
            StartAll();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshInfo();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {   
            if (dataGridView1.Rows.Count == 0 || dataGridView1.SelectedRows.Count != 1)
                return;
            dataGridView2.Rows.Clear();
            MangaSeries _series = ActiveTrackers[dataGridView1.SelectedRows[0].Cells[0].Value.ToString()];
            foreach (KeyValuePair<string, MangaSeries.MangaChapter> chap in _series.chapters)
            {
                dataGridView2.Rows.Add(chap.Key, chap.Value.DownloadedPages.Count.ToString(), (chap.Value.Pages.Count - chap.Value.DownloadedPages.Count).ToString(), 0);
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            string OutJSON = Newtonsoft.Json.JsonConvert.SerializeObject(ActiveTrackers, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("Progress.json", OutJSON);
            OutJSON = Newtonsoft.Json.JsonConvert.SerializeObject(ProgramSettings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("Config.json", OutJSON);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                removeToolStripMenuItem.Enabled = false;
                copyNameToolStripMenuItem.Enabled = false;
            }

        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveTrackers[dataGridView1.SelectedRows[0].Cells[0].Value.ToString()].STOP = true;
            ActiveTrackers.Remove(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            UpdateTrackers();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (File.Exists("progress.json"))
            {
                ActiveTrackers = JsonConvert.DeserializeObject<Dictionary<string, MangaSeries>>(File.ReadAllText("progress.json"));

                foreach (KeyValuePair<string, MangaSeries> tracker in ActiveTrackers)
                {
                    tracker.Value.DownloadedChapters.Clear();
                    foreach (KeyValuePair<string, MangaSeries.MangaChapter> chap in tracker.Value.chapters)
                    {
                        chap.Value.DownloadedPages.Clear();
                    }
                }
                UpdateTrackers();
            }
            if (File.Exists("config.json"))
            {
                ProgramSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.json"));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BatchAddByURL batch = new BatchAddByURL();
            if (batch.ShowDialog() == DialogResult.OK)
            {
                foreach (string line in batch.links)
                {
                    AddSeries adds = new AddSeries(line);
                    if (adds.ShowDialog() == DialogResult.OK && !ActiveTrackers.ContainsKey(adds._series.SeriesTitle))
                    {
                        ActiveTrackers.Add(adds._series.SeriesTitle, adds._series);
                        UpdateTrackers();
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(KeyValuePair<string, MangaSeries> series in ActiveTrackers)
            {
                sb.AppendLine(series.Value.URL);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
                File.WriteAllText(sfd.FileName, sb.ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File|*.txt";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                BatchAddByURL batch = new BatchAddByURL(File.ReadAllText(ofd.FileName));
                if (batch.ShowDialog() == DialogResult.OK)
                {
                    foreach (string line in batch.links)
                    {
                        AddSeries adds = new AddSeries(line);
                        if (adds.ShowDialog() == DialogResult.OK && !ActiveTrackers.ContainsKey(adds._series.SeriesTitle))
                        {
                            ActiveTrackers.Add(adds._series.SeriesTitle, adds._series);
                            UpdateTrackers();
                        }
                    }
                }

            }
        }

        private void copyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
        }

        private void copyFolderNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ActiveTrackers[dataGridView1.SelectedRows[0].Cells[0].Value.ToString()].AltSeriesTitle);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FormSettings settings = new FormSettings(ProgramSettings);
            if(settings.ShowDialog() == DialogResult.OK)
            {
                ProgramSettings = settings._settings;
            }
        }

        private void markAsUnfinishedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveTrackers[dataGridView1.SelectedRows[0].Cells[0].Value.ToString()].chapters[dataGridView2.SelectedRows[0].Cells[0].Value.ToString()].DownloadedPages.Clear();
            ActiveTrackers[dataGridView1.SelectedRows[0].Cells[0].Value.ToString()].chapters[dataGridView2.SelectedRows[0].Cells[0].Value.ToString()].finished = false;
        }
    }
}
