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
using System.Collections.Generic;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Graphs.xaml
	/// </summary>
	public partial class Graphs : Page, INotifyPropertyChanged
	{
		public int waitTime; //ms
		private double _axisMax;
		private double _axisMin;
		public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }
		public ChartValues<MeasureModel> GpuValues { get; set; }
		public ChartValues<MeasureModel> CpuValues { get; set; }
		public ChartValues<MeasureModel> MemValues { get; set; }
		public ChartValues<MeasureModel> DiskValues { get; set; }
		public ChartValues<MeasureModel> NetValues { get; set; }
		public Func<double, string> DateTimeFormatter { get; set; }
		public double AxisStep { get; set; }
		public double AxisUnit { get; set; }

		public double AxisMax
		{
			get { return _axisMax; }
			set
			{
				_axisMax = value;
				OnPropertyChanged("AxisMax");
			}
		}
		public double AxisMin
		{
			get { return _axisMin; }
			set
			{
				_axisMin = value;
				OnPropertyChanged("AxisMin");
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public Graphs()
		{
			Debug.WriteLine("Loading Graphs");
			InitializeComponent();

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
			DateTimeFormatter = value => new DateTime((long)value).ToString("hh:mm:ss");
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
			new Thread(loadGraphs).Start();
			//notifier.ShowSuccess(message);
			//notifier.ShowWarning(message);
			//notifier.ShowError(message);
			DataContext = this;
		}

		private void loadGraphs()
		{
			while (true)
			{
				GpuValues = _Graphs.GpuValues;
				CpuValues = _Graphs.CpuValues;
				MemValues = _Graphs.MemValues;
				DiskValues = _Graphs.DiskValues;
				NetValues = _Graphs.NetValues;
				SetAxisLimits(DateTime.Now);
				System.Threading.Thread.Sleep(waitTime);

			}
		}

		private void SetAxisLimits(DateTime now)
		{
			AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
			AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
		}
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

		private void Toast(string message, string type)
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

		private void NumericOnly(object sender, TextCompositionEventArgs e)
		{
			e.Handled = IsTextNumeric(e.Text);
		}

		public bool IsTextNumeric(string str)
		{
			Regex reg = new Regex("[0-9][0-9][0-9]");
			return reg.IsMatch(str);
		}

		private void updateLvls(object sender, RoutedEventArgs e)
		{
			string[] list = _Graphs.GetWarnList();
			cpuControl.Text = list[0];
		}
	}

}
