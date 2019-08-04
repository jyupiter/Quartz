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
        public int[] changed = new int[24];
        public int[] renamed = new int[24];
        public int[] deleted = new int[24];
        public int[] created = new int[24];

        public FileSummary()
        {
            string fileToUse = "12-07-2019_12-41-15.qtz";
            InitializeComponent();
            
            countEvents(parseLogs(fileToUse));

            SeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Values = new ChartValues<int>(changed),
                    StackMode = StackMode.Values, // this is not necessary, values is the default stack mode
                    DataLabels = true
                },
            };

            //adding series updates and animates the chart
            SeriesCollection.Add(new StackedColumnSeries
            {
                Values = new ChartValues<int>(renamed),
                StackMode = StackMode.Values
            });
            SeriesCollection.Add(new StackedColumnSeries
            {
                Values = new ChartValues<int>(deleted),
                StackMode = StackMode.Values
            });
            SeriesCollection.Add(new StackedColumnSeries
            {
                Values = new ChartValues<int>(created),
                StackMode = StackMode.Values
            });

            //adding values also updates and animates
            //SeriesCollection[2].Values.Add(4d);

            Labels = new[] { "Changed", "Renamed", "Deleted","Created"};
            //Formatter = value => value + " Mill";

            DataContext = this;
        }

        public List<Log> parseLogs(string file)
        {
            string path = _Folder.BaseFolder;
            List<Log> logs = new List<Log>();
            Log log;
            var logDir = Path.Combine(path, "Quartz/Logs/");
            string[] logFile = System.IO.File.ReadAllLines(logDir + file);
            foreach(string line in logFile)
            {
                string[] entry = line.Split('|');
                log = new Log(Int32.Parse(entry[0]), entry[1], entry[2], entry[3], entry[4]);
                logs.Add(log);
            }
            return logs;
    }

    public void countEvents(List<Log> logs)
        {
            changed = new int[24];
            renamed = new int[24];
            deleted = new int[24];
            created = new int[24];
            int hour;
            DateTime date = logs[0].dateTime.Date;
            foreach(Log log in logs)
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
                    Debug.WriteLine(log.dateTime.Hour);

                }

            }
            Debug.WriteLine(changed);
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

    }

}
