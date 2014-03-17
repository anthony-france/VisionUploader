using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Ionic.Zip;


namespace VisionUploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<UploadFile> FileList = new ObservableCollection<UploadFile>();
        public int currentIndex = -1;
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            lvFileList.ItemsSource = FileList;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!worker.IsBusy && FileList.Where(x => x.Status == UploadFile.UploadStatus.Queued).Any())
            {
                UploadFile currentUpload = new UploadFile();
                currentUpload = FileList.Where(x => x.Status == UploadFile.UploadStatus.Queued).First();
                currentIndex = FileList.IndexOf(currentUpload);
                worker.RunWorkerAsync();
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FileList[currentIndex].Progress = e.ProgressPercentage;
            FileList[currentIndex].Status = (UploadFile.UploadStatus)e.UserState;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var client = new SftpClient("upload.visionps.com", 22, "uploaddemo", "uploaddemo2014"))
            {
                client.Connect();
                var file = FileList[currentIndex];
                using (var fs = new FileStream(file.Name, FileMode.Open))
                {
                    client.BufferSize = 4 * 1024;
                    worker.ReportProgress(0, UploadFile.UploadStatus.Connecting);
                    client.UploadFile(fs, System.IO.Path.GetFileName(file.Name), false, (bytesUploaded) =>
                    {
                        decimal progress = ((decimal)bytesUploaded / (decimal)file.Size) * 100;
                        worker.ReportProgress((int)progress, UploadFile.UploadStatus.Uploading);
                    });
                }
                client.Disconnect();
            }
        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {

            if ((e.Cancelled == true))
            {
                FileList[currentIndex].Status = UploadFile.UploadStatus.Canceled;
                FileList[currentIndex].Progress = 0;
            }

            else if (!(e.Error == null))
            {
                FileList[currentIndex].Status = UploadFile.UploadStatus.Failed;
                FileList[currentIndex].Progress = 0;
            }

            else
            {
                FileList[currentIndex].Status = UploadFile.UploadStatus.Completed;
                FileList[currentIndex].Progress = 100;
            }

        }

        public void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                MessageBoxResult result;

                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        UploadFile f = new UploadFile(file);
                        FileList.Add(f);
                    }
                    else if (Directory.Exists(file))
                    {
                        using (ZipFile zip = new ZipFile())
                        {
                            result = MessageBox.Show("You must compress the folder before upload.  Continue?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                zip.AddDirectory(file);
                                string zipFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".zip";
                                zip.Save(zipFileName);
                                FileList.Add(new UploadFile(zipFileName));
                            }
                        }
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}
