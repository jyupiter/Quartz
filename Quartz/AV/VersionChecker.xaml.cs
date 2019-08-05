using Quartz.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

namespace Quartz.AV
{
    /// <summary>
    /// Interaction logic for VersionChecker.xaml
    /// </summary>
    public partial class VersionChecker : Page
    {
        string path = "..\\..\\..\\AV\\dB.txt";
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
                "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

            Microsoft.Win32.RegistryKey uinstallkey = regKey.OpenSubKey(subKey1);

            string[] subKeyNames = uinstallkey.GetSubKeyNames();



            foreach (string subKeyName in subKeyNames)
            {
                Microsoft.Win32.RegistryKey subKey2 = regKey.OpenSubKey(subKey1 + "\\" + subKeyName);

                if (ValueNameExists(subKey2.GetValueNames(), "DisplayName") &&
                ValueNameExists(subKey2.GetValueNames(), "DisplayVersion"))
                {
                    string appName = (string)subKey2.GetValue("DisplayName");
                    string appVersion = (string)subKey2.GetValue("DisplayVersion");
                    Console.WriteLine("Name:{0}, Version{1}", appName, appVersion);
                    UpdateStuff game = new UpdateStuff(appName, appVersion);
                    applist.Add(game);

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

            DatagridAvan1.ItemsSource = "{Binding UpdateStuff}";
            DataGridTextColumn ColName = new DataGridTextColumn();
            ColName.Header = "Name";
            ColName.Binding = new Binding("AppName");
            
            DataGridTextColumn ColName1 = new DataGridTextColumn();
            ColName1.Header = "Version";
            ColName1.Binding = new Binding("AppVers");

            DatagridAvan1.Columns.Add(ColName);
            DatagridAvan1.Columns.Add(ColName1);
            DatagridAvan1.ItemsSource = applist;
            for (int i = 0; i < applist.Count; i++)
            {
                UpdateStuff hello = applist[i];
                string Appsnames = hello.AppName;
                string appverss = hello.AppVers;
                
                
            }
            CompareWith(applist);

        }

        private bool CompareWith(List<UpdateStuff> applist)
        {
            // Definition of this Function is to call and compare
            bool FullyUpdated = false;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var ssr = new StreamReader(stream))
            {
                while (!ssr.EndOfStream)
                {
                    var line = ssr.ReadLine();
                    if (!ssr.EndOfStream)
                    {
                        var repspl = line.Split('|');
                        var name = repspl[0];
                        var Newversion = repspl[1];
                            try
                                {
                            var matches = applist.Where(p => p.AppName == name).ToList();

                            if (matches != null)
                            {
                                if (matches[0].AppVers == Newversion)
                                {
                                    FullyUpdated = true;
                                    Debug.WriteLine("IT IS TRUE.");
                                } else { FullyUpdated = false;  Debug.WriteLine("IT IS FALSE."); }

                            }

                                    }
                            catch (Exception e)
                                {
                            Debug.Write(e);
                                    }


                    }
                }

                return FullyUpdated;

            }



        }
    }
}
