using LiveCharts;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using NvAPIWrapper;
using NvAPIWrapper.GPU;
using System.Threading;
using LiveCharts.Configurations;
using Quartz.Classes;
using System.ComponentModel;
using System.Collections;
using System.IO;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using System.Windows;
using ToastNotifications.Messages;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Overview.xaml
	/// </summary>
	public partial class Overview : Page, INotifyPropertyChanged
	{
		public static double AxisMax;
		public static double AxisMin;
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
		public static ArrayList allProcesses;
		public static string[] whiteList;
		private static double cpuWarnLvl = 0.1;
		private static double gpuWarnLvl = 0.1;
		private static double diskWarnLvl = 0.1;
		private static double netWarnLvl = 0.1;
		private static double ramWarnLvl = 1048576;
		public static ObservableCollection<ProcessInfo> pcsdata;
		public Overview()
		{
			InitializeComponent();
			waitTime = 1000;

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
			ReloadWhiteList();
			StartMonitering();

			//notifier.ShowSuccess(message);
			//notifier.ShowWarning(message);
			//notifier.ShowError(message);
			DataContext = this;

			//<-----	Datagrid stufff	----->
			pcsdata = new ObservableCollection<ProcessInfo>();
			pcsDataGrid.DataContext = pcsdata;
		}
		public static void UpdateGraphs(int index, double value)
		{
			//Debug.WriteLine("UpdateGraphs: " + index + " | " + value);
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
			//PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
			//String[] instancename = category.GetInstanceNames();

			//foreach (string name in instancename)
			//{
			//	Console.WriteLine(name);
			//}
			//NVIDIA.Initialize();
			new Thread(GpuThread).Start();
			new Thread(CpuThread).Start();
			new Thread(MemThread).Start();
			new Thread(DiskThread).Start();
			new Thread(NetThread).Start();

			allProcesses = GetProcessNames();
			foreach (string process in allProcesses)
			{
				Debug.WriteLine(process);
				Thread PMoniter = new Thread(new ParameterizedThreadStart(MoniterProcess));
				PMoniter.SetApartmentState(ApartmentState.STA);
				PMoniter.Start(process);

			}
			new Thread(CheckNewProcesses).Start();
		}
		public static void CheckNewProcesses()
		{
			ArrayList currentProcesses = new ArrayList();
			while (true)
			{
				currentProcesses = GetProcessNames();
				foreach (string process in currentProcesses)
				{
					if (!allProcesses.Contains(process))
					{
						allProcesses.Add(process);
						Debug.WriteLine("New Process detected! " + process);
						Thread PMoniter = new Thread(new ParameterizedThreadStart(MoniterProcess));
						PMoniter.SetApartmentState(ApartmentState.STA);
						PMoniter.Start(process);
					}
				}
			}
		}
		public static void MoniterProcess(object arg)
		{
			TimeSpan runTime = new TimeSpan(0);
			DateTime startTime = DateTime.Now;
			DateTime lastTime = new DateTime();
			TimeSpan lastTotalProcessorTime = new TimeSpan();
			DateTime curTime;
			TimeSpan curTotalProcessorTime = new TimeSpan();
			string process = (string)arg;
			int gpuCycles = 0;
			int cpuCycles = 0;
			int ramCycles = 0;
			int diskCycles = 0;
			int netCycles = 0;
			double CPUUsage = 0;
			ProcessInfo pcsInfo;

			while (true)
			{
				//Debug.WriteLine("cycles: " + cycles);
				System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName(process);
				if (pname.Length == 0)
				{
                    //
					LogPcsTime(process, startTime, runTime, DateTime.Now);
					allProcesses.Remove(process);
					return;
				}
				else
				{
					try
					{
						System.Diagnostics.Process p = pname[0];

						//<----------	CPU Monitering	--------->
						runTime = p.TotalProcessorTime;
						if (lastTime == null || lastTime == new DateTime())
						{
							lastTime = DateTime.Now;
							lastTotalProcessorTime = p.TotalProcessorTime;
						}
						else
						{
							//Debug.WriteLine("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
							curTime = DateTime.Now;
							curTotalProcessorTime = p.TotalProcessorTime;

							CPUUsage = (curTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / curTime.Subtract(lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
							Console.WriteLine("{0} CPU: {1:0.0}%", p.ProcessName, CPUUsage * 100);
							if (CPUUsage > cpuWarnLvl)
							{
								if (cpuCycles < 10)
								{
									cpuCycles++;
								}
								else
								{
									Overview ovw = new Overview();
									ovw.Toast(p.ProcessName, "Warn");
									cpuCycles = 0;
								}
							}
							else
							{
								cpuCycles = 0;
							}
							lastTime = curTime;
							lastTotalProcessorTime = curTotalProcessorTime;
						}

						//<----------	RAM Monitering	--------->
						double ramUsage = p.PagedSystemMemorySize64;
						Console.WriteLine(p.ProcessName + " : " + ramUsage);
						if(ramUsage > ramWarnLvl)
						{
							if (ramCycles < 10)
							{
								ramCycles++;
							}
							else
							{
								Overview ovw = new Overview();
								ovw.Toast(p.ProcessName, "Warn");
								ramCycles = 0;
							}
						}
						else
						{
							ramCycles = 0;
						}
						pcsInfo = new ProcessInfo(p.ProcessName,CPUUsage,p.PagedMemorySize64);
						pcsdata.Add(pcsInfo);
					}
					catch (Exception e)
					{

					}
				}
				System.Threading.Thread.Sleep(waitTime);
			}

		}

		public static void LogPcsTime(string process, DateTime start, TimeSpan runTime, DateTime end)
		{
			Debug.WriteLine("Logging: " + process + "|" + start + "|" + runTime + "|" + end);
			string path = "..\\..\\..\\HQ\\Logs\\ProcessTimes.txt";
			// This text is added only once to the file.
			//if (!File.Exists(path))
			//{
			// Create a file to write to.
			for(var i =0; i<10; i++)
			{
				try
				{
					using (StreamWriter sw = File.AppendText(path))
					{
						sw.WriteLine(process + "|" + start + "|" + runTime + "|" + end);
					}
					return;
				}
				catch(Exception e)
				{
					Debug.WriteLine("File in use! Try " + i );
					Thread.Sleep(waitTime/2);
				}
			}
			
			
			//}
		}

		public static void GpuThread()
		{
			var GPUs = PhysicalGPU.GetPhysicalGPUs();
			while (true)
			{
				try
				{
					System.Threading.Thread.Sleep(waitTime);
					gpu = GPUs[0].UsageInformation.GPU.Percentage;
					UpdateGraphs(0, gpu);
					//Debug.WriteLine("updating gpu: " + gpu);
				}
				catch (Exception e)
				{
					Debug.WriteLine("<---!	GPU inactive !--->");
				}
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
			PerformanceCounter bandwidthCounter;
			float bandwidth;

			PerformanceCounter dataSentCounter;

			PerformanceCounter dataReceivedCounter;

			float sendSum = 0;
			float receiveSum = 0;

			PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
			String[] instanceName = category.GetInstanceNames();
			while (true)
			{
				bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", instanceName[0]);
				dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName[0]);
				dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName[0]);
				bandwidth = bandwidthCounter.NextValue();
				sendSum = dataSentCounter.NextValue();
				receiveSum = dataReceivedCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				bandwidth = bandwidthCounter.NextValue();
				sendSum = dataSentCounter.NextValue();
				receiveSum = dataReceivedCounter.NextValue();
				net = (8 * (sendSum + receiveSum) / bandwidth );
				UpdateGraphs(4, net);
			}
		}

		public static void ReloadWhiteList()
		{
			whiteList = File.ReadAllLines("..\\..\\..\\HQ\\Filters\\ProcessW.txt");
		}

		public static bool NotInWhiteList(string process)
		{


			foreach (string line in whiteList)
			{
				if (line == process)
				{
					return false;
				}
			}
			return true;
		}

		public static ArrayList GetProcessNames()
		{
			ArrayList currentProcesses = new ArrayList(System.Diagnostics.Process.GetProcesses());
			ArrayList pcsNames = new ArrayList();
			foreach (System.Diagnostics.Process process in currentProcesses)
			{
				if (NotInWhiteList(process.ProcessName))
				{
					pcsNames.Add(process.ProcessName);
				}
			}
			return pcsNames;
		}

		private static void SetAxisLimits(DateTime now)
		{
			AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
			AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
		}

		private void Toast(string message, string type)
		{
			Application.Current.Dispatcher.Invoke((Action)delegate
			{
				notifier.ShowWarning(message);
			});

		}

		Notifier notifier = new Notifier(cfg =>
		{
			cfg.PositionProvider = new WindowPositionProvider(
				parentWindow: Application.Current.MainWindow,
				corner: Corner.TopRight,
				offsetX: 10,
				offsetY: 10);

			cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
				notificationLifetime: TimeSpan.FromSeconds(3),
				maximumNotificationCount: MaximumNotificationCount.FromCount(5));

			cfg.Dispatcher = Application.Current.Dispatcher;
		});
	}

}
