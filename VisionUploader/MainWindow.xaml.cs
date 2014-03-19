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
using VisionUploader.Classes;
using Renci.SshNet.Sftp;


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

        private string server = "upload.visionps.com";
        private int port = 22;

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
            var file = FileList[currentIndex];

            if (file.CompressList != null && file.CompressList.Count > 0)
            {
                worker.ReportProgress(0, UploadFile.UploadStatus.Compressing);

                using (ZipFile zip = new ZipFile())
                {

                    foreach (var c in file.CompressList)
                    {
                        if (File.Exists(c))
                        {
                            zip.AddFile(c);
                        }
                        else if (Directory.Exists(c))
                        {
                            zip.AddDirectory(c);
                        }
                        else { }
                    }

                    zip.SaveProgress += (s, ev) =>
                    {
                        if (ev.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
                        {
                            decimal progress;
                            if (ev.EntriesTotal > 0)
                            {
                                progress = ((decimal)ev.EntriesSaved / (decimal)ev.EntriesTotal) * 100;
                            }
                            else
                            {
                                progress = 0;
                            }
                            worker.ReportProgress((int)progress, UploadFile.UploadStatus.Compressing);
                        }
                    };

                    FileList[currentIndex].FullName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".zip";
                    zip.Save(FileList[currentIndex].FullName);
                }
                FileList[currentIndex].CompressList.Clear();
                FileInfo info = new FileInfo(file.FullName);
                FileList[currentIndex].Size = (ulong)info.Length;
             }

             if (worker.CancellationPending == false)
             {
                string password = CredentialsHelper.ToInsecureString(CredentialsHelper.DecryptString(Properties.Settings.Default.password.ToString()));
                using (var client = new SftpClient(server, port, Properties.Settings.Default.username.ToString(), password))
                {
                    worker.ReportProgress(0, UploadFile.UploadStatus.Connecting);
                    client.Connect();
                    client.BufferSize = 4 * 1024;

                    using (var fs = new FileStream(file.FullName, FileMode.Open))
                    {
                        client.UploadFile(fs, System.IO.Path.GetFileName(file.FullName), true, (bytesUploaded) =>
                        {
                            decimal progress = ((decimal)bytesUploaded / (decimal)file.Size) * 100;
                            worker.ReportProgress((int)progress, UploadFile.UploadStatus.Uploading);
                        });
                    }
                    client.Disconnect();
                }
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        UploadFile f = new UploadFile(file);
                        FileList.Add(f);
                        this.lvFileList.ScrollIntoView(f);
                    }
                    else if (Directory.Exists(file))
                    {
                        var f = new UploadFile(new List<string>() { file });
                        FileList.Add(f);
                        this.lvFileList.ScrollIntoView(f);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        private void menuAccount_Click(object sender, RoutedEventArgs e)
        {
            AccountWindow w = new AccountWindow();
            w.Show();
        }
        
        private void mnuCancel_Click(object sender, RoutedEventArgs e)
        {
            UploadFile selected_lvi = this.lvFileList.SelectedItem as UploadFile;
            if (selected_lvi != null)
            {
                FileList[FileList.IndexOf(selected_lvi)].Status = UploadFile.UploadStatus.Canceled;
                worker.CancelAsync();
            }
        }

        private void mnuRestart_Click(object sender, RoutedEventArgs e)
        {
            UploadFile selected_lvi = this.lvFileList.SelectedItem as UploadFile;
            if (selected_lvi != null)
            {
                FileList[FileList.IndexOf(selected_lvi)].Status = UploadFile.UploadStatus.Queued;
            }
        }
    }
}
