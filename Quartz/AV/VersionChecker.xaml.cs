using Quartz.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Quartz.AV
{
    /// <summary>
    /// Interaction logic for VersionChecker.xaml
    /// </summary>
    public partial class VersionChecker : Page
    {
        public VersionChecker()
        {
            InitializeComponent();
            LoadDataGridData();
        }

        private List<UpdateStuff> HellothereStranger()
        {
            List<UpdateStuff> applist = new List<UpdateStuff>();

            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine;
            string subKey1 =
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

            Microsoft.Win32.RegistryKey uinstallkey = regKey.OpenSubKey(subKey1);

            string[] subKeyNames = uinstallkey.GetSubKeyNames();



            foreach (string subKeyName in subKeyNames)
            {
                Microsoft.Win32.RegistryKey subKey2 = regKey.OpenSubKey(subKey1 + "\\" + subKeyName);

                if (ValueNameExists(subKey2.GetValueNames(), "DisplayName") &&
                ValueNameExists(subKey2.GetValueNames(), "DisplayVersion"))
                {
                    applist.Add(new UpdateStuff((string)subKey2.GetValue("DisplayName"), (string)subKey2.GetValue("DisplayVersion")));
                }

                subKey2.Close();
            }

            uinstallkey.Close();
            return applist;
        }


        private bool ValueNameExists(string[] valueNames, string valueName)
        {
            foreach (string s in valueNames)
            {
                if (s.ToLower() == valueName.ToLower()) return true;
            }

            return false;
        }

        private void DataGridFiller()
        {
            DataTable dt = new DataTable();
            DataColumn name = new DataColumn("Name", typeof(string));
            DataColumn version = new DataColumn("Version", typeof(string));

            dt.Columns.Add(name);
            dt.Columns.Add(version);

            // Input Data from HellothereStranger

            DatagridAvan1.ItemsSource = dt.DefaultView;
        }

        private void LoadDataGridData()
        {
            List<UpdateStuff> applist = HellothereStranger();

            DatagridAvan1.ItemsSource = applist;
        }

    }
}
//DatagridAvan1.Items.Add(new DataGridItem(new string[]{
//                    subKey2.GetValue("DisplayName").ToString(),
//                   subKey2.GetValue("DisplayVersion").ToString() }));