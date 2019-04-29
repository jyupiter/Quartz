﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Quartz.AG
{
    /// <summary>
    /// Interaction logic for Downloads.xaml
    /// </summary>
    public partial class Files : Page
    {
        public Files()
        {
            InitializeComponent();
        }

        private void RedirectToFileWatcher(object sender, RoutedEventArgs e)
        {
            FilesWrapper.NavigationService.Navigate(new FileWatcher());
        }
    }
}
