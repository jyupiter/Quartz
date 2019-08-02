using Quartz.AG;
using Quartz.HQ;
using Quartz.MARC;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;
using Lucene.Net.Messages;
using Microsoft.Win32;
using NvAPIWrapper.Native.DRS.Structures;
using Quartz.AV;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace Quartz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // do not change these variables 
        public static String Datetime = "";//current timestamp
        public static string Bruteforcelogtime = "";//timestamp of failed login attempt
        public static int LoginAttemptCount = 0;
        public static string SMSlogintimestamps = "";//all timestamps of failed logins 
        public MainWindow()
        {
            InitializeComponent();
            //marcustimer();// delete if not used in final product
            //calltwilio();
            LoginLogoutDetector();//entry point for M's function
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
        public static void LoginLogoutDetector()
        {
            EventLog logListener = new EventLog("Security");
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            

            void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    //User locks screen
                    
                    Datetime = (DateTime.Now.ToString("dd:MM:yyyy hh:mm:ss tt")).ToString();
                    logListener.EntryWritten -= logListener_EntryWritten;//unregister prev subscribers
                    logListener.EntryWritten += logListener_EntryWritten;//register 
                    logListener.EnableRaisingEvents = true;

                    
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    Microsoft.Win32.SystemEvents.SessionSwitch -= new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
                    Console.WriteLine("Screen unlocked!");
                    

                }
            }//sessionswitch

            void logListener_EntryWritten(object sender, EntryWrittenEventArgs e)
            {

                //var eventLog = new EventLog("Security", System.Environment.MachineName);
                //eventLog.Clear();
                var events = new long[] { 4625 }; //4625
                
                if (events.Contains(e.Entry.InstanceId))
                {
                    Console.WriteLine("4625 detected. triggered IF block");          
                   
                    Console.WriteLine("\n");
                    string eventlogtime = e.Entry.TimeGenerated.ToString();

                    //get the timestamp the log was generated (after d2!)
                    DateTime d1 = DateTime.Parse(eventlogtime);
                    Console.WriteLine("D1 (Event Log Timestamp) " + d1);
                    Bruteforcelogtime = (d1).ToString();

                    //get the timestamp of computer locked (before d1!)
                    DateTime d2 = DateTime.ParseExact(Datetime, "dd:MM:yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    Console.WriteLine( "D2 (Computer Locked) :" + d2);


                    //d1 = event written
                    //d2 = computer locked 
                    //event log should always be written after pc lock. d1>d2
                    int result = DateTime.Compare(d1, d2);
                    Console.WriteLine("Comparing d1,d2 = " + result);
                    if (result >= 0)//d1 later than d2
                    {
                        Console.WriteLine("Login with wrong password was attempted at " + e.Entry.TimeWritten);
                        Console.Write("\n");
                        LoginAttemptCount += 1;
                        SMSlogintimestamps = SMSlogintimestamps + "\n" +Bruteforcelogtime;
                        SendSMS();
                    }
                }
                else
                {
                    Console.WriteLine("there were no brute force attempts ( no 4625) ");
                }

                
                   

                
                //System.IO.File.AppendAllLines(@"d:\log.txt", new string[] { string.Format("{0}:{1}",  e.Entry.InstanceId, e.Entry.Message) });

            }//loglistener

            
        }//LoginLogoutDetector

        public static void SendSMS()
        {
            if (LoginAttemptCount == 3)//3 should change to dynamic variable for final presentation
            {
                MarcusTwilio mt = new MarcusTwilio();
                mt.calltwilio("+6596445769", "\n Failed Login Attempts detected at " + SMSlogintimestamps);//Change to dynamic variable in final presentation
                SMSlogintimestamps = "";
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ContentWrapper.NavigationService.Navigate(new CheckUpdater());
        }


    }
}
