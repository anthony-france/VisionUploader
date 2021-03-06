﻿using System;
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
        public enum UploadStatus { Queued, Compressing, Connecting, Uploading, Completed, Failed, Canceled}

        private string fullName;
        private string fileName;
        private UploadStatus status;
        private ulong size;
        private int progress;
        private List<string> compressList;

        public UploadFile() { }

        public UploadFile(List<string> fileList)
        {
            
            FileInfo fi = new FileInfo(fileList[0]);

            FullName = fi.FullName;
            FileName = fi.Name;
            Size = 0;
            Progress = 0;
            Status = UploadStatus.Queued;
            CompressList = fileList;

        }

        public UploadFile(string name)
        {
            FileInfo fi = new FileInfo(name);
                        
            FullName = fi.FullName;
            FileName = fi.Name;
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

        public string FullName 
        { 
            get { return this.fullName; }
            set 
            {
                if (value != this.fullName)
                {
                    this.fullName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string FileName
        {
            get { return this.fileName; }
            set
            {
                if (value != this.fileName)
                {
                    this.fileName = value;
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

        public List<string> CompressList
        {
            get;
            set;
        }
    }

}
