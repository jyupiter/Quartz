﻿using LiveCharts;
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
using System.Collections.Generic;

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Graphs.xaml
	/// </summary>
	public partial class Graphs : Page
	{
		public double AxisMax;
		public double AxisMin;
		public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }
		public float cpu;
		public float mem;
		public float disk;
		public float net;
		public float gpu;
		public int waitTime; //ms
		public ChartValues<MeasureModel> GpuValues { get; set; }
		public ChartValues<MeasureModel> CpuValues { get; set; }
		public ChartValues<MeasureModel> MemValues { get; set; }
		public ChartValues<MeasureModel> DiskValues { get; set; }
		public ChartValues<MeasureModel> NetValues { get; set; }
		public Func<double, string> DateTimeFormatter { get; set; }
		public double AxisStep { get; set; }
		public double AxisUnit { get; set; }
		public Graphs()
		{
			Debug.WriteLine("Loading Graphs");
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
			StartMonitering();

			//notifier.ShowSuccess(message);
			//notifier.ShowWarning(message);
			//notifier.ShowError(message);
			DataContext = this;
		}
		public void UpdateGraphs(int index, double value)
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
			SetAxisLimits(DateTime.Now);

			//lets only use the last 150 values
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
			new Thread(GpuThread).Start();
			new Thread(CpuThread).Start();
			new Thread(MemThread).Start();
			new Thread(DiskThread).Start();
			new Thread(NetThread).Start();
		}
		public void GpuThread()
		{
			Debug.WriteLine("Starting Gpu");
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

		private void CpuThread()
		{
			Debug.WriteLine("Starting Cpu");
			while (true)
			{
				PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				cpuCounter.NextValue();
				System.Threading.Thread.Sleep(waitTime);
				cpu = cpuCounter.NextValue();
				UpdateGraphs(1, cpu);
			}
		}

		private void MemThread()
		{
			Debug.WriteLine("Starting Mem");
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

		private void DiskThread()
		{
			Debug.WriteLine("Starting Disk");
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

		private void NetThread()
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
				net = (8 * (sendSum + receiveSum) / bandwidth);
				UpdateGraphs(4, net);
			}
		}
		private void SetAxisLimits(DateTime now)
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

		private void Cpu(object sender, RoutedEventArgs e)
		{
			ToggleVisibility();
			this.CpuGraph.Visibility = Visibility.Visible;
		}
		private void Gpu(object sender, RoutedEventArgs e)
		{
			ToggleVisibility();
			this.GpuGraph.Visibility = Visibility.Visible;
		}
		private void Ram(object sender, RoutedEventArgs e)
		{
			ToggleVisibility();
			this.RamGraph.Visibility = Visibility.Visible;
		}
		private void Disk(object sender, RoutedEventArgs e)
		{
			ToggleVisibility();
			this.DiskGraph.Visibility = Visibility.Visible;
		}
		private void Network(object sender, RoutedEventArgs e)
		{
			ToggleVisibility();
			this.NetGraph.Visibility = Visibility.Visible;
		}

		private void ToggleVisibility()
		{
			Debug.WriteLine("Visibility cleared!");
			this.CpuGraph.Visibility = Visibility.Collapsed;
			this.GpuGraph.Visibility = Visibility.Collapsed;
			this.RamGraph.Visibility = Visibility.Collapsed;
			this.DiskGraph.Visibility = Visibility.Collapsed;
			this.NetGraph.Visibility = Visibility.Collapsed;
		}

	}

}
