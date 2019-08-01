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
        public static String datetime = "";
        public static string bruteforcelogtime = "";
        public static Boolean bruteforce = false;
        public MainWindow()
        {
            InitializeComponent();
            //marcustimer();// delete if not used in final product
            //calltwilio();
            marcusLoginLogoutDetector();//entry point for marcus function
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
            EventLog logListener = new EventLog("Security");
            logListener.EntryWritten -= logListener_EntryWritten;//unregister prev subscribers
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            

            void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    //User locks screen
                    datetime = (DateTime.Now.ToString("dd:MM:yyyy hh:mm:ss tt")).ToString();
                    logListener.EntryWritten -= logListener_EntryWritten;//unregister prev subscribers
                    logListener.EntryWritten += logListener_EntryWritten;//register 
                    logListener.EnableRaisingEvents = true;

                    
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    //User logs back in
                    if (bruteforce)
                    {
                        MessageBox.Show("computer was bruteforced");
                        Console.WriteLine("computer was locked at" + datetime);
                        Console.WriteLine("brute force occured at :" + bruteforcelogtime);

                    }

                    bruteforce = false;
                }
            }

            void logListener_EntryWritten(object sender, EntryWrittenEventArgs e)
            {
               

                //var eventLog = new EventLog("Security", System.Environment.MachineName);
                //eventLog.Clear();
                //MessageBox.Show("all security event logs cleared");
                //4624: An account was successfully logged on.
                //4625: An account failed to log on.
                //4648: A logon was attempted using explicit credentials.
                //4675: SIDs were filtered.
                var events = new long[] { 4648 }; //4624 4625 4675

                if (events.Contains(e.Entry.InstanceId))
                    Console.WriteLine("Wrong password detected " + e.Entry.TimeWritten);
                    bruteforce = true;
  

                    string eventlogtime = e.Entry.TimeGenerated.ToString();
                    DateTime d1 = DateTime.Parse(eventlogtime);
                    Console.WriteLine("Event log timestamp d1 is now "+ d1);
                    bruteforcelogtime = (d1).ToString();
                    

                    DateTime d2 = DateTime.ParseExact(datetime, "dd:MM:yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    Console.WriteLine("Time computer was locked d2 is now " + d2);
                    //d2 = time when pc locked
                    //d1 = time event log was written
                    //event log should always be written after pc lock. d1>d2
                    int result = DateTime.Compare(d1,d2); //d1 should be after current time 
                    Console.WriteLine("post comparison. int result = " + result);
                    if (result >= 0 && bruteforce)//d1 later than d2
                    {
                        Console.WriteLine("Wrong password detected " + e.Entry.TimeWritten);
                        
                    }

                   

                
                //System.IO.File.AppendAllLines(@"d:\log.txt", new string[] { string.Format("{0}:{1}",  e.Entry.InstanceId, e.Entry.Message) });

            }

            
        }

        public void calltwilio()
        {
            // Find your Account Sid and Token at twilio.com/console
            // DANGER! This is insecure. See http://twil.io/secure
            const string accountSid = "AC4eba0a962c64efbeedd19d4aeb101be1";
            const string authToken = "aa59862f86ebeb7ed9485d2bc4783fdf";

            TwilioClient.Init(accountSid, authToken);

            try
            {
                var message = MessageResource.Create(
                    body: "This is the ship that made the Kessel Run in fourteen parsecs?",
                    from: new Twilio.Types.PhoneNumber("+12055576024"),//DO NOT CHANGE
                    to: new Twilio.Types.PhoneNumber("+6596445769")// will need to un-static this thing to a variable with user's HP
                );

                Console.WriteLine(message.Sid);
            }
            catch (ApiException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine($"Twilio Error {e.Code} - {e.MoreInfo}");
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ContentWrapper.NavigationService.Navigate(new CheckUpdater());
        }


    }
}
