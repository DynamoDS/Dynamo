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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Controls;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for TitleBarButtons.xaml
    /// </summary>
    public partial class TitleBarButtons : UserControl
    {
        DynamoView dynamoView = null;

        public TitleBarButtons(DynamoView dynamoView)
        {
            InitializeComponent();
            this.dynamoView = dynamoView;
        }

        #region event handlers
        private void OnWindowMinimize(object sender, RoutedEventArgs e)
        {
            dynamoView.WindowState = WindowState.Minimized;
        }

        private void OnWindowMaximize(object sender, RoutedEventArgs e)
        {
            if (dynamoView.WindowState == WindowState.Maximized)
                dynamoView.WindowState = WindowState.Normal;
            else
                dynamoView.WindowState = WindowState.Maximized;
        }

        private void OnWindowClose(object sender, RoutedEventArgs e)
        {
            dynamoView.Close();
        }
        #endregion
    }
}
