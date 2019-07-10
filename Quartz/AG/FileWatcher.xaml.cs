using Quartz.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            InitializePreferences();
            LoadFromPreferences();
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
        private string path = _Folder.BaseFolder;
        private string F = "";
        private string P = "";
        
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
            if(x.FullPath.Contains(".qtz"))
                return;

            int _I = id;
            string _E = "" + x.ChangeType;
            string _D = x.FullPath + " was " + x.ChangeType;
            string _A = DateTime.Today.ToString("dd-MM-yyyy");
            string _T = DateTime.Now.ToString("HH:mm:ss tt");

            Dispatcher.Invoke(() => {
                string pattern = @"^(?:[\w]\:|\\)(\\[a-z_\-\s0-9\.]+)+\.(" + FilterExtensions.Text + ")$";

                if((bool)EnableFiltering.IsChecked)
                {
                    if((bool)FilterInclude.IsChecked)
                    {
                        if(Regex.IsMatch(x.FullPath, pattern, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _D, _A, _T);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _D, _A, _T);
                        }
                    }
                    else
                    {
                        if(!Regex.IsMatch(x.FullPath, pattern, RegexOptions.IgnoreCase))
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
            if(x.FullPath.Contains(".qtz"))
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

        // Environment

        private void SetPath()
        {
            bool skip = true;
            Dispatcher.Invoke(() => {
                if((bool)EnableLogs.IsChecked)
                    skip = false;
                if(skip)
                    return;
                
                if(Directory.Exists(LoggingDirectory.Text))
                    path = LoggingDirectory.Text;
                
                Directory.CreateDirectory(Path.Combine(path, "Quartz/Logs"));

                string filename = string.Format("{0:dd-MM-yyyy}_{1:HH-mm-ss}.qtz", DateTime.Now, DateTime.Now);
                F = Path.Combine(path, "Quartz/Logs", filename);
            });
        }

        // User-facing

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

        private async void WriteToLogs(int _I, string _E, string _D, string _A, string _T)
        {
            if(!File.Exists(F))
            {
                using(var str = new StreamWriter(F))
                {
                    await str.WriteLineAsync(
                        _I + "|" + _E.ToUpper() + "|" + _A + "|" + _T + "|" + _D
                    );
                    str.Flush();
                }
            }
            else
            {
                using(var str = new StreamWriter(F, true))
                {
                    await str.WriteLineAsync(
                        _I + "|" + _E.ToUpper() + "|" + _A + "|" + _T + "|" + _D
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

        private void FolderDialog(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = "C:\\Users",
                IsFolderPicker = true
            };
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TextBox x = (TextBox)sender;
                x.Text = dialog.FileName;
            }
        }

        #endregion

        #region Settings

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            WriteToPreferences();
        }

        private void InitializePreferences()
        {
            Directory.CreateDirectory(Path.Combine(path, "Quartz/Preferences"));
            P = Path.Combine(path, "Quartz/Preferences", "filewatcher.qonf");
        }

        private async void WriteToPreferences()
        {
            using(var str = new StreamWriter(P))
            {
                await str.WriteLineAsync(
                    "TargetDirectory=[" + TargetDirectory.Text + "]\n" +
                    "FilterEnable=[" + EnableFiltering.IsChecked + "]\n" +
                    "FilterExtensions=[" + FilterExtensions.Text + "]\n" +
                    "FilterInclude=[" + FilterInclude.IsChecked + "]\n" +
                    "LogsDirectory=[" + LoggingDirectory.Text + "]\n" +
                    "LogsEnable=[" + EnableLogs.IsChecked + "]"
                );
                str.Flush();
            }
        }

        private void LoadFromPreferences()
        {
            string[] raw = File.ReadAllLines(P);
            List<string> l = new List<string>();

            foreach(string s in raw)
                l.Add(Regex.Match(s, @"\[([^)]*)\]").Groups[1].Value);
            try
            {
                Dispatcher.Invoke(() =>
                {
                    TargetDirectory.Text = l[0];
                    EnableFiltering.IsChecked = bool.Parse(l[1]);
                    FilterExtensions.Text = l[2];
                    FilterInclude.IsChecked = bool.Parse(l[3]);
                    FilterExclude.IsChecked = !bool.Parse(l[3]);
                    LoggingDirectory.Text = l[4];
                    EnableLogs.IsChecked = bool.Parse(l[5]);
                });
            }
            catch(Exception)
            {
                //
            }
        }

        #endregion

        private void StartLiveScan(object sender, RoutedEventArgs e)
        {
            LiveScanBtn.Content = "Stop scan";
            SetPath();
            Run();
            LiveScanBtn.Click -= StartLiveScan;
            LiveScanBtn.Click += StopLiveScan;
        }

        private void StopLiveScan(object sender, RoutedEventArgs e)
        {
            LiveScanBtn.Content = "Start scan";
            Terminate();
            if(ScanResultDataGrid.HasItems)
            {
                ClearBtn.Visibility = Visibility.Visible;
            }
            LiveScanBtn.Click -= StopLiveScan;
            LiveScanBtn.Click += StartLiveScan;
        }

        private void ClearData(object sender, RoutedEventArgs e)
        {
            ScanResultDataGrid.Items.Clear();
            ClearBtn.Visibility = Visibility.Collapsed;
        }
    }
}
