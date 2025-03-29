using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MangaDL
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if(!System.IO.File.Exists("progress.json"))
                MessageBox.Show("ATTNTN: This downloader currently only downloads from bato.to");

            Application.Run(new FormMain());
        }
    }
}
