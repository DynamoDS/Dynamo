using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Dynamo.ViewModels;


namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for TermsOfUseView.xaml
    /// </summary>
    public partial class TermsOfUseView : Window
    {
        public bool AcceptedTermsOfUse { get; private set; }

        public TermsOfUseView(string touFilePath)
        {
            InitializeComponent();
            AcceptedTermsOfUse = false;
            TermsOfUseContent.File = touFilePath;
        }

        private void AcceptTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            AcceptedTermsOfUse = true;
            Close();
        }

        private void DeclineTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
