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
using System.Runtime.InteropServices;

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Grid.xaml
	/// </summary>
	public partial class Grid : Page, INotifyPropertyChanged
	{
		private List<Entry> Sauce = new List<Entry>();

		public double AxisMax;
		public double AxisMin;
		public event PropertyChangedEventHandler PropertyChanged;
		public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }
		public float cpu;
		public float mem;
		public float disk;
		public float net;
		public float gpu;
		public int waitTime; //ms
		public double ticks;
		public ArrayList allProcesses;
		public string[] whiteList;
		private double cpuWarnLvl = 0.1;
		private double gpuWarnLvl = 0.1;
		private double diskWarnLvl = 0.1;
		private double netWarnLvl = 0.1;
		private double ramWarnLvl = 1048576;
		public ObservableCollection<ProcessInfo> pcsdata;
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


		public void StartMonitering()
		{
			//PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
			//String[] instancename = category.GetInstanceNames();

			//foreach (string name in instancename)
			//{
			//	Console.WriteLine(name);
			//}
			//NVIDIA.Initialize();
			new Thread(gridUpdate).Start();
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

		public void gridUpdate()
		{
			while (true)
			{
				UpdateDisplay();
				System.Threading.Thread.Sleep(waitTime * 2);
			}
			
		}
		public void CheckNewProcesses()
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
		public void MoniterProcess(object arg)
		{
			TimeSpan runTime = new TimeSpan(0);
			DateTime startTime = DateTime.Now;
			DateTime lastTime = new DateTime();
			TimeSpan lastTotalProcessorTime = new TimeSpan();
			DateTime curTime;
			TimeSpan curTotalProcessorTime = new TimeSpan();
			string process = (string)arg;
			int cpuCycles = 0;
			int ramCycles = 0;
			double CPUUsage = 0;
			Entry entry = new Entry(process, 0.0,0.0, new TimeSpan());
			Sauce.Add(entry);
			//TreeViewItem item = new TreeViewItem();
			//item.Name = process;
			//item.Header = process;
			//this.Dispatcher.Invoke(() =>
			//{
			//	mainTree.Items.Add(item);
			//});

			//grid.mainTree
			while (true)
			{
				//Debug.WriteLine("cycles: " + cycles);
				System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName(process);
				if (pname.Length == 0)
				{
					//
					LogPcsTime(process, startTime, runTime, DateTime.Now);
					allProcesses.Remove(process);
					RemoveEntry(process);
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
						//Console.WriteLine("{0} CPU: {1:0.0}%", p.ProcessName, CPUUsage * 100);
						if (CPUUsage > cpuWarnLvl)
						{
							if (cpuCycles < 10)
							{
								cpuCycles++;
							}
							else
							{
								Toast(p.ProcessName, "Warn");
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
					//Console.WriteLine(p.ProcessName + " : " + ramUsage);
					if (ramUsage > ramWarnLvl)
					{
						if (ramCycles < 10)
						{
							ramCycles++;
						}
						else
						{
							Toast(p.ProcessName, "Warn");
							ramCycles = 0;
						}
					}
					else
					{
						ramCycles = 0;
					}
					SetEntry(p.ProcessName, CPUUsage*100,ramUsage,curTotalProcessorTime);
					
					//}
					//catch (Exception e)
					//{

					//}
				}
				System.Threading.Thread.Sleep(waitTime);
			}

		}

		public void SetEntry(string name, double cpu, double ram, TimeSpan time)
		{
			for (var i = 0; i < Sauce.Count; i++)
			{
				if (name == Sauce[i].pcsName)
				{
					Sauce[i].pcsCpu = cpu;
					Sauce[i].pcsRam = ram;
					Sauce[i].pcsTime = time;
				}
			}
		}
		public void RemoveEntry(string name)
		{
			for(var i = 0; i < Sauce.Count; i++)
			{
				if(name == Sauce[i].pcsName)
				{
					Sauce.RemoveAt(i);
				}
			}
		}

		public void LogPcsTime(string process, DateTime start, TimeSpan runTime, DateTime end)
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

		public void ReloadWhiteList()
		{
			whiteList = File.ReadAllLines("..\\..\\..\\HQ\\Filters\\ProcessW.txt");
		}

		public bool NotInWhiteList(string process)
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

		public ArrayList GetProcessNames()
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


		private void UpdateDisplay()
		{

			Dispatcher.Invoke(() =>
			{
				Debug.WriteLine("Adding Entry!");
				processGrid.ItemsSource = Sauce;
				processGrid.Items.Refresh();
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

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class Entry
	{
		public string pcsName { get; set; }
		public double pcsCpu { get; set; }
		public double pcsRam { get; set; }
		public TimeSpan pcsTime { get; set; }

		public Entry() { }
		public Entry(string name, double cpu, double ram, TimeSpan Time)
		{
			pcsName = name;
			pcsCpu = cpu;
			pcsRam = ram;
			pcsTime = Time;
		}
	}
}
