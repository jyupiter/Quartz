using LiveCharts;
using LiveCharts.Wpf;
using Quartz.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace Quartz.AG
{
    /// <summary>
    /// Interaction logic for FileSummary.xaml
    /// </summary>
    public partial class FileSummary : Page
    {
        public FileSummary()
        {
            InitializeComponent();
            DisplayFiles();
            Graph();
        }

        public int[] changed = new int[24];
        public int[] renamed = new int[24];
        public int[] deleted = new int[24];
        public int[] created = new int[24];
        string path = _Folder.BaseFolder;
        string fileToUse = null;

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void DisplayFiles()
        {
            string[] fp = Directory.GetFiles(Path.Combine(path, "Quartz/Logs/"));
            foreach(string s in fp)
            {
                ComboBoxItem c = new ComboBoxItem
                {
                    Content = s,
                    ToolTip = s
                };
                SelectedFileComboBox.Items.Add(c);
            }
            SelectedFileComboBox.SelectedIndex = 0;
        }

        private void SelectedFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            ComboBoxItem s = (ComboBoxItem)c.SelectedItem;
            fileToUse = (string)s.Content;
            if(SeriesCollection == null)
                return;
            changed = new int[24];
            renamed = new int[24];
            deleted = new int[24];
            created = new int[24];
            CountEvents(ParseLogs(fileToUse));
            SeriesCollection[0].Values = new ChartValues<int>(changed);
            SeriesCollection[1].Values = new ChartValues<int>(renamed);
            SeriesCollection[2].Values = new ChartValues<int>(deleted);
            SeriesCollection[3].Values = new ChartValues<int>(created);
        }

        private void Graph()
        {
            CountEvents(ParseLogs(fileToUse));

            SeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Values = new ChartValues<int>(changed),
                    StackMode = StackMode.Values,
                    DataLabels = true
                },
            };
            SeriesCollection.Add(new StackedColumnSeries
            {
                Values = new ChartValues<int>(renamed),
                StackMode = StackMode.Values,
                DataLabels = true
            });
            SeriesCollection.Add(new StackedColumnSeries
            {
                Values = new ChartValues<int>(deleted),
                StackMode = StackMode.Values,
                DataLabels = true
            });
            SeriesCollection.Add(new StackedColumnSeries
            {
                Values = new ChartValues<int>(created),
                StackMode = StackMode.Values,
                DataLabels = true
            });
            SeriesCollection x = SeriesCollection;
            Labels = new[] { "Changed", "Renamed", "Deleted", "Created" };
            DataContext = this;
        }

        public List<Log> ParseLogs(string file)
        {
            if(file == null)
                return null;
            List<Log> l = new List<Log>();
            Log log;
            var target = Path.Combine(path, "Quartz/Quartz/Logs/", file);
            string[] f = File.ReadAllLines(target);
            foreach(string s in f)
            {
                string[] entry = s.Split('|');
                log = new Log(int.Parse(entry[0]), entry[1], entry[2], entry[3], entry[4]);
                l.Add(log);
            }
            return l;
    }

    public void CountEvents(List<Log> l)
        {
            if(l == null)
                return;
            changed = new int[24];
            renamed = new int[24];
            deleted = new int[24];
            created = new int[24];
            int hour;
            DateTime date = l[0].dateTime.Date;
            foreach(Log log in l)
            {
                if(log.dateTime.Date == date)
                {
                    hour = log.dateTime.Hour;
                    switch(log.changes)
                    {
                        case "CHANGED":
                        changed[hour]++;
                        break;

                        case "RENAMED":
                        renamed[hour]++;
                        break;

                        case "DELETED":
                        deleted[hour]++;
                        break;

                        case "CREATED":
                        deleted[hour]++;
                        break;
                    }
                }
            }
        }
    }

}
