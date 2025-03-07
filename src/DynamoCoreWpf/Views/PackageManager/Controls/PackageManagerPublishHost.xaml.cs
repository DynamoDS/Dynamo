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
using Views.PackageManager.Pages;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishHost.xaml
    /// </summary>
    public partial class PackageManagerPublishHost : UserControl
    {
        //public Window Owner { get; set; }
        public PublishPackageViewModel PublishPackageViewModel { get; set; }
        internal Dynamo.UI.Views.PackageManagerWizard wizard;

        public PackageManagerPublishHost()
        {
            InitializeComponent();

            this.Loaded += InitializeContext;
            this.DataContextChanged += PackageManagerPublishControl_DataContextChanged;

            LoadWizardComponent();
        }
        private void InitializeContext(object sender, RoutedEventArgs e)
        {
            SetDataContext();
            this.Loaded -= InitializeContext;
        }

        private void PackageManagerPublishControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(this.DataContext is PublishPackageViewModel)) return;

            SetDataContext();
        }

        private void SetDataContext()
        {
            // Set the owner of this user control
            //this.Owner = Window.GetWindow(this);

            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
            //PublishPackageViewModel.Owner = this.Owner;

            if (PublishPackageViewModel != null)
            {
                wizard.DataContext = DataContext;
            }
        }

        private async void LoadWizardComponent()
        {
            if (wizard == null)
            {
                try
                {
                    wizard = new Dynamo.UI.Views.PackageManagerWizard();
                    wizard.DataContext = DataContext;

                    this.mainGrid.Children.Add(wizard);

                    await wizard.PreloadWebView2Async();
                }
                catch (Exception ex) {
                    // TODO:replace with Logger somehow
                    Console.WriteLine(ex.Message);
                }
            }
        }

        internal void Dispose()
        {
            wizard?.Dispose();
            wizard = null;
        }
    }
}
