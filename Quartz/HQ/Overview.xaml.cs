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
using LiveCharts.Configurations;
using Quartz.Classes;
using System.ComponentModel;

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Overview.xaml
	/// </summary>
	public partial class Overview : Page, INotifyPropertyChanged
	{
		public Overview()
		{
			InitializeComponent();
			waitTime = 100; // 10secs
			Debug.WriteLine("Loading graphs");

			var mapper = Mappers.Xy<MeasureModel>()
				.X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
				.Y(model => model.Value);           //use the value property as Y

			Charting.For<MeasureModel>(mapper);

			//the values property will store our values array
			GpuValues = new ChartValues<MeasureModel>();
			CpuValues = new ChartValues<MeasureModel>();
			MemValues = new ChartValues<MeasureModel>();
			DiskValues = new ChartValues<MeasureModel>();
			NetValues = new ChartValues<MeasureModel>();
			//Debug.WriteLine(value);
			//lets set how to display the X Labels
			//DateTimeFormatter = value => new DateTime((long)value).ToString("hh:mm:ss");
			//AxisStep forces the distance between each separator in the X axis
			AxisStep = TimeSpan.FromSeconds(1).Ticks;
			//AxisUnit forces lets the axis know that we are plotting seconds
			//this is not always necessary, but it can prevent wrong labeling
			AxisUnit = TimeSpan.TicksPerSecond;

			SetAxisLimits(DateTime.Now);
			//SeriesCollection = new SeriesCollection
			//{
			//	new LineSeries
			//	{
			//		Title = "GPU",
			//		Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
			//		PointGeometry = DefaultGeometries.Diamond,
			//		PointGeometrySize = 15
			//	},
			//	new LineSeries
			//	{
			//		Title = "CPU",
			//		Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
			//		PointGeometry = DefaultGeometries.Circle,
			//		PointGeometrySize = 15
			//	},
			//	new LineSeries
			//	{
			//		Title = "RAM",
			//		Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
			//		PointGeometry = DefaultGeometries.Square,
			//		PointGeometrySize = 15
			//	},
			//	new LineSeries
			//	{
			//		Title = "DISK",
			//		Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
			//		PointGeometry = DefaultGeometries.Cross,
			//		PointGeometrySize = 15
			//	},
			//	new LineSeries
			//	{
			//		Title = "NETWORK",
			//		Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
			//		PointGeometry = DefaultGeometries.Triangle,
			//		PointGeometrySize = 15
			//	}
			//};
			//YFormatter = value => value.ToString("C");


			//1:gpu 
			//2:cpu
			//3:ram
			//4:disk
			//5:network
			new Thread(StartMonitering).Start();

			DataContext = this;
		}
		public static void UpdateGraphs(int index, double value)
		{
			//Debug.WriteLine(index + "||" + value);
			//SeriesCollection[index].Values.Add(value);
			//SeriesCollection[index].Values.RemoveAt(0);
			switch (index)
			{
				case 0:
					GpuValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (GpuValues.Count > 150) GpuValues.RemoveAt(0);
					break;
				case 2:
					CpuValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (CpuValues.Count > 150) CpuValues.RemoveAt(0);
					break;
				case 3:
					MemValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (MemValues.Count > 150) MemValues.RemoveAt(0);
					break;
				case 4:
					DiskValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (DiskValues.Count > 150) DiskValues.RemoveAt(0);
					break;
				case 5:
					NetValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (NetValues.Count > 150) NetValues.RemoveAt(0);
					break;

			}



			SetAxisLimits(DateTime.Now);

			//lets only use the last 150 values
			
		}

		public static void StartMonitering()
		{
			new Thread(CpuThread).Start();
			new Thread(MemThread).Start();
			new Thread(DiskThread).Start();
			//	new Thread(NetThread).Start();
		}

		public static void GetGpu()
		{
			try
			{
				var GPUs = PhysicalGPU.GetPhysicalGPUs();

				//Debug.WriteLine("\n<-----					----->");
				//Debug.WriteLine(GPUs[0].FullName);
				//Debug.WriteLine(GPUs[0].MemoryInformation);
				//Debug.WriteLine(GPUs[0].ThermalInformation.CurrentThermalLevel);
				//Debug.WriteLine(GPUs[0].UsageInformation.GPU);
				//Debug.WriteLine("<-----					----->\n");
				gpu = GPUs[0].UsageInformation.GPU.Percentage;
				UpdateGraphs(0,gpu);
			}
			catch (Exception e)
			{
				Debug.WriteLine("<---!	NVApi not found	!--->");
			}
		}

		private static void CpuThread()
		{
			while (true)
			{
				PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				cpuCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				UpdateGraphs(1, cpu);
			}
		}
		private static void MemThread()
		{
			while (true)
			{
				PerformanceCounter ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
				ramCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime/2);
				mem = ramCounter.NextValue();
				UpdateGraphs(2, mem);
				//Console.WriteLine("Memory Used: " + mem);
			}
		}
		private static void DiskThread()
		{
			while (true)
			{
				PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
				diskCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime/2);
				disk = diskCounter.NextValue();
				UpdateGraphs(3, disk);
				//Console.WriteLine("Disk usage: " + disk);
			}
		}
		private static void NetThread()
		{
			while (true)
			{
				PerformanceCounter netCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
				netCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime/2);
				net = netCounter.NextValue();
				//Console.WriteLine("Disk usage: " + net);
			}
		}
		private static void SetAxisLimits(DateTime now)
		{
			AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
			AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
		}

		public static double AxisMax;
		//{
		//	get { return _axisMax; }
		//	set
		//	{
		//		_axisMax = value;
		//		OnPropertyChanged("AxisMax");
		//	}
		//}
		public static double AxisMin;
		//{
		//	get { return _axisMin; }
		//	set
		//	{
		//		_axisMin = value;
		//		OnPropertyChanged("AxisMin");
		//	}
		//}
		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public static SeriesCollection SeriesCollection { get; set; }
		public static string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }
		public static float cpu;
		public static float mem;
		public static float disk;
		public static float net;
		public static float gpu;
		public static int waitTime; //ms
		public static ChartValues<MeasureModel> GpuValues { get; set; }
		public static ChartValues<MeasureModel> CpuValues { get; set; }
		public static ChartValues<MeasureModel> MemValues { get; set; }
		public static ChartValues<MeasureModel> DiskValues { get; set; }
		public static ChartValues<MeasureModel> NetValues { get; set; }
		public Func<double, string> DateTimeFormatter { get; set; }
		public static double AxisStep { get; set; }
		public static double AxisUnit { get; set; }
		public static double ticks;
	}
}
