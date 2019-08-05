using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.Security.AccessControl;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Quartz.Classes;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Quartz.AG
{
    /// <summary>
    /// Interaction logic for FileProtector.xaml
    /// </summary>
    /// 
    public partial class FileProtector : Page
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
                                            int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        public FileProtector()
        {
            InitializeComponent();
            try {
                CreateAccount();
                ConditionDrives();
            } catch(Exception) { }
            IDG();
            SetPath();
            LoadFromLocked();
        }

        #region SetUp

        private void IDG()
        {
            DataGridTextColumn name = new DataGridTextColumn();
            DataGridTextColumn path = new DataGridTextColumn();
            DataGridTextColumn type = new DataGridTextColumn();
            DataGridTextColumn size = new DataGridTextColumn();
            ProtectedFilesDataGrid.Columns.Add(name);
            ProtectedFilesDataGrid.Columns.Add(path);
            ProtectedFilesDataGrid.Columns.Add(type);
            ProtectedFilesDataGrid.Columns.Add(size);
            name.Header = "File";
            path.Header = "Path";
            type.Header = "Type";
            size.Header = "Size";
            name.Binding = new Binding("N");
            path.Binding = new Binding("P");
            type.Binding = new Binding("T");
            size.Binding = new Binding("S");
        }

        private struct Record
        {
            public string N { get; set; }
            public string P { get; set; }
            public string T { get; set; }
            public string S { get; set; }
        }

        private void Display(string _N, string _P, string _T, string _S)
        {
            Dispatcher.Invoke(() =>
            {
                ProtectedFilesDataGrid.Items.Add(new Record
                {
                    N = _N,
                    P = _P,
                    T = _T,
                    S = _S
                });
            });
        }

        private void SetPath()
        {
            string path = _Folder.BaseFolder;
            Directory.CreateDirectory(Path.Combine(path, "Quartz/Locked"));
            F = Path.Combine(path, "Quartz/Locked");
        }

        private void ExecuteCommand(string args)
        {
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = @"C:\Windows\System32\cmd.exe";
            si.Verb = "runas";
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            si.Arguments = args;

            Process cmdProcess = new Process();
            cmdProcess.StartInfo = si;
            cmdProcess.Start();
        }

        private void CreateAccount()
        {
            ExecuteCommand("/C net user quartz q413zac612l /add &" +
                               "net localgroup administrators quartz /add &" +
                               "net localgroup users quartz /delete");
        }

        private void ConditionDrives()
        {
            foreach(DriveInfo drive in DriveInfo.GetDrives())
            {
                if(drive.IsReady)
                {
                    string x = "\"" + drive.Name + "\"";
                    ExecuteCommand("/C icacls " + x + " /grant quartz:(OI)(CI)F");
                }
            }
        }

        private string[] FileDialog()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = "Desktop",
                Multiselect = true
            };
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileNames.ToArray();
            }
            return null;
        }

        #endregion

        #region FileProtector

        private string originalOwner;
        private List<FileSystemAccessRule> originalPermissions;
        private object[] original;
        private List<object[]> held = new List<object[]>();
        private string F = "";

        private void Lock(string[] files, bool _lock)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;
            string dn = Environment.UserDomainName,
                   un = "quartz",
                   pw = _Files.pw;

            bool returnValue = LogonUser(un, dn, pw, LOGON32_LOGON_INTERACTIVE,
                                         LOGON32_PROVIDER_DEFAULT,
                                         out SafeAccessTokenHandle safeAccessTokenHandle);
            if(!returnValue)
                return;

            WindowsIdentity.RunImpersonated(
                safeAccessTokenHandle,
                () => {
                    foreach(string s in files)
                    {
                        SetFileOwner(s, dn, un, _lock);
                        SetFileSecurity(s, dn, un, _lock);
                        if(!_lock)
                            SaveToLocked(s);
                    }
                });
        }

        private void SetFileOwner(string path, string dn, string un, bool _lock)
        {
            IdentityReference owner = new NTAccount(dn + "\\" + un);
            if(!_lock)
                foreach(object[] o in held)
                    if((string)o[1] == path)
                    {
                        owner = new NTAccount((string)o[1]);
                        break;
                    }
            FileInfo fi = new FileInfo(path);
            FileSecurity fs = fi.GetAccessControl();
            string x = fs.GetOwner(typeof(NTAccount)).ToString();
            originalOwner = x;
            fs.SetOwner(owner);
            fi.SetAccessControl(fs);
        }

        public void SetFileSecurity(string path, string dn, string un, bool _lock)
        {
            string s = Environment.UserName;
            FileInfo fi = new FileInfo(path);
            FileSecurity fs = fi.GetAccessControl();
            if(_lock)
            {
                originalPermissions = new List<FileSystemAccessRule>();
                fs.SetAccessRuleProtection(true, false);
                AuthorizationRuleCollection rules = fs.GetAccessRules(true, true, typeof(NTAccount));
                foreach(FileSystemAccessRule rule in rules)
                {
                    originalPermissions.Add(rule);
                    fs.RemoveAccessRule(rule);
                }
                fs.AddAccessRule(new FileSystemAccessRule(dn + "\\" + un, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(path, fs);
            }
            else
            {
                if(!(held.Count < 1))
                    foreach(object[] o in held)
                        if((string)o[0] == path)
                        {
                            JArray j = (JArray)o[2];
                            List<FileSystemAccessRule> l = j.ToObject<List<FileSystemAccessRule>>();
                            if(l == null || l.Count < 1)
                                break;
                            foreach(FileSystemAccessRule p in (List<FileSystemAccessRule>)o[2])
                                fs.AddAccessRule(p);
                            break;
                        }
                fs.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(path, fs);
            }
        }

        private async void SaveToLocked(string path)
        {
            string dest = Path.Combine(F, Path.GetFileName(path));
            original = new object[3];
            original[0] = path;
            string y = originalOwner;
            original[1] = originalOwner;
            original[2] = originalPermissions;
            string x = JsonConvert.SerializeObject(original);
            try
            {
                using(var str = new StreamWriter(dest))
                {
                    await str.WriteLineAsync(x);
                    str.Flush();
                }
            } catch(Exception){ }
        }

        private void RemoveFromLocked(string[] p)
        {
            foreach(string s in p)
            {
                string file = Path.Combine(F, Path.GetFileName(s));
                File.Delete(file);
                Dispatcher.Invoke(() =>
                {
                    for(int i = 0; i < ProtectedFilesDataGrid.Items.Count; i++)
                    {
                        Record r = (Record)ProtectedFilesDataGrid.Items.GetItemAt(i);
                        if(r.P == s)
                        {
                            ProtectedFilesDataGrid.Items.RemoveAt(i);
                            break;
                        }
                            
                    }
                });
            }
        }

        private void LoadFromLocked()
        {
            if(!Directory.EnumerateFiles(F).Any())
                return;
            string[] files = Directory.GetFiles(F);

            foreach(string f in files)
            {
                string[] raw = File.ReadAllLines(f);
                object[] o;
                o = JsonConvert.DeserializeObject<object[]>(raw[0]);
                held.Add(o);
                string oPath = (string)o[0];
                FileInfo fi = new FileInfo(oPath);
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        Display(fi.Name, oPath, fi.Extension, fi.Length.ToString());
                    });
                }
                catch(Exception) { }
            }
        }

        #endregion

        private void SelectFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] selected = FileDialog();
            if(selected == null)
                return;

            try { Lock(selected, true); } catch(Exception) { }
            
            foreach(string s in selected)
            {
                FileInfo f = new FileInfo(s);
                Display(f.Name, s, f.Extension, f.Length.ToString());
            }
        }

        private void UnlockFileBtn_Click(object sender, RoutedEventArgs e)
        {
            Record x = (Record)ProtectedFilesDataGrid.SelectedItem;
            string[] y = { x.P };
            try { Lock(y, false); RemoveFromLocked(y); } catch(Exception) { }
        }

        private void ProtectedFilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UnlockFileBtn.Visibility = Visibility.Visible;
        }
    }
}
