using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using MangaDL.MangaObjects;

namespace MangaDL.Loader
{
    public class Batoto : ILoader
    {
        public string LoaderName => "Batoto Loader";
        public string Author => "PhoenixARC";
        public string ServiceAddress => "://bato.to/";

        public async Task<MangaSeries> GetSeriesFromURL(string URL, bool isMature = false)
        {
            MangaSeries series = new MangaSeries();
            series.chapters = new Dictionary<string, MangaSeries.MangaChapter>();
            series.SeriesTitle = "Placeholder Title";
            series.URL = URL;
            series.isMature = isMature;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.6788.76 Safari/537.36");
            string page = await client.GetStringAsync(URL);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(page);



            var listItems = isMature ? document.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[2]/div[5]/div[3]/div") :
                                        document.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[1]/div[5]/div[3]/div");

            var titleNode = isMature ? document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/h3[1]/a[1]") :
                                        document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/h3[1]/a[1]");

            var coverNode = isMature ? document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/div[1]/div[2]/div[3]/div[1]/img[1]") :
                                       document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/div[1]/div[1]/div[3]/div[1]/img[1]");

            if (titleNode == null || listItems == null || coverNode == null)
            {
                throw new Exception("Cannot load series - is the mature flag wrong?");
                
                return null;
            }

            series.SeriesTitle = Regex.Replace(titleNode.InnerText, @"[^\u0000-\u007F]+", string.Empty);
            series.SeriesTitle = Regex.Replace(series.SeriesTitle, @"[:\\\/><?*|]+", string.Empty);
            series.SeriesTitle = series.SeriesTitle.Replace('\"', ' ');
            series.SeriesTitle = series.SeriesTitle.Replace("&#39;", "");
            series.SeriesCoverURL = coverNode.Attributes[1].Value;

            series.UseAltTitle = true;
            series.AltSeriesTitle = String.Format("{0:X}", series.SeriesTitle.GetHashCode());


            listItems.Reverse();
            foreach (var item in listItems)
            {
                string ChapName = item.ChildNodes[1].ChildNodes[1].InnerText.Replace("Episode", "Chapter").Replace("Ep", "Ch");
                ChapName = Regex.Replace(ChapName, @"[^\u0000-\u007F]+", string.Empty);
                ChapName = Regex.Replace(ChapName, @"[:\\\/><?*|]+", string.Empty);
                ChapName = ChapName.Replace('\"', ' ');
                ChapName = ChapName.Replace("&#39;", "");
                string ChapURI = item.ChildNodes[1].Attributes[1].Value;

                MangaSeries.MangaChapter BChapter = new MangaSeries.MangaChapter();
                BChapter.URL = "https://bato.to" + ChapURI;

                if (!series.chapters.ContainsKey(ChapName.Trim()))
                {
                    series.chapters.Add(ChapName.Trim(), BChapter);
                }
            }

            return series;
        }

        public async Task<bool> RescanChapters(MangaSeries series)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.6788.76 Safari/537.36");
            string page = await client.GetStringAsync(series.URL);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(page);


            var listItems = series.isMature ? document.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[2]/div[5]/div[3]/div") :
                                        document.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[1]/div[5]/div[3]/div");

            if (listItems != null)
            {
                listItems.Reverse();
                foreach (var item in listItems)
                {
                    string ChapName = item.ChildNodes[1].ChildNodes[1].InnerText.Replace("Episode", "Chapter").Replace("Ep", "Ch");
                    ChapName = Regex.Replace(ChapName, @"[^\u0000-\u007F]+", string.Empty);
                    ChapName = Regex.Replace(ChapName, @"[:\\\/><?*|]+", string.Empty);
                    ChapName = ChapName.Replace('\"', ' ');
                    ChapName = ChapName.Replace("&#39;", "");
                    string ChapURI = item.ChildNodes[1].Attributes[1].Value;

                    MangaSeries.MangaChapter BChapter = new MangaSeries.MangaChapter();
                    BChapter.URL = "https://bato.to" + ChapURI;

                    if (!series.chapters.ContainsKey(ChapName.Trim()))
                    {
                        series.chapters.Add(ChapName.Trim(), BChapter);
                    }
                }
            }
            return true;
        }

