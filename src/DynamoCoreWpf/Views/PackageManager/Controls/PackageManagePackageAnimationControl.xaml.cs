using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPackagesControl.xaml
    /// </summary>
    public partial class PackageManagerPackageAnimationControl : UserControl
    {
        private ObservableCollection<string> dummySearchResults { get; set; }

        /// <summary>
        /// A dummy object to match the structure of the package control
        /// </summary>
        public ObservableCollection<string> DummySearchResults
        {
            get { return dummySearchResults; }
            set { dummySearchResults = value; }
        }

        public PackageManagerPackageAnimationControl()
        {
            DataContext = this;

            DummySearchResults = new ObservableCollection<string>();
            for (int i = 0; i < 15; i++)
            {
                DummySearchResults.Add(String.Empty);
            }

            InitializeComponent();
        }
    }
}
