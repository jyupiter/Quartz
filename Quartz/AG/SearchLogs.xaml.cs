using System;
using System.Collections.Generic;
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

namespace Quartz.AG
{
    /// <summary>
    /// Interaction logic for SearchLogs.xaml
    /// </summary>
    public partial class SearchLogs : Page
    {
        public SearchLogs()
        {
            InitializeComponent();
            IDG();
        }

        #region SetUp

        private void IDG()
        {
            DataGridTextColumn indx = new DataGridTextColumn();
            DataGridTextColumn evnt = new DataGridTextColumn();
            DataGridTextColumn date = new DataGridTextColumn();
            DataGridTextColumn time = new DataGridTextColumn();
            DataGridTextColumn desc = new DataGridTextColumn();
            SearchResultDataGrid.Columns.Add(indx);
            SearchResultDataGrid.Columns.Add(evnt);
            SearchResultDataGrid.Columns.Add(date);
            SearchResultDataGrid.Columns.Add(time);
            SearchResultDataGrid.Columns.Add(desc);
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

        private void Display(int _I, string _E, string _A, string _T, string _D)
        {
            Dispatcher.Invoke(() =>
            {
                SearchResultDataGrid.Items.Add(new Record
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

        #region SearchLogs

        private void LoadFromLogs()
        {
            /*
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
            */
        }

        private void RetrieveFromLucene()
        {

        }

        #endregion

        private void SearchLogsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveSearchBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
