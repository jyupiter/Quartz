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
using NvAPIWrapper;
using NvAPIWrapper.GPU;
using System.Threading;

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
					Values = new ChartValues<double> {1.0},
					PointGeometry = DefaultGeometries.Diamond,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "CPU",
					Values = new ChartValues<double> {1.0},
					PointGeometry = DefaultGeometries.Circle,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "RAM",
					Values = new ChartValues<double> {1.0},
					PointGeometry = DefaultGeometries.Square,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "DISK",
					Values = new ChartValues<double> {1.0},
					PointGeometry = DefaultGeometries.Cross,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "NETWORK",
					Values = new ChartValues<double> {1.0},
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
			Init();

			DataContext = this;
		}
		public static void UpdateGraphs(int index, double value)
		{
			//Debug.WriteLine(index + "||" + value);
			SeriesCollection[index].Values.Add(value);

		}

		public static void Init()
		{
			//new Thread(GetCpu).Start();
			new Thread(GetGpu).Start();
			//new Thread(GetRam).Start();
			//new Thread(GetDisk).Start();
			//new Thread(GetNetwork).Start();
		}

		public static void GetGpu()
		{
			var GPUs = PhysicalGPU.GetPhysicalGPUs();
			while (true)
			{
				//Debug.WriteLine("\n<-----					----->");
				//Debug.WriteLine(GPUs[0].FullName);
				//Debug.WriteLine(GPUs[0].MemoryInformation);
				//Debug.WriteLine(GPUs[0].ThermalInformation.CurrentThermalLevel);
				//Debug.WriteLine(GPUs[0].UsageInformation.GPU);
				//Debug.WriteLine("<-----					----->\n");
				UpdateGraphs(0, GPUs[0].UsageInformation.GPU.Percentage);
				Thread.Sleep(100);
			}
		}


		public static void GetCpu()
		{

		}

		public static void GetRam()
		{

		}

		public static void GetDisk()
		{

		}

		public static void GetNetwork()
		{

		}

		public static SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

	}
}
