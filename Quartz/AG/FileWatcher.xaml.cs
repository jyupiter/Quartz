using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Quartz.AG
{
    /// <summary>
    /// Interaction logic for FileWatcher.xaml
    /// </summary>
    public partial class FileWatcher : Page
    {
        public FileWatcher()
        {
            InitializeComponent();
            IDG();
        }

        #region SetUp

        private void IDG()
        {
            DataGridTextColumn indx = new DataGridTextColumn();
            DataGridTextColumn evnt = new DataGridTextColumn();
            DataGridTextColumn desc = new DataGridTextColumn();
            DataGridTextColumn date = new DataGridTextColumn();
            DataGridTextColumn time = new DataGridTextColumn();
            ScanResultDataGrid.Columns.Add(indx);
            ScanResultDataGrid.Columns.Add(evnt);
            ScanResultDataGrid.Columns.Add(desc);
            ScanResultDataGrid.Columns.Add(date);
            ScanResultDataGrid.Columns.Add(time);
            indx.Header = "#";
            evnt.Header = "Event";
            desc.Header = "Description";
            date.Header = "Date";
            time.Header = "Time";
            indx.Binding = new Binding("I");
            evnt.Binding = new Binding("E");
            desc.Binding = new Binding("D");
            date.Binding = new Binding("A");
            time.Binding = new Binding("T");
        }

        private struct Record
        {
            public int I { get; set; }
            public string E { get; set; }
            public string D { get; set; }
            public string A { get; set; }
            public string T { get; set; }
        }

        #endregion

        #region FileSystemWatcher

        private static int id = 0;
        private FileSystemWatcher watcher;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void Run()
        {
            string path = "C:\\";
            if(TargetDirectory.Text != null && TargetDirectory.Text.Length > 2)
                path = TargetDirectory.Text;

            watcher = new FileSystemWatcher
            {
                Path = path,

                // Watch for changes
                NotifyFilter = NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.FileName
                             | NotifyFilters.DirectoryName,

                Filter = "*.*",

                IncludeSubdirectories = true
            };

            // Add event handlers
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching
            watcher.EnableRaisingEvents = true;
        }

        // Event handlers

        private void OnChanged(object source, FileSystemEventArgs x)
        {
            Dispatcher.Invoke(() =>
            {
                if((bool)EnableFiltering.IsChecked)
                {
                    string ext = Path.GetExtension(x.FullPath);
                    if((bool)FilterInclude.IsChecked)
                    {
                        if(Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            ScanResultDataGrid.Items.Add(new Record
                            {
                                I = id,
                                E = ("" + x.ChangeType),
                                D = (x.FullPath + " was " + x.ChangeType),
                                A = DateTime.Today.ToString("dd-MM-yyyy"),
                                T = DateTime.Now.ToString("HH:mm:ss tt")
                            });
                            id++;
                        }
                    }
                    else
                    {
                        if(!Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            ScanResultDataGrid.Items.Add(new Record
                            {
                                I = id,
                                E = ("" + x.ChangeType),
                                D = (x.FullPath + " was " + x.ChangeType),
                                A = DateTime.Today.ToString("dd-MM-yyyy"),
                                T = DateTime.Now.ToString("HH:mm:ss tt")
                            });
                            id++;
                        }
                    }
                }
            });
        }

        private void OnRenamed(object source, RenamedEventArgs x)
        {
            Dispatcher.Invoke(() =>
            {
                string ext = Path.GetExtension(x.FullPath);
                if((bool)EnableFiltering.IsChecked)
                {
                    if(Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                    {
                        ScanResultDataGrid.Items.Add(new Record
                        {
                            I = id,
                            E = ("Renamed"),
                            D = (x.OldFullPath + " was Renamed to " + x.FullPath),
                            A = DateTime.Today.ToString("dd-MM-yyyy"),
                            T = DateTime.Now.ToString("HH:mm:ss tt")
                        });
                        id++;
                    }
                }
                else
                {
                    if(!Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                    {
                        ScanResultDataGrid.Items.Add(new Record
                        {
                            I = id,
                            E = ("Renamed"),
                            D = (x.OldFullPath + " was Renamed to " + x.FullPath),
                            A = DateTime.Today.ToString("dd-MM-yyyy"),
                            T = DateTime.Now.ToString("HH:mm:ss tt")
                        });
                        id++;
                    }
                }
            });
        }

        private void Terminate()
        {
            watcher.Dispose();
        }

        #endregion

        #region Timer

        Stopwatch stopWatch = new Stopwatch();
        string currentTime = string.Empty;

        private void Clock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Tick;
            stopWatch.Start();
            timer.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if(stopWatch.IsRunning)
                {
                    TimeSpan ts = stopWatch.Elapsed;
                    currentTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    TimeElapsed.Content = currentTime;
                }
            });
        }

        private void Tock()
        {
            if(stopWatch.IsRunning)
            {
                stopWatch.Stop();
            }
        }

        #endregion

        private void StartLiveScan(object sender, RoutedEventArgs e)
        {
            LiveScanBtn.Content = "Stop scan";
            Run();
            Clock();
            LiveScanBtn.Click -= StartLiveScan;
            LiveScanBtn.Click += StopLiveScan;
        }

        private void StopLiveScan(object sender, RoutedEventArgs e)
        {
            LiveScanBtn.Content = "Start scan";
            Terminate();
            Tock();
            LiveScanBtn.Click -= StopLiveScan;
            LiveScanBtn.Click += StartLiveScan;
        }
    }
}
