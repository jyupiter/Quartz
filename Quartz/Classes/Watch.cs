using Quartz.AG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Quartz.Classes
{
    class Watch
    {
        private static int id = 0;
        private static FileSystemWatcher watcher;
        private static string path = _Folder.BaseFolder;
        private static string F = FileWatcher.F;
        private static string targetDirectory = FileWatcher.targetDirectory;
        private static string filterExtensions = FileWatcher.filterExtensions;
        private static bool enableFiltering = FileWatcher.enableFiltering;
        private static bool filterInclude = FileWatcher.filterInclude;
        private static bool enableLogs = FileWatcher.enableLogs;
        public static int backLogSize = 0;

        public static List<Record> lr = new List<Record>();

        public struct Record
        {
            public int I { get; set; }
            public string E { get; set; }
            public string A { get; set; }
            public string T { get; set; }
            public string D { get; set; }
        }

        private static void Display(int _I, string _E, string _A, string _T, string _D)
        {
            lr.Add(new Record
            {
                I = _I,
                E = _E,
                A = _A,
                T = _T,
                D = _D
            });
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            string path = "C:\\";
            if(targetDirectory != null && targetDirectory.Length > 2)
                path = targetDirectory;

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

        private static void OnChanged(object source, FileSystemEventArgs x)
        {
            if(x.FullPath.Contains(".qtz"))
                return;

            int _I = id;
            string _E = "" + x.ChangeType;
            string _A = DateTime.Today.ToString("dd-MM-yyyy");
            string _T = DateTime.Now.ToString("HH:mm:ss tt");
            string _D = x.FullPath + " was " + x.ChangeType;

            string pattern = @"^(?:[\w]\:|\\)(\\[a-z_\-\s0-9\.]+)+\.(" + filterExtensions + ")$";

            if(enableFiltering)
            {
                if(filterInclude)
                {
                    if(Regex.IsMatch(x.FullPath, pattern, RegexOptions.IgnoreCase))
                    {
                        Display(_I, _E, _A, _T, _D);
                        id++;
                        if(enableLogs)
                            WriteToLogs(_I, _E, _A, _T, _D);
                    }
                }
                else
                {
                    if(!Regex.IsMatch(x.FullPath, pattern, RegexOptions.IgnoreCase))
                    {
                        Display(_I, _E, _A, _T, _D);
                        id++;
                        if(enableLogs)
                            WriteToLogs(_I, _E, _A, _T, _D);
                    }
                }
            }
            else
            {
                Display(_I, _E, _A, _T, _D);
                id++;
                if(enableLogs)
                    WriteToLogs(_I, _E, _A, _T, _D);
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs x)
        {
            if(x.FullPath.Contains(".qtz"))
                return;

            int _I = id;
            string _E = "Renamed";
            string _A = DateTime.Today.ToString("dd-MM-yyyy");
            string _T = DateTime.Now.ToString("HH:mm:ss tt");
            string _D = x.OldFullPath + " was Renamed to " + x.FullPath;

            string ext = Path.GetExtension(x.FullPath);
            if(enableFiltering)
            {
                if(filterInclude)
                {
                    if(Regex.IsMatch(ext, filterExtensions, RegexOptions.IgnoreCase))
                    {
                        Display(_I, _E, _A, _T, _D);
                        id++;
                        if(enableLogs)
                            WriteToLogs(_I, _E, _A, _T, _D);
                    }
                }
                else
                {
                    if(!Regex.IsMatch(ext, filterExtensions, RegexOptions.IgnoreCase))
                    {
                        Display(_I, _E, _A, _T, _D);
                        id++;
                        if(enableLogs)
                            WriteToLogs(_I, _E, _A, _T, _D);
                    }
                }
            }
            else
            {
                Display(_I, _E, _A, _T, _D);
                id++;
                if(enableLogs)
                    WriteToLogs(_I, _E, _A, _T, _D);
            }
        }

        private static async void WriteToLogs(int _I, string _E, string _A, string _T, string _D)
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

        public static void Terminate()
        {
            watcher.Dispose();
            id = 0;
        }


    }
}
