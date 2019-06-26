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
	/// Interaction logic for Page1.xaml
	/// </summary>
	public partial class Cpu : Page
	{
		public Cpu()
		{
			LoadGraph();
			InitializeComponent();
			SeriesCollection SeriesCollection = new SeriesCollection
			{
				new LineSeries
				{
					Values = new ChartValues<decimal> { 3, 5, 7, 4 }
				},
				new ColumnSeries
				{
					Values = new ChartValues<decimal> { 5, 6, 2, 7 }
				}
			};

		}
		public static void LoadGraph()
		{
			Debug.WriteLine("Loading graphs");
			
		}
	}
}
