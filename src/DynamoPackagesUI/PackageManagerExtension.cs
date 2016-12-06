using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using DynamoPackagesUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.DynamoPackagesUI
{
    public class PackageManagerExtension : IViewExtension
    {
        private PackageManagerView view;
        private PackageManagerViewModel viewModel;

        private ViewLoadedParams viewLoadedParams;
        private ViewStartupParams viewStartupParams;

        private MenuItem packageManagerMenuItem;
        private static bool enablePackageManager = false;

        public static bool EnablePackageManager { get { return enablePackageManager; } }

        public string UniqueId
        {
            get { return "4b6f94ba-3248-4311-8ac4-fd7f660abbfb"; }
        }

        public string Name
        {
            get { return "PackageManager"; }
        }

        public void Startup(ViewStartupParams p)
        {
            viewStartupParams = p;
        }
        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            AddMenuItem(p);
        }

        private void AddMenuItem(ViewLoadedParams p)
        {
#if DEBUG
            p.AddSeparator(MenuBarType.Packages, new Separator());
            packageManagerMenuItem = new MenuItem() { Header = "Package Manager", Name = "PackageManager" };
            packageManagerMenuItem.Click += (object sender, RoutedEventArgs e) => OnPackageManagerClick();
            p.AddMenuItem(MenuBarType.Packages, packageManagerMenuItem);
#endif
        }

        private void OnPackageManagerClick()
        {
            

            if (view == null)
            {
                var vm = new PackageManagerViewModel(((DynamoViewModel)viewLoadedParams.DynamoWindow.DataContext).BrandingResourceProvider, 
                    ((DynamoViewModel)viewLoadedParams.DynamoWindow.DataContext).Model.GetPackageManagerExtension().PackageLoader, ((DynamoViewModel)viewLoadedParams.DynamoWindow.DataContext).Model,  "assets");
                view = new PackageManagerView(vm)
                {
                    Owner = viewLoadedParams.DynamoWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                //var vm = new PackageManagerViewModel("assets");
                view.DataContext = vm;

                view.Closed += (sender, args) => view = null;
                view.Show();
            }
            view.Focus();
            //if (_packageManagerView.IsLoaded && IsLoaded) _packageManagerView.Owner = this;
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {

        }

    }
}