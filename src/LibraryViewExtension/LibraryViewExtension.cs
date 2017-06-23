using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.LibraryUI.ViewModels;
using Dynamo.LibraryUI.Views;
using Dynamo.Wpf.Extensions;
using Dynamo.PackageManager;
using Dynamo.Models;
using System.Linq;
using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.LibraryUI
{
    public class ViewExtension : IViewExtension
    {
        private ViewLoadedParams viewLoadedParams;
        private ViewStartupParams viewStartupParams;
        private ILibraryViewCustomization customization = new LibraryViewCustomization();
        private LibraryViewController controller;

        public string UniqueId
        {
            get { return "85941358-5525-4FF4-8D61-6CA831F122AB"; }
        }

        public static readonly string ExtensionName = "LibraryUI";

        public string Name
        {
            get { return ExtensionName; }
        }

        public void Startup(ViewStartupParams p)
        {
            viewStartupParams = p;
            p.ExtensionManager.RegisterService(customization);
        }

        public void Loaded(ViewLoadedParams p)
        {
            if (!DynamoModel.IsTestMode)
            {
                viewLoadedParams = p;
                controller = new LibraryViewController(p.DynamoWindow, p.CommandExecutive, customization);
                controller.AddLibraryView();
            }
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {

        }
    }

    public static class DynamoModelExtensions
    {
        public static PackageManagerExtension GetPackageManagerExtension(this DynamoModel model)
        {
            return PackageManager.DynamoModelExtensions.GetPackageManagerExtension(model);
        }
    }
}