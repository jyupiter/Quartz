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
using System.Collections;
using System.IO;

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
			waitTime = 1000; 
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

			//1:gpu 
			//2:cpu
			//3:ram
			//4:disk
			//5:network
			StartMonitering();

			DataContext = this;
		}
		public static void UpdateGraphs(int index, double value)
		{
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
				case 1:
					CpuValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (CpuValues.Count > 150) CpuValues.RemoveAt(0);
					break;
				case 2:
					MemValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (MemValues.Count > 150) MemValues.RemoveAt(0);
					break;
				case 3:
					DiskValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (DiskValues.Count > 150) DiskValues.RemoveAt(0);
					break;
				case 4:
					NetValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (NetValues.Count > 150) NetValues.RemoveAt(0);
					break;
				default:
					Debug.WriteLine("Invalid Index " + index);
					break;


			}



			SetAxisLimits(DateTime.Now);

			//lets only use the last 150 values
			
		}

		public static void StartMonitering()
		{
			PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
			String[] instancename = category.GetInstanceNames();

			foreach (string name in instancename)
			{
				Console.WriteLine(name);
			}
			//new Thread(CpuThread).Start();
			//new Thread(MemThread).Start();
			//new Thread(DiskThread).Start();
			//new Thread(NetThread).Start();

			ArrayList allProcesses = new ArrayList(Process.GetProcesses());
			ArrayList processNames = new ArrayList();
			foreach(Process process in allProcesses)
			{
				processNames.Add(process.ProcessName);
			}
			processNames = CheckWhiteList(processNames);
			foreach (string process in processNames)
			{
				Debug.WriteLine(process);
				Thread PMoniter = new Thread(new ParameterizedThreadStart(MoniterProcess));
				//PMoniter.Start(process);
			}

		}
		public static void MoniterProcess(object arg)
		{
			Process process = (Process)arg;
			
			while (true)
			{
				Process[] pname = Process.GetProcessesByName(process.ProcessName);
				if (pname.Length == 0)
				{
					return;
				}
				else
				{
					//Debug.WriteLine("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
					//Debug.WriteLine(process.ProcessName);
					//Debug.WriteLine(process.TotalProcessorTime);
					//Debug.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");
				}
				System.Threading.Thread.Sleep(waitTime);
			}
			
		}

		public static void GetGpu()
		{
			try
			{
				var GPUs = PhysicalGPU.GetPhysicalGPUs();

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
				cpu = cpuCounter.NextValue();
				UpdateGraphs(1, cpu);
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
				//Debug.Write(mem + "-");
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
				System.Threading.Thread.Sleep(waitTime);
				disk = diskCounter.NextValue();
				UpdateGraphs(3, disk);
				//Console.WriteLine("Disk usage: " + disk);
			}
		}
		private static void NetThread()
		{
			PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
			String[] instancename = category.GetInstanceNames();
			while (true)
			{
				PerformanceCounter netCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
				netCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				net = netCounter.NextValue();
				//Console.WriteLine("Disk usage: " + net);
			}
		}

		public static ArrayList CheckWhiteList(ArrayList allProcesses)
		{
			string[] lines = File.ReadAllLines("..\\..\\..\\HQ\\Filters\\ProcessW.txt");

			foreach (string line in lines)
			{
				foreach(string process in allProcesses.ToArray())
				{
					if(line == process)
					{
						//Console.WriteLine(allProcesses.Capacity);
						allProcesses.Remove(process);
						//Console.WriteLine(allProcesses.Capacity);
						//Console.WriteLine("removing: " + line);

					}
				}
			}
			return allProcesses;	
		}
		private static void SetAxisLimits(DateTime now)
		{
			AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
			AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
		}

		public static double AxisMax;
		public static double AxisMin;
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
