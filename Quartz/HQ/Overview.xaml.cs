using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Overview.xaml
	/// </summary>
	public partial class Overview : Page
	{
		public Overview()
		{
			InitializeComponent();
			Debug.WriteLine("Loading graphs");
			SeriesCollection = new SeriesCollection
			{
				new LineSeries
				{
					Title = "GPU",
					Values = new ChartValues<double> {},
					PointGeometry = DefaultGeometries.Diamond,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "CPU",
					Values = new ChartValues<double> {},
					PointGeometry = DefaultGeometries.Circle,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "RAM",
					Values = new ChartValues<double> {},
					PointGeometry = DefaultGeometries.Square,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "DISK",
					Values = new ChartValues<double> {},
					PointGeometry = DefaultGeometries.Cross,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "NETWORK",
					Values = new ChartValues<double> {},
					PointGeometry = DefaultGeometries.Triangle,
					PointGeometrySize = 15
				}
			};

			Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May","june" };
			//YFormatter = value => value.ToString("C");


			//1:gpu 
			//2:cpu
			//3:ram
			//4:disk
			//5:network
			SeriesCollection[3].Values.Add(5d);

			DataContext = this;
		}

		public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

	}
}
