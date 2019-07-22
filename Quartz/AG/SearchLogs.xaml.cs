using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Quartz.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Version = Lucene.Net.Util.Version;

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
            DataGridTextColumn mtch = new DataGridTextColumn();
            DataGridTextColumn indx = new DataGridTextColumn();
            DataGridTextColumn evnt = new DataGridTextColumn();
            DataGridTextColumn date = new DataGridTextColumn();
            DataGridTextColumn time = new DataGridTextColumn();
            DataGridTextColumn desc = new DataGridTextColumn();
            SearchResultDataGrid.Columns.Add(mtch);
            SearchResultDataGrid.Columns.Add(indx);
            SearchResultDataGrid.Columns.Add(evnt);
            SearchResultDataGrid.Columns.Add(date);
            SearchResultDataGrid.Columns.Add(time);
            SearchResultDataGrid.Columns.Add(desc);
            mtch.Header = "Match";
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
            public string M { get; set; }
            public int I { get; set; }
            public string E { get; set; }
            public string A { get; set; }
            public string T { get; set; }
            public string D { get; set; }
        }

        private void Display(string _M, int _I, string _E, string _A, string _T, string _D)
        {
            Dispatcher.Invoke(() =>
            {
                SearchResultDataGrid.Items.Add(new Record
                {
                    M = _M,
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

        string path = _Folder.BaseFolder;

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

        private string[] Keywords()
        {
            string[] k = { "" };
            Dispatcher.Invoke(() =>
            {
                string key = SearchKeywords.Text;
                key = Regex.Replace(key, @"\s", "");
                k = key.Split('|');
            });
            return k;
        }

        private void RetrieveFromLucene()
        {
            Lucene.Net.Store.Directory dir = FSDirectory.Open(Path.Combine(path, "Lucene"));
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using(var indexSearcher = new IndexSearcher(dir))
            {
                var queryParser = new QueryParser(Version.LUCENE_30, "description", analyzer);

                if(Keywords().Length < 1)
                    return;

                foreach(var term in Keywords())
                {
                    Query query = queryParser.Parse(term);
                    TopDocs hits = indexSearcher.Search(query, null, 100);

                    foreach(ScoreDoc scoreDoc in hits.ScoreDocs)
                    {
                        Document document = indexSearcher.Doc(scoreDoc.Doc);
                        
                        string __M = document.Get("score");
                        int __I = int.Parse(document.Get("id"));
                        string __E = document.Get("event");
                        string __A = document.Get("date");
                        string __T = document.Get("time");
                        string __D = document.Get("description");
                        Display(__M, __I, __E, __A, __T, __D);
                    }
                }
            }
        }

        #endregion

        private void SearchLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            RetrieveFromLucene();
        }

        private void SaveSearchBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
