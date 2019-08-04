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
	public partial class Overview : Page
	{
		public Overview()
		{
			InitializeComponent();
		}
		private void RedirectToGraphs(object sender, RoutedEventArgs e)
		{
			OverviewPage.NavigationService.Navigate(new Graphs());
		}

		private void RedirectToGrid(object sender, RoutedEventArgs e)
		{
			OverviewPage.NavigationService.Navigate(new Grid());
		}
	}
}
