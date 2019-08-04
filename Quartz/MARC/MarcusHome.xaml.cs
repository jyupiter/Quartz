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
   


        public MarcusHome()
        {
            InitializeComponent();
            ReadConfigFile();
            GetMyCurrentConfig();
        }

        private void GetMyCurrentConfig()
        {
            string IsPasswordSet = "Config password has been set";
            if (ConfigPassword.Equals("n"))
            {
                IsPasswordSet = "No Password Set!";
            }

            CurrentConfig.Text = "Phone Number: "+ConfigPhoneNo + "\n" + "Attempts allowed: "+ConfigTimes + "\n" + "Login Guard enabled: "+ConfigEnabled + "\n" + "Intruder Photo: "+ConfigTakePic + "\n" + "Warning SMS: "+ConfigSMS + "\n" + "Send Email: "+ConfigEmail + "\n" + IsPasswordSet;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("sms tick");
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
            MessageBox.Show("sms untick");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }


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
            string passwordPromptBox = new InputBox("\nSet a new password").ShowDialog();
             
            //need a function to write to 6 

            WriteConfigPassword(passwordPromptBox);
            MessageBox.Show("Password set!");
        }

        //change pw

        //remove pw

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

        }//CreateConfigFile



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
            string errormessage = "Error message";//error messagebox content
            string errortitle = "Error message title";//error messagebox heading title
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
