using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.TestLinterExtension.LinterRules;
using Dynamo.ViewModels;
using Dynamo.Extensions;
using Dynamo.Linting;
using Dynamo.Linting.Rules;
using Dynamo.Linting.Interfaces;
using Dynamo.Wpf.Extensions;
using System.Windows.Controls;
using System.Windows;
using System;

namespace Dynamo.TestLinterExtension
{
    public class TestLinterExtension : LinterExtensionBase, IViewExtension
    {
        private MenuItem testLinterMenuItem;
        private ViewLoadedParams viewLoadedParamsReference;

        public override string UniqueId => "a7ad5249-10ea-4fbf-b2f6-7f9658773850";

        public override string Name => "Test Linter ViewExtension";

        #region Extension Lifecycle

        public override void Ready(ReadyParams sp)
        {
            base.Ready(sp);

            this.AddLinterRule(new NodesCantBeNamedFooRule());
            this.AddLinterRule(new InputNodesNotAllowedRule());
            this.AddLinterRule(new GraphNeedsOutputNodesRule());
        }

        public override void Dispose() { }
        public override void Shutdown() { }

        public void Startup(ViewStartupParams viewStartupParams)
        {
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            this.viewLoadedParamsReference = viewLoadedParams;
            this.testLinterMenuItem = new MenuItem { Header = "TestLinter", IsCheckable = true };
            this.testLinterMenuItem.Checked += MenuItemCheckHandler;
            this.viewLoadedParamsReference.AddExtensionMenuItem(this.testLinterMenuItem);
        }

        private void MenuItemCheckHandler(object sender, RoutedEventArgs e)
        {
            var viewModel = this.viewLoadedParamsReference.DynamoWindow.DataContext as DynamoViewModel;
            var linterManager = viewModel.Model.LinterManager;
            foreach (var linter in linterManager.AvailableLinters)
            {
                if (linter.Id == this.UniqueId)
                    linterManager.ActiveLinter = linter;

            }
        }

        #endregion

    }
}
