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
using System.Windows.Data;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Quartz.HQ
{
	/// <summary>
	/// Interaction logic for Grid.xaml
	/// </summary>
	public partial class Grid : Page
	{
		public Grid()
		{
			Debug.WriteLine("Loading grid!");
			InitializeComponent();
			DataContext = this;
			//InitGrid();
			DisplayWhiteList();
			updateDisplay();
			new Thread(UpdateDisplay).Start();
		}
		private void InitGrid()
		{
			DataGridTextColumn name = new DataGridTextColumn();
			DataGridTextColumn cpu = new DataGridTextColumn();
			DataGridTextColumn ram = new DataGridTextColumn();
			DataGridTextColumn runTime = new DataGridTextColumn();
			//name.Header = "Process";
			//cpu.Header = "Cpu Usage (%)";
			//ram.Header = "Ram Usage (MB)";
			//runTime.Header = "Process Runtime";
			//name.Binding = new Binding("pcsName");
			//cpu.Binding = new Binding("pcsCpu");
			//ram.Binding = new Binding("pcsRam");
			//runTime.Binding = new Binding("pcsTime");

			processGrid.Columns.Add(name);
			processGrid.Columns.Add(cpu);
			processGrid.Columns.Add(ram);
			processGrid.Columns.Add(runTime);

		}

		private void UpdateDisplay()
		{
			while (true)
			{
				try
				{
					Dispatcher.Invoke(() =>
					{
						Debug.WriteLine("Adding Entry!");
						processGrid.ItemsSource = _Grid.Sauce;
						processGrid.Items.Refresh();
					});
				}
				catch (Exception)
				{
					
				}
				Thread.Sleep(_Grid.waitTime / 2);
			}
			
		}

		public void SaveWhitelist(object sender, RoutedEventArgs e)
		{
			using (System.IO.StreamWriter file =
			new System.IO.StreamWriter("..\\..\\..\\HQ\\Filters\\ProcessW.txt"))
			{
				file.WriteLine(whitelistBox.Text);
			}
			_Grid.ReloadWhiteList();
			DisplayWhiteList();
		}

		public void DisplayWhiteList()
		{
			string output = "";
			for (int i = 0; i < _Grid.whiteList.Length -1; i++)
			{
				output += _Grid.whiteList[i] + "\n";
			}
			//foreach(string line in whiteList)
			//{
			//	output += line + "\n";
			//}
			whitelistBox.Text = output;
		}

		private void updateLvls(object sender, RoutedEventArgs e)
		{
			string[] list = new string[2] { PCSCpu.Text,PCSRam.Text };
			using (System.IO.StreamWriter file =
			new System.IO.StreamWriter("..\\..\\..\\HQ\\Config\\PCSWarn.txt"))
			{
				foreach (string line in list)
				{
					file.WriteLine(line);
				}

			}
			updateDisplay();
		}

		private void updateDisplay()
		{
			_Grid.ReloadWarnList();
			string[] list = _Grid.GetWarnList();
			PCSCpu.Text = list[0];
			PCSRam.Text = list[1];
		}
	}
}
