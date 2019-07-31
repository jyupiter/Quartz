using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Quartz.Classes;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using Version = Lucene.Net.Util.Version;
using System.Collections.Generic;

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
            DataGridTextColumn keyw = new DataGridTextColumn();
            DataGridTextColumn indx = new DataGridTextColumn();
            DataGridTextColumn evnt = new DataGridTextColumn();
            DataGridTextColumn date = new DataGridTextColumn();
            DataGridTextColumn time = new DataGridTextColumn();
            DataGridTextColumn desc = new DataGridTextColumn();
            SearchResultDataGrid.Columns.Add(keyw);
            SearchResultDataGrid.Columns.Add(indx);
            SearchResultDataGrid.Columns.Add(evnt);
            SearchResultDataGrid.Columns.Add(date);
            SearchResultDataGrid.Columns.Add(time);
            SearchResultDataGrid.Columns.Add(desc);
            keyw.Header = "Keyword";
            indx.Header = "#";
            evnt.Header = "Event";
            date.Header = "Date";
            time.Header = "Time";
            desc.Header = "Description";
            keyw.Binding = new Binding("K");
            indx.Binding = new Binding("I");
            evnt.Binding = new Binding("E");
            date.Binding = new Binding("A");
            time.Binding = new Binding("T");
            desc.Binding = new Binding("D");
        }

        private struct Record
        {
            public string K { get; set; }
            public int I { get; set; }
            public string E { get; set; }
            public string A { get; set; }
            public string T { get; set; }
            public string D { get; set; }
        }

        private void Display(string _K, int _I, string _E, string _A, string _T, string _D)
        {
            Dispatcher.Invoke(() =>
            {
                SearchResultDataGrid.Items.Add(new Record
                {
                    K = _K,
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

        private string[] Keywords()
        {
            string[] k = {};
            Dispatcher.Invoke(() =>
            {
                string key = SearchKeywords.Text;
                key = Regex.Replace(key, @"\s", "");
                k = key.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            });
            return k;
        }

        private object[,] Checkboxes()
        {
            object[,] c = {
                { "created", true },
                { "renamed", true },
                { "changed", true },
                { "deleted", true}
            };
            Dispatcher.Invoke(() =>
            {
                c[0,1] = (bool)CreateLogShow.IsChecked;
                c[1,1] = (bool)RenameLogShow.IsChecked;
                c[2,1] = (bool)UpdateLogShow.IsChecked;
                c[3,1] = (bool)DeleteLogShow.IsChecked;
            });
            return c;
        }

        private bool NotAllHidden()
        {
            return (bool)CreateLogShow.IsChecked || (bool)RenameLogShow.IsChecked || (bool)UpdateLogShow.IsChecked || (bool)DeleteLogShow.IsChecked;
        }

        private void RetrieveFromLucene()
        {
            Lucene.Net.Store.Directory dir = FSDirectory.Open(Path.Combine(path, "Lucene"));
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            try
            {
                using(var indexSearcher = new IndexSearcher(dir))
                {
                    var queryParser = new QueryParser(Version.LUCENE_30, "description", analyzer);

                    List<string> kw = Keywords().ToList();

                    if(kw.Count < 1)
                    {
                        Query query = new MatchAllDocsQuery();
                        TopDocs hits = indexSearcher.Search(query, null, 100);

                        foreach(ScoreDoc scoreDoc in hits.ScoreDocs)
                        {
                            Document document = indexSearcher.Doc(scoreDoc.Doc);

                            if(IsMatchBad(document.Get("event")))
                                continue;

                            string __K = "ALL";
                            int __I = int.Parse(document.Get("id"));
                            string __E = document.Get("event");
                            string __A = document.Get("date");
                            string __T = document.Get("time");
                            string __D = document.Get("description");
                            Display(__K, __I, __E, __A, __T, __D);

                        }
                    }
                    else
                    {
                        foreach(var term in kw)
                        {
                            Query query = queryParser.Parse(term);
                            TopDocs hits = indexSearcher.Search(query, null, 100);

                            foreach(ScoreDoc scoreDoc in hits.ScoreDocs)
                            {
                                Document document = indexSearcher.Doc(scoreDoc.Doc);

                                if(IsMatchBad(document.Get("event")))
                                    continue;

                                string __K = term;
                                int __I = int.Parse(document.Get("id"));
                                string __E = document.Get("event");
                                string __A = document.Get("date");
                                string __T = document.Get("time");
                                string __D = document.Get("description");
                                Display(__K, __I, __E, __A, __T, __D);

                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Index files not found.");
            }
        }

        private bool IsMatchBad(string ev)
        {
            object[,] ck = Checkboxes();
            for(int i = 0; i < 4; i++)
            {
                string name = (string)ck[i, 0];
                bool ismatch = name == ev.ToLower(),
                     asleep = !(bool)ck[i, 1];
                if(ismatch && asleep)
                    return true;
            }
            return false;
        }

        #endregion

        private void SearchLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchResultDataGrid.Items.Clear();
            if(NotAllHidden())
                RetrieveFromLucene();
            else
                MessageBox.Show("All event types are hidden.");
        }
    }
}
