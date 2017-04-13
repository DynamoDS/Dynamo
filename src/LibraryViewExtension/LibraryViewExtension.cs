using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.LibraryUI.ViewModels;
using Dynamo.LibraryUI.Views;
using Dynamo.Wpf.Extensions;

namespace Dynamo.LibraryUI
{
    public class ViewExtension : IViewExtension
    {
        private ViewLoadedParams viewLoadedParams;
        private ViewStartupParams viewStartupParams;

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
        }
        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            controller = new LibraryViewController(p.DynamoWindow, p.CommandExecutive);
            controller.AddLibraryView();
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