using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaDL.MangaObjects
{
    public interface ILoader
    {
        string LoaderName { get; }
        string Author { get; }
        string ServiceAddress { get; }
        Task<MangaSeries> GetSeriesFromURL(string URL, bool isMature = false);
        Task<bool> RescanChapters(MangaSeries series);
        Task<bool> PopulatePages(MangaSeries.MangaChapter BatoChapter);
        Task<bool> DownloadSeries(MangaSeries series, string OutDir, int ChapterSaveMode = 0);
        Task<bool> DownloadChapter(MangaSeries series, string ChapName, string OutDir, int ChapterSaveMode = 0);
        Task<long> DownloadPage(string DirPath, string EntryName, string URI);
    }
    public class MangaSeries
    {
        public bool STOP { get; set; }
        public bool UseAltTitle { get; set; }
        public bool checking { get; set; }
        public bool isMature { get; set; }
        public string URL { get; set; } // the URL For the series, to be processed with their respective scanners
        public string SeriesTitle { get; set; } // the title for the Manga series
        public string AltSeriesTitle { get; set; } // the alternate title for the Manga series if the main title is too long
        public string SeriesCoverURL { get; set; }
        public Dictionary<string, MangaChapter> chapters { get; set; } // The Key is the Chapter Name, Value is the chapter itself
        public List<string> DownloadedChapters { get; set; } // only used to track downloads between sessions, stores chapter keys
        public MangaSeries(string _title = "", string _URL = "")
        {
            URL = _URL;
            SeriesTitle = _title;
            chapters = new Dictionary<string, MangaChapter>();
            DownloadedChapters = new List<string>();
            isMature = false;
            checking = false;
        }
        public class MangaChapter
        {

            public bool finished { get; set; }
            public bool checking { get; set; }
            public string URL { get; set; } // the web address the chapter is at, used for deferred page loading
            public Dictionary<string, string> Pages { get; set; } // The Key is the Page number, Value is the URL
            public List<string> DownloadedPages { get; set; } // only used to track downloads between sessions, stores page keys
            public MangaChapter(string _URL = "")
            {
                URL = _URL;
                Pages = new Dictionary<string, string>();
                DownloadedPages = new List<string>();
                checking = false;
            }
        }
    }

    public class Settings
    {

        public string OutputDirectory { get; set; }
        public bool NeverUseShortFolderNames { get; set; } // not using short names could cause directory length issues
        public MangaSaveMode ChapterSaveMode { get; set; }
        public enum MangaSaveMode : int
        {
            CBZ = 0, // the only format that can save metadata
            ZIP = 1,
            FOLDER = 2
        };

        public Settings()
        {
            OutputDirectory = Environment.CurrentDirectory + "\\MangaDL";
            NeverUseShortFolderNames = false;
            ChapterSaveMode = MangaSaveMode.CBZ;
        }

    }
}
