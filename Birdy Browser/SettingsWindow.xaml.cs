using System;
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
using System.Windows.Shapes;

namespace Birdy_Browser
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public dynamic SettingsData { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Tpath.Text = SettingsData["Theme"];
            LPicons.IsChecked = SettingsData["LoadFavicons"];
            Hurl.Text = SettingsData["HomePageUrl"];
            Surl.Text = SettingsData["SearchUrl"];
            if ((int)SettingsData["BookmarksBarVisible"] == 0)
            {
                shbOptionNever.IsChecked = true;
            }else if ((int)SettingsData["BookmarksBarVisible"] == 1)
            {
                shbOptionAlways.IsChecked = true;
            }else if ((int)SettingsData["BookmarksBarVisible"] == 2)
            {
                shbOptionOIN.IsChecked = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SettingsData["Theme"] = Tpath.Text;
            SettingsData["LoadFavicons"] = LPicons.IsChecked;
            SettingsData["HomePageUrl"] = Hurl.Text;
            SettingsData["SearchUrl"] = Surl.Text;
            if ((bool)shbOptionNever.IsChecked)
            {
                SettingsData["BookmarksBarVisible"] = 0;
            }
            if ((bool)shbOptionAlways.IsChecked)
            {
                SettingsData["BookmarksBarVisible"] = 1;
            }
            if ((bool)shbOptionOIN.IsChecked)
            {
                SettingsData["BookmarksBarVisible"] = 2;
            }
        }
    }
}
