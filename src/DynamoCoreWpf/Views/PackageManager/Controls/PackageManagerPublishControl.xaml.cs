using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishControl.xaml
    /// </summary>
    public partial class PackageManagerPublishControl : UserControl
    {

        public Window Owner { get; set; }
        public PublishPackageViewModel PublishPackageViewModel { get; set; }
        private Dictionary<int, Page> PublishPages { get; set; }
        private Dictionary<int, DockPanel> NavButtonStacks { get; set; }
        public ObservableCollection<string> Breadcrumbs { get; } = new ObservableCollection<string>();

        public PackageManagerPublishControl()
        {
            InitializeComponent();

            this.Loaded += InitializeContext;
            this.DataContextChanged += PackageManagerPublishControl_DataContextChanged;


            this.mainGrid.Children.Clear(); // clean everything
            LoadWizardComponent();
        }

        private void PackageManagerPublishControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(this.DataContext is PublishPackageViewModel)) return;

            //ResetDataContext();
            SetDataContext();
        }

        private void InitializeContext(object sender, RoutedEventArgs e)
        {
            SetDataContext();

            this.Loaded -= InitializeContext;
        }

        private void SetDataContext()
        {
            wizard.DataContext = DataContext;
        }

        internal Dynamo.UI.Views.PackageManagerWizard wizard;

        // Add the Wizard Component
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
                catch (Exception ex)
                {
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