        public async Task<bool> PopulatePages(MangaSeries.MangaChapter BatoChapter)
        {
            if (BatoChapter.finished)
            {
                BatoChapter.Pages.Add("cover.webp", "");
                BatoChapter.DownloadedPages.Add("cover.webp");
                return true;
            }
            BatoChapter.Pages.Clear();

            BatoChapter.checking = true;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.6788.76 Safari/537.36");
            string page = await client.GetStringAsync(BatoChapter.URL);


            HtmlDocument chapter = new HtmlDocument();
            chapter.OptionFixNestedTags = true;
            chapter.LoadHtml(page);

            var pages = chapter.DocumentNode.SelectSingleNode("/html[1]/body[1]/script[13]");

            string[] lines = pages.InnerHtml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int pNum = 0;
            foreach (string entry in lines[5].Split(new[] { "\"" }, StringSplitOptions.None))
            {
                if (entry.StartsWith("http"))
                {
                    BatoChapter.Pages.Add(pNum + ".webp", entry);
                    pNum++;
                }
            }
            BatoChapter.checking = false;
            System.GC.Collect();

            return true;
        }

        public async Task<bool> DownloadSeries(MangaSeries series, string OutDir, int ChapterSaveMode = 0)
        {
            foreach (KeyValuePair<string, MangaSeries.MangaChapter> chapter in series.chapters)
            {
                if (series.STOP) return true;
                string PathToUse = series.UseAltTitle ? series.AltSeriesTitle : series.SeriesTitle;
                DownloadChapter(series, chapter.Key.Trim(), OutDir + "\\" + PathToUse, ChapterSaveMode);
                series.DownloadedChapters.Add(chapter.Key);

            }
            return true;
        }

        public async Task<bool> DownloadChapter(MangaSeries series, string ChapName, string OutDir, int ChapterSaveMode = 0)
        {

            if (File.Exists(OutDir + "\\" + ChapName + ".zip") || File.Exists(OutDir + "\\" + ChapName + ".cbz") || series.chapters[ChapName].finished)
            {
                foreach (KeyValuePair<string, string> page in series.chapters[ChapName].Pages)
                {
                    series.chapters[ChapName].DownloadedPages.Add(page.Key);
                }
                return true;
            }

            string DirName = OutDir + "\\" + ChapName;
            string CoverName = DirName + "\\cover.webp";

            Directory.CreateDirectory(DirName);
            try
            {
                File.WriteAllText(DirName + "\\ComicInfo.xml", "<?xml version=\"1.0\"?><ComicInfo><Series>" + series.SeriesTitle + "</Series><Title>" + ChapName + "</Title></ComicInfo>");
                File.WriteAllBytes(CoverName, new WebClient().DownloadData(series.SeriesCoverURL));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
            }


            foreach (KeyValuePair<string, string> page in series.chapters[ChapName].Pages)
            {
                if (series.STOP) return true;
                await DownloadPage(OutDir + "\\" + ChapName, page.Key, page.Value);
                series.chapters[ChapName].DownloadedPages.Add(page.Key);
            }

            switch (ChapterSaveMode)
            {
                case 0:
                    ZipFile.CreateFromDirectory(OutDir + "\\" + ChapName, OutDir + "\\" + ChapName + ".cbz");
                    Directory.Delete(OutDir + "\\" + ChapName, true);
                    break;
                case 1:
                    ZipFile.CreateFromDirectory(OutDir + "\\" + ChapName, OutDir + "\\" + ChapName + ".zip");
                    Directory.Delete(OutDir + "\\" + ChapName, true);
                    break;
            }

            series.chapters[ChapName].finished = true;
            return true;
        }

        public async Task<long> DownloadPage(string DirPath, string EntryName, string URI)
        {
            WebClient wc = new WebClient();
            try
            {
                if (File.Exists(DirPath + "\\" + EntryName)) return File.ReadAllBytes(DirPath + "\\" + EntryName).LongLength;


                Directory.CreateDirectory(DirPath);
                byte[] imageData = wc.DownloadData(URI);
                File.WriteAllBytes(DirPath + "\\" + EntryName, imageData);
                return imageData.LongLength;
            }
            catch
            {
            }
            return 0;
        }
    }
}
