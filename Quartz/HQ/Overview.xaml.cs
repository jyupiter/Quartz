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
			waitTime = 100; // 10secs
			Debug.WriteLine("Loading graphs");
			SeriesCollection = new SeriesCollection
			{
				new LineSeries
				{
					Title = "GPU",
					Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
					PointGeometry = DefaultGeometries.Diamond,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "CPU",
					Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
					PointGeometry = DefaultGeometries.Circle,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "RAM",
					Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
					PointGeometry = DefaultGeometries.Square,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "DISK",
					Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
					PointGeometry = DefaultGeometries.Cross,
					PointGeometrySize = 15
				},
				new LineSeries
				{
					Title = "NETWORK",
					Values = new ChartValues<double> {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
					PointGeometry = DefaultGeometries.Triangle,
					PointGeometrySize = 15
				}
			};

			Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "june" };
			//YFormatter = value => value.ToString("C");


			//1:gpu 
			//2:cpu
			//3:ram
			//4:disk
			//5:network
			new Thread(StartMonitering).Start();
			new Thread(Init).Start();

			DataContext = this;
		}
		public static void UpdateGraphs(int index, double value)
		{
			//Debug.WriteLine(index + "||" + value);
			SeriesCollection[index].Values.Add(value);
			SeriesCollection[index].Values.RemoveAt(0);

		}

		public static void Init()
		{
			while (true)
			{
				new Thread(GetCpu).Start();
				//new Thread(GetGpu).Start();
				new Thread(GetMem).Start();
				new Thread(GetDisk).Start();
				//new Thread(GetNetwork).Start();
				Labels = new[] {
					DateTime.Now.AddMilliseconds(-9*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-8*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-7*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-6*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-5*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-4*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-3*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-2*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.AddMilliseconds(-1*waitTime).ToString("hh:mm:ss"),
					DateTime.Now.ToString("hh:mm:ss"),

				};
				Thread.Sleep(waitTime);
			}
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
				UpdateGraphs(0, GPUs[0].UsageInformation.GPU.Percentage);
			}
			catch (Exception e)
			{
				Debug.WriteLine("<---!	NVApi not found	!--->");
			}
		}


		public static void GetCpu()
		{
			UpdateGraphs(1,cpu);
		}

		public static void GetMem()
		{
			UpdateGraphs(2,mem);
		}

		public static void GetDisk()
		{
			UpdateGraphs(3,disk);
		}

		public static void GetNetwork()
		{
			//UpdateGraphs(0, GPUs[0].UsageInformation.GPU.Percentage);
		}

		private static void CpuThread()
		{
			while (true)
			{
				PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				cpuCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				cpu = cpuCounter.NextValue();
				Console.WriteLine("Processor Usage: " + cpu);
			}
		}
		private static void MemThread()
		{
			while (true)
			{
				PerformanceCounter ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
				ramCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				mem = ramCounter.NextValue();
				Console.WriteLine("Memory Used: " + mem);
			}
		}
		private static void DiskThread()
		{
			while (true)
			{
				PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
				diskCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				disk = diskCounter.NextValue();
				Console.WriteLine("Disk usage: " + disk);
			}
		}
		private static void NetThread()
		{
			while (true)
			{
				PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
				diskCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				disk = diskCounter.NextValue();
				Console.WriteLine("Disk usage: " + disk);
			}
		}

		public static SeriesCollection SeriesCollection { get; set; }
		public static string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }
		public static float cpu;
		public static float mem;
		public static float disk;
		public static float net;
		public static int waitTime; //ms
	}
}
