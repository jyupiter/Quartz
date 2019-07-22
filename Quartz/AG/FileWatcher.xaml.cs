using Quartz.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using Lucene.Net.Analysis.Standard;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using System.Diagnostics;

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
            DataGridTextColumn date = new DataGridTextColumn();
            DataGridTextColumn time = new DataGridTextColumn();
            DataGridTextColumn desc = new DataGridTextColumn();
            ScanResultDataGrid.Columns.Add(indx);
            ScanResultDataGrid.Columns.Add(evnt);
            ScanResultDataGrid.Columns.Add(date);
            ScanResultDataGrid.Columns.Add(time);
            ScanResultDataGrid.Columns.Add(desc);
            indx.Header = "#";
            evnt.Header = "Event";
            date.Header = "Date";
            time.Header = "Time";
            desc.Header = "Description";
            indx.Binding = new Binding("I");
            evnt.Binding = new Binding("E");
            date.Binding = new Binding("A");
            time.Binding = new Binding("T");
            desc.Binding = new Binding("D");
        }

        private struct Record
        {
            public int I { get; set; }
            public string E { get; set; }
            public string A { get; set; }
            public string T { get; set; }
            public string D { get; set; }
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
                System.IO.Directory.CreateDirectory(Path.Combine(path, "Quartz/Logs"));

                string filename = string.Format("{0:dd-MM-yyyy}_{1:HH-mm-ss}.qtz", DateTime.Now, DateTime.Now);
                F = Path.Combine(path, "Quartz/Logs", filename);
            });
        }

        // User-facing

        private void Display(int _I, string _E, string _A, string _T, string _D)
        {
            Dispatcher.Invoke(() =>
            {
                ScanResultDataGrid.Items.Add(new Record
                {
                    I = _I,
                    E = _E,
                    A = _A,
                    T = _T,
                    D = _D
                });
            });
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
            string _A = DateTime.Today.ToString("dd-MM-yyyy");
            string _T = DateTime.Now.ToString("HH:mm:ss tt");
            string _D = x.FullPath + " was " + x.ChangeType;

            Dispatcher.Invoke(() => {
                string pattern = @"^(?:[\w]\:|\\)(\\[a-z_\-\s0-9\.]+)+\.(" + FilterExtensions.Text + ")$";

                if((bool)EnableFiltering.IsChecked)
                {
                    if((bool)FilterInclude.IsChecked)
                    {
                        if(Regex.IsMatch(x.FullPath, pattern, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _A, _T, _D);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _A, _T, _D);
                        }
                    }
                    else
                    {
                        if(!Regex.IsMatch(x.FullPath, pattern, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _A, _T, _D);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _A, _T, _D);
                        }
                    }
                }
                else
                {
                    Display(_I, _E, _A, _T, _D);
                    id++;
                    if((bool)EnableLogs.IsChecked)
                        WriteToLogs(_I, _E, _A, _T, _D);
                }
            });
        }

        private void OnRenamed(object source, RenamedEventArgs x)
        {
            if(x.FullPath.Contains(".qtz"))
                return;

            int _I = id;
            string _E = "Renamed";
            string _A = DateTime.Today.ToString("dd-MM-yyyy");
            string _T = DateTime.Now.ToString("HH:mm:ss tt");
            string _D = x.OldFullPath + " was Renamed to " + x.FullPath;

            string ext = Path.GetExtension(x.FullPath);
            Dispatcher.Invoke(() => {
                if((bool)EnableFiltering.IsChecked)
                {
                    if((bool)FilterInclude.IsChecked)
                    {
                        if(Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _A, _T, _D);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _A, _T, _D);
                        }
                    }
                    else
                    {
                        if(!Regex.IsMatch(ext, FilterExtensions.Text, RegexOptions.IgnoreCase))
                        {
                            Display(_I, _E, _A, _T, _D);
                            id++;
                            if((bool)EnableLogs.IsChecked)
                                WriteToLogs(_I, _E, _A, _T, _D);
                        }
                    }
                }
                else
                {
                    Display(_I, _E, _A, _T, _D);
                    id++;
                    if((bool)EnableLogs.IsChecked)
                        WriteToLogs(_I, _E, _A, _T, _D);
                }
            });
        }

        private async void WriteToLogs(int _I, string _E, string _A, string _T, string _D)
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
            System.IO.Directory.CreateDirectory(Path.Combine(path, "Quartz/Preferences"));
            P = Path.Combine(path, "Quartz/Preferences", "filewatcher.qonf");
            if(!File.Exists(P))
                File.Copy(_Folder.PTemplate, P, true);
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
                    EnableLogs.IsChecked = bool.Parse(l[4]);
                });
            }
            catch(Exception)
            {
                //
            }
        }

        #endregion

        #region Lucene

        private void IndexToLucene()
        {
            Lucene.Net.Store.Directory dir = FSDirectory.Open(Path.Combine(path,"Lucene"));
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using(var writer = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                string[] raw = File.ReadAllLines(F);
                foreach(var line in raw)
                {
                    string[] entry = line.Split('|');

                    var id = new Field("id", entry[0], Field.Store.YES, Field.Index.NOT_ANALYZED);
                    var ev = new Field("event", entry[1], Field.Store.YES, Field.Index.ANALYZED);
                    var dt = new Field("date", entry[2], Field.Store.YES, Field.Index.ANALYZED);
                    var tm = new Field("time", entry[3], Field.Store.YES, Field.Index.ANALYZED);
                    var ds = new Field("description", entry[4], Field.Store.YES, Field.Index.ANALYZED);
               
                    var document = new Document();
                    document.Add(id);
                    document.Add(ev);
                    document.Add(dt);
                    document.Add(tm);
                    document.Add(ds);

                    writer.AddDocument(document);
                }

                writer.Optimize();
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
            IndexToLucene();
        }

        private void ClearData(object sender, RoutedEventArgs e)
        {
            ScanResultDataGrid.Items.Clear();
            ClearBtn.Visibility = Visibility.Collapsed;
        }
    }
}
