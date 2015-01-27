using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dynamo.Applications
{
    public partial class BrowserWindow : Window
    {
        private readonly Uri _location;

        public BrowserWindow(Uri location)
        {
            this._location = location;
            this.Loaded += Window_Loaded;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // hide loading grid when navigation ready
            this.Browser.Navigated += (s, a) =>
            {
                this.LoadingGrid.Visibility = Visibility.Collapsed;
                this.Browser.Visibility = Visibility.Visible;
            };
            this.Browser.Navigate( _location.AbsoluteUri );
        }
    }
}
