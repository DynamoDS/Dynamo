using Dynamo.ViewModels;
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

namespace Dynamo.Wpf.Views.PackageManager
{
    /// <summary>
    /// Interaction logic for PackagePathView.xaml
    /// </summary>
    public partial class PackagePathView : Window
    {
        public PackagePathView()
        {
            InitializeComponent();
            PathListBox.ItemsSource = new List<string>
            {
                "PathOne",
                "PathTwo"
            };
        }

        internal PackagePathView(PackagePathViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            InitializeComponent();
            PathListBox.ItemsSource = viewModel.RootLocations;
        }
    }
}
