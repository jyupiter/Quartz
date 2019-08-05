using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Twilio.TwiML.Messaging;

namespace Quartz.MARC
{
    /// <summary>
    /// Interaction logic for MarcusHome.xaml
    /// </summary>
    public partial class MarcusHome : Page
    {
        public static string ConfigPhoneNo = "";
        public static string ConfigTimes = "";
        public static string ConfigEnabled = "";
        public static string ConfigTakePic = "";
        public static string ConfigSMS = "";
        public static string ConfigEmail = "";
        public static string ConfigPassword = "";

        public static string TempConfigPhoneNo = "";
        public static string TempConfigTimes = "";
        public static string TempConfigEnabled = "";
        public static string TempConfigTakePic = "";
        public static string TempConfigSMS = "";
        public static string TempConfigEmail = "";
        public static string TempConfigPassword = "";



        public MarcusHome()
        {
            InitializeComponent();
            ReadConfigFile();
            GetMyCurrentConfig();
        }

        private void GetMyCurrentConfig()
        {
            //TextBox current config
            string IsPasswordSet = "Config password has been set";
            if (ConfigPassword.Equals("n"))
            {
                IsPasswordSet = "Password : NOT set!";
            }

            CurrentConfig.Text = "Phone Number: "+ConfigPhoneNo + "\n" + "Attempts allowed: "+ConfigTimes + "\n" + "Login Guard enabled: "+ConfigEnabled + "\n" + "Intruder Photo: "+ConfigTakePic + "\n" + "Warning SMS: "+ConfigSMS + "\n" + "Send Email: "+ConfigEmail + "\n" + IsPasswordSet;
        }

        //SAVE SETTINGS BUTTON!
        private void LiveScanBtn_Click(object sender, RoutedEventArgs e)
        {
            //Enable / Disable SecureLogin 
            if ((bool) (SecureLoginTick.IsChecked))
            {
               // MessageBox.Show("it is ticked, saved");
                //write to config now 
                WriteEnableDisableSecureLogin("Yes");
            }
            else if ((bool) (!SecureLoginTick.IsChecked))
            {
                MessageBox.Show("not ticky enabled");
                WriteEnableDisableSecureLogin("No");
            }
            ReadConfigFile();//IMPORTANT. Update new config file settings to ConfigXXXX variables
            


            //number of attempts
            string attempts = AttemptsComboBox.Text;
            if (attempts.Equals("1 Attempt"))
            {
                //set writeconfigattempts to 1 
                MessageBox.Show("1 attempt");
                WriteAttempts("1");
            }
            else if (attempts.Equals("2 Attempts"))
            {

                MessageBox.Show("2 attempt");
                WriteAttempts("2");

            }
            else if (attempts.Equals("3 Attempts (default)"))
            {
                MessageBox.Show("3 attempt");
                WriteAttempts("3");
            }
            ReadConfigFile();//call after every update



            GetMyCurrentConfig();//call at the end of this func
        }

        private void EnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            //MessageBox.Show("enabled tick");

            
        }
        

