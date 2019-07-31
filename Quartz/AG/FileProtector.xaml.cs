using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.Security.AccessControl;

namespace Quartz.AG
{
    /// <summary>
    /// Interaction logic for FileProtector.xaml
    /// </summary>
    public partial class FileProtector : Page
    {
        public FileProtector()
        {
            InitializeComponent();
            IDG();
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

        #endregion

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

        private void Lock(string[] files)
        {

        }

        private void Unlock(string[] files)
        {

        }

        public void SetFileSecurity(string filePath, string domainName, string userName)
        {
            FileInfo fi = new FileInfo(filePath);//get file info
            FileSecurity fs = fi.GetAccessControl();//get security access
            fs.SetAccessRuleProtection(true, false);//remove any inherited access
            AuthorizationRuleCollection rules = fs.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));//get any special user access
            foreach(FileSystemAccessRule rule in rules)//remove any special access
                fs.RemoveAccessRule(rule);
            fs.AddAccessRule(new FileSystemAccessRule(".\\" + userName, FileSystemRights.FullControl, AccessControlType.Allow));//add current user with full control.
            File.SetAccessControl(filePath, fs);//flush security access.
        }

        private void SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] selected = FileDialog();
            if(selected == null)
                return;
            Lock(selected);
            foreach(string s in selected)
            {
                FileInfo f = new FileInfo(s);
                Display(f.Name, s, f.Extension, f.Length.ToString());
            }
        }

        private void DeleteFileBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
