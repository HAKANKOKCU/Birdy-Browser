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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Birdy_Browser
{
    /// <summary>
    /// Interaction logic for FlatButton.xaml
    /// </summary>
    public partial class FlatButton : UserControl
    {
        public FlatButton()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        /// <summary>
        /// Gets or sets additional content for the UserControl
        /// </summary>
        public object AdditionalContent
        {
            get { return (object)GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }
        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register("AdditionalContent", typeof(object), typeof(FlatButton),
              new PropertyMetadata(null));

        public SolidColorBrush hoverBrush { get; set; } = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        public SolidColorBrush downBrush { get; set; } = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Border).Background = hoverBrush;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Border).Background = Brushes.Transparent;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as Border).Background = downBrush;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as Border).Background = Brushes.Transparent;
        }

        private void Border_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as Border).Background = hoverBrush;
            Click(this, new RoutedEventArgs());
        }

        private void Border_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            Click(this, new RoutedEventArgs());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }

}
