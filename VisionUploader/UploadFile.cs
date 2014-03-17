using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VisionUploader
{
    public class UploadFile : INotifyPropertyChanged
    {
        public enum UploadStatus { Queued, Connecting, Uploading, Completed, Failed, Canceled}

        public string name;
        public UploadStatus status;
        public ulong size;
        public int progress;

        public UploadFile() { }

        public UploadFile(string name)
        {
            FileInfo fi = new FileInfo(name);
            Name = Path.GetFullPath(name);
            Size = (ulong)fi.Length;

            Progress = 0;
            Status = UploadStatus.Queued;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Name { 
            get { return this.name; }
            set 
            {
                if (value != this.name)
                {
                    this.name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public UploadStatus Status
        {
            get { return this.status; }
            set
            {
                if (value != this.status)
                {
                    this.status = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Progress
        {
            get { return this.progress; }
            set
            {
                if (value != this.progress)
                {
                    this.progress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ulong Size
        {
            get { return this.size; }
            set
            {
                if (value != this.size)
                {
                    this.size = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

}
