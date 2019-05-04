using System;
using System.Collections.Generic;
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
        private string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Quartz", "0_qtz.txt");

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
            if(x.FullPath.Contains("qtz.txt"))
                return;

            int _I = id;
            string _E = ("" + x.ChangeType);
            string _D = (x.FullPath + " was " + x.ChangeType);
            string _A = DateTime.Today.ToString("dd-MM-yyyy");
            string _T = DateTime.Now.ToString("HH:mm:ss tt");

            string ext = Path.GetExtension(x.FullPath);
            Dispatcher.Invoke(() => {
                if((bool)EnableFiltering.IsChecked)
                {
                    if((bool)FilterInclude.IsChecked)
                    {
                        if(Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _D, _A, _T);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _D, _A, _T);
                        }
                    }
                    else
                    {
                        if(!Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _D, _A, _T);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _D, _A, _T);
                        }
                    }
                }
                else
                {
                    Display(_I, _E, _D, _A, _T);
                    id++;
                    if((bool)EnableLogs.IsChecked)
                        WriteToLogs(_I, _E, _D, _A, _T);
                }
            });
        }

        private void OnRenamed(object source, RenamedEventArgs x)
        {
            if(x.FullPath.Contains("qtz.txt"))
                return;

            int _I = id;
            string _E = "Renamed";
            string _D = x.OldFullPath + " was Renamed to " + x.FullPath;
            string _A = DateTime.Today.ToString("dd-MM-yyyy");
            string _T = DateTime.Now.ToString("HH:mm:ss tt");

            string ext = Path.GetExtension(x.FullPath);
            Dispatcher.Invoke(() => {
                if((bool)EnableFiltering.IsChecked)
                {
                    if((bool)FilterInclude.IsChecked)
                    {
                        if(Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _D, _A, _T);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _D, _A, _T);
                        }
                    }
                    else
                    {
                        if(!Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _D, _A, _T);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _D, _A, _T);
                        }
                    }
                }
                else
                {
                    Display(_I, _E, _D, _A, _T);
                    id++;
                    if((bool)EnableLogs.IsChecked)
                        WriteToLogs(_I, _E, _D, _A, _T);
                }
            });
        }

        private void Display(int _I, string _E, string _D, string _A, string _T)
        {
            Dispatcher.Invoke(() =>
            {
                ScanResultDataGrid.Items.Add(new Record
                {
                    I = _I,
                    E = _E,
                    D = _D,
                    A = _A,
                    T = _T
                });
            });
        }

        private void SetPath()
        {
            bool skip = true;
            Dispatcher.Invoke(() => {
                if((bool)EnableLogs.IsChecked)
                    skip = false;
                if(skip)
                    return;

                string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if(Directory.Exists(LoggingDirectory.Text))
                    directory = LoggingDirectory.Text;

                string filename = String.Format("{0:dd-MM-yyyy}_{1:HH-mm-ss}_qtz.txt", DateTime.Now, DateTime.Now);
                path = Path.Combine(directory, "Quartz", filename);
            });
        }

        private async void WriteToLogs(int _I, string _E, string _D, string _A, string _T)
        {
            if(!File.Exists(path))
            {
                using(var str = new StreamWriter(path))
                {
                    await str.WriteLineAsync(
                    "[" + _I + "] " + _E.ToUpper() + " | " + _A + " | " + _T + " | " + _D
                    );
                    str.Flush();
                }
            }
            else
            {
                using(var str = new StreamWriter(path, true))
                {
                    await str.WriteLineAsync(
                    "[" + _I + "] " + _E.ToUpper() + " | " + _A + " | " + _T + " | " + _D
                    );
                    str.Flush();
                }
            }
        }

        private void Terminate()
        {
            watcher.Dispose();
            id = 0;
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
            if(stopWatch.IsRunning)
            {
                TimeSpan ts = stopWatch.Elapsed;
                currentTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds / 10);
                Dispatcher.Invoke(() =>
                {
                    TimeElapsed.Content = currentTime;
                });
            }
        }

        private void Tock()
        {
            if(stopWatch.IsRunning)
            {
                stopWatch.Stop();
                Dispatcher.Invoke(() => {
                    TimeElapsed.Content = "";
                });
            }
            
        }

        #endregion

        private void ClearData(object sender, RoutedEventArgs e)
        {
            ScanResultDataGrid.Items.Clear();
            ClearBtn.Visibility = Visibility.Collapsed;
        }

        private void StartLiveScan(object sender, RoutedEventArgs e)
        {
            LiveScanBtn.Content = "Stop scan";
            SetPath();
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
            if(ScanResultDataGrid.HasItems)
            {
                ClearBtn.Visibility = Visibility.Visible;
            }
            LiveScanBtn.Click -= StopLiveScan;
            LiveScanBtn.Click += StartLiveScan;
        }

        private void SavePreferences(object sender, RoutedEventArgs e)
        {

        }
    }
}
