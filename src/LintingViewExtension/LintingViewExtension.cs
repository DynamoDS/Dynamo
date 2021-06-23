﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Graph;
using Dynamo.Linting;
using Dynamo.LintingViewExtension.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.LintingViewExtension
{
    public class LintingViewExtension : ViewExtensionBase
    {
        private const string EXTENSION_NAME = "Graph Status";
        private const string EXTENSION_GUID = "3467481b-d20d-4918-a454-bf19fc5c25d7";

        private LinterManager linterManager;
        private ViewLoadedParams viewLoadedParamsReference;
        private MenuItem linterMenuItem;
        private LinterViewModel linterViewModel;
        private LinterView linterView;


        public override string UniqueId { get { return EXTENSION_GUID; } }

        public override string Name { get { return EXTENSION_NAME; } }

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            this.linterManager = (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).Model.LinterManager;
            this.viewLoadedParamsReference = viewLoadedParams;
            this.linterViewModel = new LinterViewModel(linterManager, viewLoadedParamsReference);
            this.linterView = new LinterView() { DataContext = linterViewModel };

            (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).RequestOpenLinterView += OnRequestOpenLinterView;

            // Add a button to Dynamo View menu to manually show the window
            this.linterMenuItem = new MenuItem { Header = Resources.MenuItemText, IsCheckable = true };
            this.linterMenuItem.Checked += MenuItemCheckHandler;
            this.linterMenuItem.Unchecked += MenuItemUnCheckedHandler;
            if(linterManager.AvailableLinters.Count > 1) { viewLoadedParamsReference.AddExtensionMenuItem(this.linterMenuItem); }

            this.linterManager.PropertyChanged += OnLinterManagerPropertyChange;
        }

        private void OnRequestOpenLinterView(object sender, System.EventArgs e)
        {
            if (linterMenuItem.IsChecked) return;
            linterMenuItem.IsChecked = true;
        }

        public override void Shutdown()
        {
            // Do nothing for now 
        }

        public override void Dispose()
        {
            this.linterMenuItem.Checked -= MenuItemCheckHandler;
            this.linterMenuItem.Unchecked -= MenuItemUnCheckedHandler;
            if (linterManager != null) linterManager.PropertyChanged -= OnLinterManagerPropertyChange;
        }

        public override void Closed()
        {
            if (this.linterMenuItem is null)
                return;
            
            this.linterMenuItem.IsChecked = false;
        }

        private void OnLinterManagerPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(linterManager.AvailableLinters))
            {
                viewLoadedParamsReference.AddExtensionMenuItem(this.linterMenuItem);
            }
        }

        private void MenuItemUnCheckedHandler(object sender, RoutedEventArgs e)
        {
            viewLoadedParamsReference.CloseExtensioninInSideBar(this);
        }

        private void MenuItemCheckHandler(object sender, RoutedEventArgs e)
        {
            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.linterView);
        }
    }
}
