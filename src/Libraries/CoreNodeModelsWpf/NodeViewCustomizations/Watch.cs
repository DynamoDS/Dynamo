using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Wpf.Nodes
{
    public class WatchNodeViewCustomization : INodeViewCustomization<Watch>
    {
        #region Private fields

        private IdentifierNode astBeingWatched;

        private DynamoViewModel dynamoViewModel;

        private Watch watch;
        private SynchronizationContext syncContext;
        private WatchViewModel rootWatchViewModel;

        #endregion

        public void CustomizeView(Watch nodeModel, NodeView nodeView)
        {
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            this.watch = nodeModel;
            this.syncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);

            var watchTree = new WatchTree();

            // make empty watchViewModel
            rootWatchViewModel = new WatchViewModel(this.dynamoViewModel.VisualizationManager);

            // Fix the maximum width/height of watch node.
            nodeView.PresentationGrid.MaxWidth = Configurations.MaxWatchNodeWidth;
            nodeView.PresentationGrid.MaxHeight = Configurations.MaxWatchNodeHeight;
            nodeView.PresentationGrid.Children.Add(watchTree);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;

            Bind(watchTree, nodeView);

            Subscribe();
            ResetWatch();
        }

        private void Bind(WatchTree watchTree, NodeView nodeView)
        {
            // The WatchTree Control is bound to the WatchViewModel
            watchTree.DataContext = rootWatchViewModel;

            // Add binding for TreeView
            var sourceBinding = new Binding("Children")
            {
                Mode = BindingMode.TwoWay,
                Source = rootWatchViewModel,
            };
            watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty, sourceBinding);

            // Add binding for "Show Raw Data" in context menu
            var checkedBinding = new Binding("ShowRawData")
            {
                Mode = BindingMode.TwoWay,
                Source = rootWatchViewModel
            };

            var rawDataMenuItem = new MenuItem
            {
                Header = "Show Raw Data",
                IsCheckable = true,
            };
            rawDataMenuItem.SetBinding(MenuItem.IsCheckedProperty, checkedBinding);
            nodeView.MainContextMenu.Items.Add(rawDataMenuItem);
        }

        private void Subscribe()
        {
            watch.EvaluationComplete += WatchOnEvaluationComplete;
            this.dynamoViewModel.Model.PreferenceSettings.PropertyChanged += PreferenceSettingsOnPropertyChanged;
            rootWatchViewModel.PropertyChanged += RootWatchViewModelOnPropertyChanged;

            watch.InPorts[0].PortConnected += OnPortConnected;
        }

        public void Dispose()
        {
            watch.EvaluationComplete -= WatchOnEvaluationComplete;
            dynamoViewModel.Model.PreferenceSettings.PropertyChanged -= PreferenceSettingsOnPropertyChanged;
            rootWatchViewModel.PropertyChanged -= RootWatchViewModelOnPropertyChanged;

            watch.InPorts[0].PortConnected -= OnPortConnected;
        }

        private void OnPortConnected(PortModel port, ConnectorModel connectorModel)
        {
            Tuple<int, NodeModel> input;

            if (watch.TryGetInput(watch.InPorts.IndexOf(connectorModel.End), out input))
            {
                var oldId = astBeingWatched;
                astBeingWatched = input.Item2.GetAstIdentifierForOutputIndex(input.Item1);
                if (oldId != null && astBeingWatched.Value != oldId.Value)
                {
                    // the input node has changed, we clear preview
                    rootWatchViewModel.Children.Clear();
                }
            }
        }

        private WatchViewModel GetWatchViewModel()
        {
            var inputVar = watch.IsPartiallyApplied
                    ? watch.AstIdentifierForPreview.Name
                    : watch.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name;

            var core = this.dynamoViewModel.Model.EngineController.LiveRunnerRuntimeCore;
            var watchHandler = this.dynamoViewModel.WatchHandler;

            return watchHandler.GenerateWatchViewModelForData(
                watch.CachedValue,
                core,
                inputVar,
                rootWatchViewModel.ShowRawData);
        }

        private void ResetWatch()
        {
            syncContext.Post((_) => SilentUpdate(), null);
        }

        private void SilentUpdate()
        {
            // store in temp variable to silence binding
            var temp = rootWatchViewModel.Children;
            rootWatchViewModel.Children = null;

            temp.Clear();
            temp.Add(this.GetWatchViewModel());

            // rebind
            rootWatchViewModel.Children = temp;
        }

        private void RootWatchViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowRawData")
            {
                ResetWatch();
            }
        }

        private void PreferenceSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LengthUnit" ||
                e.PropertyName == "AreaUnit" ||
                e.PropertyName == "VolumeUnit" ||
                e.PropertyName == "NumberFormat")
            {
                ResetWatch();
            }
        }

        private void WatchOnEvaluationComplete(object o)
        {
            ResetWatch();
        }

    }
}