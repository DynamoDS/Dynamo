using CefSharp;
using Dynamo.DynamoPackagesUI.Utilities;
using Dynamo.DynamoPackagesUI.ViewModels;
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

namespace DynamoPackagesUI.Views
{
    /// <summary>
    /// Interaction logic for PackageManagerView.xaml
    /// </summary>
    public partial class PackageManagerView : Window
    {
        public PackageManagerView(PackageManagerViewModel viewModel)
        {
            this.DataContext = viewModel;

            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings { RemoteDebuggingPort = 8088 };
                //to fix Fickering set disable-gpu to true
                settings.CefCommandLineArgs.Add("disable-gpu", "1");
                Cef.Initialize(settings);
            }

            //viewModel.PublishPkgCommands.PublishSuccess += PackageViewModelOnPublishSuccess;

            InitializeComponent();

            viewModel.ParentWindow = this;
            //cefHelper object for Explore Packages, Explore Authors and My Packages Tab
            this.cefBrowser.RegisterJsObject("cefHelper", viewModel);
            
            //publishCefHelper for Publish Package Tab 
            //this.cefBrowser.RegisterJsObject("publishCefHelper", viewModel.PublishPkgCommands);

            viewModel.Browser = this.cefBrowser;
            //viewModel.PublishPkgCommands.Browser = this.cefBrowser;

            this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.95);
            this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.75);
        }

        /*private void PackageViewModelOnPublishSuccess(PublishCommands sender)
        {
            this.Dispatcher.BeginInvoke((Action)(Close));
        }*/
    }
}