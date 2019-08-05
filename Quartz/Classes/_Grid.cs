using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace Quartz.Classes
{
	static class _Grid
	{
		public static List<Entry> Sauce = new List<Entry>();

		public static float cpu;
		public static float mem;
		public static float disk;
		public static float net;
		public static float gpu;
		public static int waitTime = 1000; //ms
		public static double ticks;
		public static ArrayList allProcesses;
		public static string[] whiteList;
		private static double cpuWarnLvl = 0.1;
		private static double gpuWarnLvl = 0.1;
		private static double diskWarnLvl = 0.1;
		private static double netWarnLvl = 0.1;
		private static double ramWarnLvl = 1048576;
		private static string[] config;
		private static bool isInit = false;

		public static void InitGrid()
		{
			if (!isInit)
			{
				isInit = true;
				Debug.WriteLine("Loading grid!");
				//1:gpu 
				//2:cpu
				//3:ram
				//4:disk
				//5:network
				ReloadWhiteList();
				StartMonitering();

			}
		}

		public static void ReloadWarnList()
		{
			config = File.ReadAllLines("..\\..\\..\\HQ\\Config\\PCSWarn.txt");
			cpuWarnLvl = ( Convert.ToDouble(config[0])) / 100;
			ramWarnLvl = Convert.ToInt32(config[1])*1024;
		}

		public static string[] GetWarnList()
		{
			string[] list = new string[2] { config[0], config[1]};
			return list;
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
			//new Thread(gridUpdate).Start();
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

		//public static void gridUpdate()
		//{
		//	while (true)
		//	{
		//		UpdateDisplay();
		//		System.Threading.Thread.Sleep(waitTime * 2);
		//	}

		//}

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
			int cpuCycles = 0;
			int ramCycles = 0;
			double CPUUsage = 0;
			Entry entry = new Entry(process, 0.0, 0.0, new TimeSpan());
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
				if (pname.Length == 0 || !NotInWhiteList(process))
				{
					//
					LogPcsTime(process, startTime, runTime, DateTime.Now);
					allProcesses.Remove(process);
					RemoveEntry(process);
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
						//Console.WriteLine("{0} CPU: {1:0.0}%", p.ProcessName, CPUUsage * 100);
						if (CPUUsage > cpuWarnLvl)
						{
							if (cpuCycles < 10)
							{
								cpuCycles++;
							}
							else
							{
								Toast("High CPU usage by " + p.ProcessName, "Error");
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
							Toast("High memory usage by " + p.ProcessName, "Warn");
							ramCycles = 0;
						}
					}
					else
					{
						ramCycles = 0;
					}
					SetEntry(p.ProcessName, CPUUsage, ramUsage, curTotalProcessorTime);

					}
					catch (Exception e)
					{

					}
				}
				System.Threading.Thread.Sleep(waitTime);
			}

		}

		public static void SetEntry(string name, double cpu, double ram, TimeSpan time)
		{
			for (var i = 0; i < Sauce.Count; i++)
			{
				if (name == Sauce[i].pcsName)
				{
					//Sauce[i].pcsCpu = cpu * 100;
					//Sauce[i].pcsRam = ram /1024;
					//Sauce[i].pcsTime = time;
					Sauce[i] = new Entry(Sauce[i].pcsName, cpu * 100, ram / 1024,time);
				}
			}
		}
		public static void RemoveEntry(string name)
		{
			for (var i = 0; i < Sauce.Count; i++)
			{
				if (name == Sauce[i].pcsName)
				{
					Sauce.RemoveAt(i);
				}
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
				catch (Exception)
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

		
		private static void Toast(string message, string type)
		{
			Application.Current.Dispatcher.Invoke((Action)delegate
			{
				switch (type)
				{
					case "Warn":
						notifier.ShowWarning(message);
						break;
					case "Error":
						notifier.ShowError(message);
						break;
					case "Info":
						notifier.ShowInformation(message);
						break;
				}

			});

		}

		static Notifier notifier = new Notifier(cfg =>
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

	//[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Entry
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

}