        private void EnabledCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            //MessageBox.Show("enabled untick");
        }

        private void PhoneNoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("hp tick");
        }

        private void PhoneNoCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("hp untick");
        }

        private void AttemptsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("attempts tick");
        }

        private void AttemptsCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("attempts untick");
        }

        private void SMSCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("sms tick");
        }

        private void SMSCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("sms untick");
        }

        private void WebcamCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("webcam tick");
        }

        private void WebcamCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("webcam untick");
        }

        private void EmailCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("Email tick");
        }

        private void EmailCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("Email untick");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }


        public void ReadConfigFile()
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
                int counter = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    string result = line.Substring(line.IndexOf(":") + 1);//Remove all before
                    if (counter == 0)
                    {
                        ConfigPhoneNo = result;
                        Console.WriteLine("Phone number detected : " + ConfigPhoneNo);
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
                        Dispatcher.Invoke(() =>
                        {
                            SecureLoginTick.IsChecked = true;//tick by default

                            if (!ConfigEnabled.Equals("Yes"))// if not yes, untick it
                            {
                                SecureLoginTick.IsChecked = false;
                            }
                        });

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
                        ConfigPassword = result;
                        Console.WriteLine("password option yo: " + ConfigPassword);
                        if (ConfigPassword.Equals("n"))
                        {
                            MessageBox.Show("Looks like you haven't set a password. We recommend it to protect your configuration.");

                        }
                       
                    }
                    counter += 1;
                }

            }

        }//readconfigfile

        //set new password
        private void SetPassword(object sender, RoutedEventArgs e)
        {
            ReadConfigFile();
            if (ConfigPassword.Equals("n"))
            {
                string passwordPromptBox = new InputBox("\nSet a new password").ShowDialog();

                //need a function to write to 6 

                WriteConfigPassword(passwordPromptBox);
                MessageBox.Show("Password set!");
                ReadConfigFile();
                GetMyCurrentConfig();

            }
            else
            {
                //prompt for previous password
                //if match, allow
                //else dont
                string passwordPromptBox = new InputBox("\nEnter EXISTING password").ShowDialog();
                if (passwordPromptBox.Equals(ConfigPassword))
                {
                    //if match, ask for new password
                    string updatedPasswordPrompt = new InputBox("\nEnter NEW password").ShowDialog();
                    if (updatedPasswordPrompt.Equals(""))
                    {
                        MessageBox.Show("Password NOT updated");
                    }
                    else
                    {
                        WriteConfigPassword(updatedPasswordPrompt);
                        MessageBox.Show("Password Updated!");
                        ReadConfigFile();
                        GetMyCurrentConfig();
                    }
                    
                }
                else
                {
                    MessageBox.Show("Wrong password");
                }

            }

        }

        //remove password
        private void RemovePassword(object sender, RoutedEventArgs e)
        {
            ReadConfigFile();
            if (ConfigPassword.Equals("n"))
            {
                MessageBox.Show("You haven't set a password, nothing to remove leh.");
            }
            else
            {
                string passwordPromptBox = new InputBox("\nEnter Current Password").ShowDialog();
                if (passwordPromptBox.Equals(ConfigPassword))
                {
                    string sMessageBoxText = "Do you want to remove your password? (we don't recommend this!)";
                    string sCaption = "Password removal";

                    MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                    switch (rsltMessageBox)
                    {
                        case MessageBoxResult.Yes:
                            /* ... */
                            WriteConfigPassword("n");
                            MessageBox.Show("Password has been REMOVED");
                            break;

                        case MessageBoxResult.No:
                            /* ... */
                            MessageBox.Show("No changes made");
                            break;




                    }
                }
            }
            
            ReadConfigFile();
            GetMyCurrentConfig();
        }
        //forgot pw 

        public static void WriteConfigPassword(string newpassword)//just re-creates the entire file
        {
            //get desktop path 
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath += @"\MarcConfig.txt";

            //if config file doesn't exist, create one 
            if (File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
                File.AppendAllLines(filePath, new[] { "hp:"+ConfigPhoneNo, "times:"+ConfigTimes, "enabled:"+ConfigEnabled, "takepic:"+ConfigTakePic, "sms:"+ConfigSMS, "email:"+ConfigEmail, "password:"+newpassword });
            }
            else
            {
                Console.WriteLine("Config file exists");
            }

        }//WriteConfigPassword

        public static void WriteEnableDisableSecureLogin(string enabled)
        {
            //get desktop path 
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath += @"\MarcConfig.txt";

            //if config file doesn't exist, create one 
            if (File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
                File.AppendAllLines(filePath, new[] { "hp:" + ConfigPhoneNo, "times:" + ConfigTimes, "enabled:" + enabled, "takepic:" + ConfigTakePic, "sms:" + ConfigSMS, "email:" + ConfigEmail, "password:" + ConfigPassword });
            }
            else
            {
                Console.WriteLine("Config file exists");
            }
        }

        public static void WriteAttempts(string attempts)
        {
            //get desktop path 
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath += @"\MarcConfig.txt";

            //if config file doesn't exist, create one 
            if (File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
                File.AppendAllLines(filePath, new[] { "hp:" + ConfigPhoneNo, "times:" + attempts, "enabled:" + ConfigEnabled, "takepic:" + ConfigTakePic, "sms:" + ConfigSMS, "email:" + ConfigEmail, "password:" + ConfigPassword });
            }
            else
            {
                Console.WriteLine("Config file exists");
            }
        }

        //for set new password 
        public class InputBox
        {

            Window Box = new Window();//window for the inputbox

            FontFamily font = new FontFamily("Tahoma");//font for the whole inputbox
            int FontSize = 20;//fontsize for the input
            StackPanel sp1 = new StackPanel();// items container
            string title = "Enter Password";//title as heading
            string boxcontent;//title
            string defaulttext = "Default text";//not used but leave it alone
            string errormessage = "Very funny, password can't be left empty!";//error messagebox content
            string errortitle = "Error Message";//error messagebox heading title
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

        
        
    }
}
