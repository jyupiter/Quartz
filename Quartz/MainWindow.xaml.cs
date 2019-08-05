using Quartz.AG;
using Quartz.HQ;
using Quartz.MARC;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using Quartz.Classes;

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
        public static string ConfigPhoneNo = "";
        public static string ConfigTimes = "";
        public static string ConfigEnabled = "";
        public static string ConfigTakePic = "";//
        public static string ConfigSMS = "";
        public static string ConfigEmail = "";
        public static string ConfigPass = "";
        int passcount = 0;




        public MainWindow()
        {
            InitializeComponent();
			//marcustimer();// delete if not used in final product
			//calltwilio();
			_Graphs.initGraphs();
            LoginLogoutDetector();//entry point for M's function
            CreateConfigFile();// Marcus's config file
            ReadConfigFile();//Marcus's config file 
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

        private void RedirectToUpdates(object sender, RoutedEventArgs e)
        {
            ContentWrapper.NavigationService.Navigate(new CheckUpdater());
        }

        private void RedirectToHome(object sender, RoutedEventArgs e)
        {
            ContentWrapper.NavigationService.Navigate(new Home());
        }

        #region Marcus

        /*Marcus page*/
        private void RedirectMarcusHome(object sender, RoutedEventArgs e)
        {
            ReadConfigFile();//Marcus's config file 
            //check if user set a password for SecLogin
            if (ConfigPass.Equals("n"))
            {
                //pw not set 
                ContentWrapper.NavigationService.Navigate(new MarcusHome());
            }
            else 
            {
                //user has password set
                string passwordPromptBox = new InputBox("\nEnter Password").ShowDialog();

                if (passwordPromptBox.Equals(ConfigPass))//password match 
                {
                    ContentWrapper.NavigationService.Navigate(new MarcusHome());
                }
                else
                {
                    MessageBox.Show("Wrong password!");//password not match
                    passcount += 1;
                    if (passcount == 2)
                    {
                        string sMessageBoxText = "Do you want to reset your password??";
                        string sCaption = "Multiple failed password attempts";

                        MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                        MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                        MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                        switch (rsltMessageBox)
                        {
                            case MessageBoxResult.Yes:
                                /* ... */
                                MarcusTwilio msg = new MarcusTwilio();
                                msg.calltwilio(ConfigPhoneNo, "A reset for your password was requested." +ConfigPass);
                                MessageBox.Show("A reset has been sent to your SMS / Email");
                                passcount = 0;
                                break;

                            case MessageBoxResult.No:
                                /* ... */
                                break;

                        }

                        passcount = 0;
                    }
                }
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
                    if (ConfigEnabled.Equals("Yes"))
                    {
                        Datetime = (DateTime.Now.ToString("dd:MM:yyyy hh:mm:ss tt")).ToString();
                        logListener.EntryWritten -= logListener_EntryWritten;//unregister prev subscribers
                        logListener.EntryWritten += logListener_EntryWritten;//register 
                        logListener.EnableRaisingEvents = true;
                    }
                    
                    
                   

                    
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    if (ConfigEnabled.Equals("Yes"))
                    {
                        Microsoft.Win32.SystemEvents.SessionSwitch -= new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
                    }

                    
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
                LoginAttemptCount = 0;
            }
        }

        //always called to check if file exists, create file if file doesn't exist
        public static void CreateConfigFile()
        {
            //get desktop path 
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath += @"\MarcConfig.txt";
            
            //if config file doesn't exist, create one 
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
                File.AppendAllLines(filePath, new[] { "hp:00000000" , "times:3" , "enabled:Yes" , "takepic:Yes" , "sms:Yes" , "email:Yes" , "password:n"});
            }
            else
            {
                Console.WriteLine("Config file exists");
            }
            
        }//CreateConfigFile

        //always called to read the current config.
        public static void ReadConfigFile()
        {
            //read from config file 
            //get desktop path 
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath += @"\MarcConfig.txt";

            const Int32 BufferSize = 1024;
            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                int counter=0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    string result = line.Substring(line.IndexOf(":") + 1);//Remove all before
                    if (counter == 0)
                    {
                        ConfigPhoneNo = result;
                        Console.WriteLine("Phone number detected : "+ ConfigPhoneNo); 
                    }

                    if (counter == 1)
                    {
                        ConfigTimes = result;
                        Console.WriteLine("Attempt counter detected: " + ConfigTimes);
                    }

                    if (counter == 2)
                    {
                        ConfigEnabled = result;
                        Console.WriteLine("enable/disabled option detected: " + ConfigEnabled);
                    }

                    if (counter == 3)
                    {
                        ConfigTakePic = result;
                        Console.WriteLine("Webcam take photo option detected: " + ConfigTakePic);
                    }

                    if (counter == 4)
                    {
                        ConfigSMS = result;
                        Console.WriteLine("SMS warning option detected: " + ConfigSMS);
                    }

                    if (counter == 5)
                    {
                        ConfigEmail = result;
                        Console.WriteLine("Email option detected: " + ConfigEmail);
                    }

                    if (counter == 6)
                    {
                        ConfigPass = result;
                        Console.WriteLine("Password set option " + ConfigPass);
                    }
                    counter += 1;
                }
              
            }
            
        }//readconfigfile

        //only call if user changes their secLogin settings
        public static void WriteToConfigFile(string phoneNo , string attempts, string option)
        {
            //get desktop path 
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath += @"\MarcConfig.txt";

            //TRAIN OF THOUGHTS ENDED HERE BEFORE SLEEPING
            //open file line by line
            //edit each line after : 
            //done 
            
        }//WriteToConfigFile

        public class InputBox
        {

            Window Box = new Window();//window for the inputbox
           
            FontFamily font = new FontFamily("Tahoma");//font for the whole inputbox
            int FontSize = 20;//fontsize for the input
            StackPanel sp1 = new StackPanel();// items container
            string title = "Enter Password";//title as heading
            string boxcontent;//title
            string defaulttext = "Default text";//default textbox content
            string errormessage = "Password can't be empty";//error messagebox content
            string errortitle = "Error";//error messagebox heading title
            string okbuttontext = "Enter";//Ok button content
            Brush BoxBackgroundColor = Brushes.Azure;// Window Background
            Brush InputBackgroundColor = Brushes.Ivory;// Textbox Background
            bool clicked = false;
            PasswordBox input = new PasswordBox();
            Button ok = new Button();
            bool inputreset = false;

            public InputBox(string content)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Error!"; }
                windowdef();
            }

            public InputBox(string content, string Htitle, string DefaultText)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Error!"; }
                try
                {
                    title = Htitle;
                }
                catch
                {
                    title = "Error!";
                }
                try
                {
                    defaulttext = DefaultText;
                }
                catch
                {
                    DefaultText = "Error!";
                }
                windowdef();
            }

            public InputBox(string content, string Htitle, string Font, int Fontsize)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Error!"; }
                try
                {
                    font = new FontFamily(Font);
                }
                catch { font = new FontFamily("Tahoma"); }
                try
                {
                    title = Htitle;
                }
                catch
                {
                    title = "Error!";
                }
                if (Fontsize >= 1)
                    FontSize = Fontsize;
                windowdef();
            }

            private void windowdef()// window building - check only for window size
            {
                Box.Height = 250;// Box Height
                Box.Width = 300;// Box Width
                Box.Background = Brushes.White;
                Box.Title = title;
                Box.Content = sp1;
                //Box.Closing += Box_Closing;
                TextBlock content = new TextBlock();
                content.TextWrapping = TextWrapping.Wrap;
                content.Background = null;
                content.HorizontalAlignment = HorizontalAlignment.Center;
                content.Text = boxcontent;
                content.FontFamily = font;
                content.FontSize = FontSize;
                sp1.Children.Add(content);

                input.Background = InputBackgroundColor;
                input.FontFamily = font;
                input.FontSize = FontSize;
                input.HorizontalAlignment = HorizontalAlignment.Center;
                //input.Text = defaulttext;
                input.MinWidth = 200;
               // input.MouseEnter += input_MouseDown;
                
                sp1.Children.Add(input);
                ok.Width = 70;
                ok.Height = 30;
                ok.Click += ok_Click;
                ok.Content = okbuttontext;
                ok.HorizontalAlignment = HorizontalAlignment.Center;
                sp1.Children.Add(ok);

            }

            //void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            //{
            //    if (!clicked)
            //        e.Cancel = true;
            //}

            //private void input_MouseDown(object sender, MouseEventArgs e)
            //{
            //    if ((sender as TextBox).Text == defaulttext && inputreset == false)
            //    {
            //        (sender as TextBox).Text = null;
            //        inputreset = true;
            //    }
            //}

            void ok_Click(object sender, RoutedEventArgs e)
            {
                clicked = true;
                if (input.Password == defaulttext || input.Password == "")
                    MessageBox.Show(errormessage, errortitle);
                else
                {
                    Box.Close();
                }
                clicked = false;
            }

            public string ShowDialog()
            {
                Box.ShowDialog();
                return input.Password;
            }
        }

        #endregion

    }


}

