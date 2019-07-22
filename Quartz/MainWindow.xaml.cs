using Quartz.AG;
using Quartz.HQ;
using Quartz.MARC;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Win32;
using Quartz.AV;

namespace Quartz
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			marcustimer();
			marcusLoginLogoutDetector();
			foreach(Button b in MainMenu.Children.OfType<Button>())
			{
				b.Click += FocusHandler;
			}

		}

		private void FocusHandler(object sender, RoutedEventArgs e)
		{
		}

		#region WindowActions
		private void DragWindow(object sender, MouseButtonEventArgs e)
		{
			try
			{
				DragMove();
			}
			catch(InvalidOperationException i)
			{
				Console.WriteLine(i.Message);
			}
		}

		private void WindowClose(object sender, RoutedEventArgs e)
		{
			var fullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + "avan_magpie.txt";
			if(System.IO.File.Exists(fullPath))
			{
				// Use a try block to catch IOExceptions, to
				// handle the case of the file already being
				// opened by another process.
				try
				{
					File.SetAttributes(fullPath, File.GetAttributes(fullPath) & ~FileAttributes.ReadOnly);
					System.IO.File.Delete(fullPath);
				}
				catch(System.IO.IOException f)
				{
					Console.WriteLine(f.Message);
					return;
				}
			}
			Close();
		}

		private void WindowMinimize(object sender, RoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}
		#endregion

		#region ResizeWindows
		bool ResizeInProcess = false;
		private void Resize_Init(object sender, MouseButtonEventArgs e)
		{
			Rectangle senderRect = sender as Rectangle;
			if(senderRect != null)
			{
				ResizeInProcess = true;
				senderRect.CaptureMouse();
			}
		}

		private void Resize_End(object sender, MouseButtonEventArgs e)
		{
			Rectangle senderRect = sender as Rectangle;
			if(senderRect != null)
			{
				ResizeInProcess = false;
				senderRect.ReleaseMouseCapture();
			}
		}

		private void Resizing_Form(object sender, MouseEventArgs e)
		{
			if(ResizeInProcess)
			{
				Rectangle senderRect = sender as Rectangle;
				Window mainWindow = senderRect.Tag as Window;
				if(senderRect != null)
				{
					double width = e.GetPosition(mainWindow).X;
					double height = e.GetPosition(mainWindow).Y;
					senderRect.CaptureMouse();
					if(senderRect.Name.ToLower().Contains("right"))
					{
						width += 5;
						if(width > 0)
							mainWindow.Width = width;
					}
					if(senderRect.Name.ToLower().Contains("left"))
					{
						width -= 5;
						mainWindow.Left += width;
						width = mainWindow.Width - width;
						if(width > 0)
						{
							mainWindow.Width = width;
						}
					}
					if(senderRect.Name.ToLower().Contains("bottom"))
					{
						height += 5;
						if(height > 0)
							mainWindow.Height = height;
					}
					if(senderRect.Name.ToLower().Contains("top"))
					{
						height -= 5;
						mainWindow.Top += height;
						height = mainWindow.Height - height;
						if(height > 0)
						{
							mainWindow.Height = height;
						}
					}
				}
			}
		}
		#endregion

		private void RedirectToFiles(object sender, RoutedEventArgs e)
		{
			ContentWrapper.NavigationService.Navigate(new Files());
		}

			private void RedirectToMonitering(object sender, RoutedEventArgs e)
			{
				  ContentWrapper.NavigationService.Navigate(new Overview());
			}
		
		private void RedirectToHome(object sender, RoutedEventArgs e)
		{
			ContentWrapper.NavigationService.Navigate(new Home());
		}

		/*Marcus page*/
		private void RedirectMarcusHome(object sender, RoutedEventArgs e)
		{
			ContentWrapper.NavigationService.Navigate(new MarcusHome());
		}

		//temp func, delete if not used in final product
		public void marcustimer()
		{
			System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
			dispatcherTimer.Tick += dispatcherTimer_Tick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
			dispatcherTimer.Start();

			 void dispatcherTimer_Tick(object sender, EventArgs e)
			 {
				// code goes here
				

			 }

		}

		// detect if user locks / unlocks their screen, done so by pressing windows+L
		public void marcusLoginLogoutDetector()
		{
			Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);

			void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
			{
				if (e.Reason == SessionSwitchReason.SessionLock)
				{
					//User locks screen
					// call get logs method and begin monitoring for unauthorized access
					MessageBox.Show("i left my desk");
					Class1 clas = new Class1();
					clas.printlinetest();
					
				}
				else if (e.Reason == SessionSwitchReason.SessionUnlock)
				{
					//User logs back in
					MessageBox.Show("i returned to desk");
					Class1 clas = new Class1();
					clas.printlinetest2();
				}
			}
		}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ContentWrapper.NavigationService.Navigate(new CheckUpdater());
        }
    }
}
