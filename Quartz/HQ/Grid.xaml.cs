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
using System.Windows.Data;
using System.Collections.Generic;

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Grid.xaml
	/// </summary>
	public partial class Grid : Page, INotifyPropertyChanged
	{
		private static List<Entry> Sauce = new List<Entry>();

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
		public static double ticks;
		public static ArrayList allProcesses;
		public static string[] whiteList;
		private static double cpuWarnLvl = 0.1;
		private static double gpuWarnLvl = 0.1;
		private static double diskWarnLvl = 0.1;
		private static double netWarnLvl = 0.1;
		private static double ramWarnLvl = 1048576;
		public static ObservableCollection<ProcessInfo> pcsdata;
		public Grid()
		{
			Debug.WriteLine("Loading grid!");
			InitializeComponent();
			waitTime = 1000;
			//1:gpu 
			//2:cpu
			//3:ram
			//4:disk
			//5:network
			ReloadWhiteList();
			StartMonitering();
			initGrid();
			//notifier.ShowSuccess(message);
			//notifier.ShowWarning(message);
			//notifier.ShowError(message);
			DataContext = this;

			//<-----	Datagrid stufff	----->
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
			//new Thread(GpuThread).Start();
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
			Grid grid = new Grid();
			Entry entry = new Entry();
			while (true)
			{
				//Debug.WriteLine("cycles: " + cycles);
				System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName(process);
				if (pname.Length == 0)
				{
					//
					LogPcsTime(process, startTime, runTime, DateTime.Now);
					allProcesses.Remove(process);
					Sauce.Remove(entry);
					return;
				}
				else
				{
					//try
					//{
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
									Grid ovw = new Grid();
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
						if (ramUsage > ramWarnLvl)
						{
							if (ramCycles < 10)
							{
								ramCycles++;
							}
							else
							{
								Grid ovw = new Grid();
								ovw.Toast(p.ProcessName, "Warn");
								ramCycles = 0;
							}
						}
						else
						{
							ramCycles = 0;
						}
						Sauce.Remove(entry);
						entry = new Entry(p.ProcessName, CPUUsage, ramUsage, runTime);
						Sauce.Add(entry);
						grid.SetDisplay();
					//}
					//catch (Exception e)
					//{
							
					//}
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
			for (var i = 0; i < 10; i++)
			{
				try
				{
					using (StreamWriter sw = File.AppendText(path))
					{
						sw.WriteLine(process + "|" + start + "|" + runTime + "|" + end);
					}
					return;
				}
				catch (Exception e)
				{
					Debug.WriteLine("File in use! Try " + i);
					Thread.Sleep(waitTime / 2);
				}
			}


			//}
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

		private void initGrid()
		{
			DataGridTextColumn name = new DataGridTextColumn();
			DataGridTextColumn cpu = new DataGridTextColumn();
			DataGridTextColumn ram = new DataGridTextColumn();
			DataGridTextColumn runTime = new DataGridTextColumn();
			name.Header = "Process";
			cpu.Header = "Cpu Usage";
			ram.Header = "Ram Usage";
			runTime.Header = "Process Runtime";
			name.Binding = new Binding("pcsName");
			cpu.Binding = new Binding("pcsCpu");
			ram.Binding = new Binding("pcsRam");
			runTime.Binding = new Binding("pcsTime");

			processGrid.Columns.Add(name);
			processGrid.Columns.Add(cpu);
			processGrid.Columns.Add(ram);
			processGrid.Columns.Add(runTime);

		}

		private struct Entry
		{
			public string pcsName { get; set; }
			public double pcsCpu { get; set; }
			public double pcsRam { get; set; }
			public TimeSpan pcsTime { get; set; }

			public Entry(string name, double cpu, double ram, TimeSpan Time)
			{
				pcsName = name;
				pcsCpu = cpu;
				pcsRam = ram;
				pcsTime = Time;
			}
		}
		private void SetDisplay()
		{
			
			Dispatcher.Invoke(() =>
			{
				Debug.WriteLine("Adding Entry!");
				processGrid.ItemsSource = Sauce;
			});
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
