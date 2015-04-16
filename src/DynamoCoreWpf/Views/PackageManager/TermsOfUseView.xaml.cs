using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

using Dynamo.ViewModels;
using Dynamo;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for TermsOfUseView.xaml
    /// </summary>
    public partial class TermsOfUseView : Window
    {
        private DynamoViewModel dynamoViewModel;

        public TermsOfUseView(DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            this.dynamoViewModel = dynamoViewModel;
            DataContext = dynamoViewModel;
        }

        private void AcceptTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            dynamoViewModel.PreferenceSettings.PackageDownloadTouAccepted = true;
            Close();
        }

        private void DeclineTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
