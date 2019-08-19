using LiveCharts;
using NvAPIWrapper.GPU;
using Quartz.HQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace Quartz.Classes
{
	class _Graphs
	{
		public static ChartValues<MeasureModel> GpuValues { get; set; }
		public static ChartValues<MeasureModel> CpuValues { get; set; }
		public static ChartValues<MeasureModel> MemValues { get; set; }
		public static ChartValues<MeasureModel> DiskValues { get; set; }
		public static ChartValues<MeasureModel> NetValues { get; set; }
		public static int waitTime; //ms
		private static bool isTracking = false;
		private static double cpu;
		private static double mem;
		private static double disk;
		private static double net;
		private static double gpu;
		private static double cpuThreshold = 80;
		private static double memThreshold = 80;
		private static double diskThreshold = 80;
		private static double netThreshold = 80;
		private static double gpuThreshold = 80;
		private static string[] config;
		public static void initGraphs()
		{
			if (!isTracking) { 
			isTracking = true;
			Debug.WriteLine("Loading Graphs");
			waitTime = 1000;

			//the values property will store our values array
			GpuValues = new ChartValues<MeasureModel>();
			CpuValues = new ChartValues<MeasureModel>();
			MemValues = new ChartValues<MeasureModel>();
			DiskValues = new ChartValues<MeasureModel>();
			NetValues = new ChartValues<MeasureModel>();
			//1:gpu 
			//2:cpu
			//3:ram
			//4:disk
			//5:network
			StartMonitering();
			}
		}

		public static void ReloadWarnList()
		{
			config = File.ReadAllLines("..\\..\\..\\HQ\\Config\\Thresholds.txt");
			cpuThreshold = Convert.ToInt32(config[0]);
			gpuThreshold = Convert.ToInt32(config[1]);
			memThreshold = Convert.ToInt32(config[2]);
			diskThreshold = Convert.ToInt32(config[3]);
			netThreshold = Convert.ToInt32(config[4]);
		}

		public static string[] GetWarnList()
		{
			string[] list = new string[5] { config[0], config[1], config[2], config[3], config[4] };
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
			//new Thread(GpuThread).Start();
			//new Thread(CpuThread).Start();
			//new Thread(MemThread).Start();
			//ew Thread(DiskThread).Start();
			//new Thread(NetThread).Start();
		}
		public static void GpuThread()
		{
			Debug.WriteLine("Starting Gpu");
			int gpuCycles = 0;
                var GPUs = PhysicalGPU.GetPhysicalGPUs();
 

            
			while (true)
			{
				try
				{
					System.Threading.Thread.Sleep(waitTime);
					gpu = GPUs[0].UsageInformation.GPU.Percentage;
					UpdateGraphs(0, gpu);
					//Debug.WriteLine("updating gpu: " + gpu);
					if (gpu > gpuThreshold)
					{
						if (gpuCycles < 20)
						{
							gpuCycles++;
						}
						else
						{
							Toast("High GPU usage detected!", "Info");
							gpuCycles = 0;
						}
					}
					else
					{
						gpuCycles = 0;
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
					Debug.WriteLine("<---!	GPU inactive !--->");
				}


			}

		}

		private static void CpuThread()
		{
			Debug.WriteLine("Starting Cpu");
			int cpuCycles = 0;
			while (true)
			{
				PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				cpuCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				cpu = cpuCounter.NextValue();
				UpdateGraphs(1, cpu);
				if (cpu > cpuThreshold)
				{
					if (cpuCycles < 20)
					{
						cpuCycles++;
					}
					else
					{
						Toast("High CPU usage detected!", "Info");
						cpuCycles = 0;
					}
				}
				else
				{
					cpuCycles = 0;
				}
			}
		}

		private static void MemThread()
		{
			Debug.WriteLine("Starting Mem");
			int memCycles = 0;
			while (true)
			{

				PerformanceCounter ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
				ramCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				mem = ramCounter.NextValue();
				//Debug.Write(mem + "-");
				UpdateGraphs(2, mem);
				if (mem > memThreshold)
				{
					if (memCycles < 20)
					{
						memCycles++;
					}
					else
					{
						Toast("High Memory usage detected!", "Info");
						memCycles = 0;
					}
				}
				else
				{
					memCycles = 0;
				}
			}
		}

		private static void DiskThread()
		{
			int diskCycles = 0;
			Debug.WriteLine("Starting Disk");
			while (true)
			{
				PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
				diskCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				disk = diskCounter.NextValue();
				UpdateGraphs(3, disk);
				if (disk > diskThreshold)
				{
					if (diskCycles < 20)
					{
						diskCycles++;
					}
					else
					{
						Toast("High Disk usage detected!", "Info");

					}
				}
				else
				{
					diskCycles = 0;
				}
				//Console.WriteLine("Disk usage: " + disk);
			}
		}

		private static void NetThread()
		{
			Debug.WriteLine("Starting Net");
			PerformanceCounter bandwidthCounter;
			float bandwidth;

			PerformanceCounter dataSentCounter;

			PerformanceCounter dataReceivedCounter;


			float sendSum = 0;
			float receiveSum = 0;

			PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
			String[] instanceName = category.GetInstanceNames();

			bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", instanceName[0]);
			dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName[0]);
			dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName[0]);

			while (true)
			{

				bandwidth = bandwidthCounter.NextValue();
				sendSum = dataSentCounter.NextValue();
				receiveSum = dataReceivedCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				bandwidth = bandwidthCounter.NextValue();
				sendSum = dataSentCounter.NextValue();
				receiveSum = dataReceivedCounter.NextValue();
				Random rng = new Random();
				net = rng.NextDouble();//(8 * (sendSum + receiveSum) / bandwidth);
									   //Debug.WriteLine("instanceName " + instanceName[0]);

				//Debug.WriteLine("sendSum " + sendSum);
				//Debug.WriteLine("receiveSum " + receiveSum);
				//Debug.WriteLine("bandwidth " + bandwidth);
				UpdateGraphs(4, net);

			}
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
					if (GpuValues.Count > 10) GpuValues.RemoveAt(0);
					break;
				case 1:
					CpuValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (CpuValues.Count > 10) CpuValues.RemoveAt(0);
					break;
				case 2:
					MemValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (MemValues.Count > 10) MemValues.RemoveAt(0);
					break;
				case 3:
					DiskValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (DiskValues.Count > 10) DiskValues.RemoveAt(0);
					break;
				case 4:
					NetValues.Add(new MeasureModel
					{
						DateTime = DateTime.Now,
						Value = value
					});
					if (NetValues.Count > 10) NetValues.RemoveAt(0);
					break;
				default:
					Debug.WriteLine("Invalid Index " + index);
					break;
			}
			//SetAxisLimits(DateTime.Now);

			//lets only use the last 150 values
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
}
