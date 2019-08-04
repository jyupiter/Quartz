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
            EditTextBox();
        }

        private void EditTextBox()
        {
            CurrentConfig.Text = "john";
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //sms checkbox
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
    }
}
