using Dynamo.DynamoPackagesUI.ViewModels;
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
        private PackageManagerView _pkgMgrView;
        private PackageManagerViewModel _pkgMgrViewModel;

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
            

            if (_pkgMgrView == null)
            {
                var vm = new PackageManagerViewModel((DynamoViewModel)viewLoadedParams.DynamoWindow.DataContext, "assets");
                _pkgMgrView = new PackageManagerView(vm)
                {
                    Owner = viewLoadedParams.DynamoWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                //var vm = new PackageManagerViewModel("assets");
                _pkgMgrView.DataContext = vm;

                _pkgMgrView.Closed += (sender, args) => _pkgMgrView = null;
                _pkgMgrView.Show();
            }
            _pkgMgrView.Focus();
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