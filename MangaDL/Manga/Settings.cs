using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaDL
{
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
