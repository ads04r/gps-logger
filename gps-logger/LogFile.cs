using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gps_logger
{
    public class LogFile
    {
        public Windows.Storage.StorageFile file { get; set; }
        public string label { get; set; }
        public string ds { get; set; }
        public LogFile()
        {
            this.file = null;
            this.label = "Unknown place";
            this.ds = "";
        }
    }

    public class LogFileViewModel
    {
        private LogFile defaultLogFile = new LogFile();
        public LogFile DefaultLogFile {  get { return this.defaultLogFile; } }
        private System.Collections.ObjectModel.ObservableCollection<LogFile> logfiles = new System.Collections.ObjectModel.ObservableCollection<LogFile>();
        public System.Collections.ObjectModel.ObservableCollection<LogFile> LogFiles {  get { return this.logfiles; } }
        public LogFileViewModel()
        {

        }

        public async System.Threading.Tasks.Task Refresh()
        {
            Windows.Storage.StorageFolder fLocal = Windows.Storage.ApplicationData.Current.LocalCacheFolder;
            IReadOnlyList<Windows.Storage.StorageFile> files = await fLocal.GetFilesAsync();
            foreach (Windows.Storage.StorageFile lfile in files)
            {
                this.logfiles.Add(new LogFile() { file = lfile, label=this.get_location(lfile), ds=lfile.DateCreated.ToString("ddd, d MMM yyyy, HH:mm") });
            }
        }

        private string get_location(Windows.Storage.StorageFile f)
        {
            return (f.Name.ToString());
        }
    }
}
