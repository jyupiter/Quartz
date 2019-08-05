using Quartz.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for ProcessRunner.xaml
    /// </summary>
    public partial class ProcessRunner : Page
    {
        string path = "..\\..\\..\\HQ\\Logs\\ProcessTimes.txt";

        public ProcessRunner()
        {
            InitializeComponent();
            GetStuffFromFile();
        }

        public void GetStuffFromFile()
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var ssr = new StreamReader(stream))
            {
                while (!ssr.EndOfStream)
                {
                    var line = ssr.ReadLine();
                    if (!ssr.EndOfStream)
                    {
                        var nextLine = ssr.ReadLine();
                        var repspl = nextLine.Split('|');
                        var name = repspl[0];
                        var startDate = repspl[1];
                        var runtime = repspl[2];
                        var EndDate = repspl[3];

                        foreach (String s in repspl)
                        {
                            Debug.WriteLine(s);
                        }
                    }
                }

            }


        }
        public static ArrayList GetProcessNames()
        {
            ArrayList currentProcesses = new ArrayList(Process.GetProcesses());
            ArrayList pcsNames = new ArrayList();
            foreach (System.Diagnostics.Process process in currentProcesses)
            {
                pcsNames.Add(process.ProcessName);
                Debug.WriteLine(process.ProcessName);
            }
            return pcsNames;
        }

        public static void GetProcessExitTimes(object arg)
        {
            string processs = (string)arg;
            System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName(processs);
            
            foreach (Process process in pname)
            { 
            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => { Debug.WriteLine("There has been some kind of exit"); };
        }
            // var isRunning = !process.HasExited;
            return;
        }

        public static void DisplayDatagrid(string name, TimeSpan startTime)
        {
            //DatagridAvan2.
            //
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Display Process Name, StartTime, Runtime here
            DatagridAvan2.ItemsSource = "{Binding UpdateStuff}";
            DataGridTextColumn ColName = new DataGridTextColumn();
            ColName.Header = "Process";
            ColName.Binding = new Binding("Name");

            DataGridTextColumn ColName1 = new DataGridTextColumn();
            ColName1.Header = "RunTime";
            ColName1.Binding = new Binding("RunTime");

        }
    }
}
