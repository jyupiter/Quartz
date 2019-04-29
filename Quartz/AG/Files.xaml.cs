using System.Windows;
using System.Windows.Controls;

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

        private void RedirectToFileComparer(object sender, RoutedEventArgs e)
        {
            FilesWrapper.NavigationService.Navigate(new FileComparer());
        }
    }
}
