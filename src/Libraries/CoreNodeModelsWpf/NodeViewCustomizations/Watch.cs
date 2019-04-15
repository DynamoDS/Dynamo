using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Interfaces;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModelsWpf.Nodes
{
    public class WatchNodeViewCustomization : INodeViewCustomization<Watch>
    {
        #region Private fields

        private IdentifierNode astBeingComputed;

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
            rootWatchViewModel = new WatchViewModel(dynamoViewModel.BackgroundPreviewViewModel.AddLabelForPath);

            // Fix the maximum width/height of watch node.
            nodeView.PresentationGrid.MaxWidth = Configurations.MaxWatchNodeWidth;
            nodeView.PresentationGrid.MaxHeight = Configurations.MaxWatchNodeHeight;
            nodeView.PresentationGrid.Children.Add(watchTree);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;
            // disable preview control
            nodeView.TogglePreviewControlAllowance();

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

            // add binding for ItemCount
            var numItemsBinding = new Binding("NumberOfItems")
            {
                Mode = BindingMode.TwoWay,
                Source = rootWatchViewModel,
            };
            watchTree.ListItems.SetBinding(ItemsControl.ItemsSourceProperty, numItemsBinding);

            // add binding for depth of list
             var listlevelBinding = new Binding("Levels")
            {
                Mode = BindingMode.TwoWay,
               Source = rootWatchViewModel,
            };
            watchTree.listLevelsView.SetBinding(ItemsControl.ItemsSourceProperty, listlevelBinding);

            // Add binding for "Show Raw Data" in context menu
            var checkedBinding = new Binding("ShowRawData")
            {
                Mode = BindingMode.TwoWay,
                Source = rootWatchViewModel
            };

            var rawDataMenuItem = new MenuItem
            {
                Header = Dynamo.Wpf.Properties.Resources.WatchNodeRawDataMenu,
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

            watch.PortConnected += OnPortConnected;
            watch.PortDisconnected += OnPortDisconnected;
        }

        public void Dispose()
        {
            watch.EvaluationComplete -= WatchOnEvaluationComplete;
            dynamoViewModel.Model.PreferenceSettings.PropertyChanged -= PreferenceSettingsOnPropertyChanged;
            rootWatchViewModel.PropertyChanged -= RootWatchViewModelOnPropertyChanged;

            watch.PortConnected -= OnPortConnected;
            watch.PortDisconnected -= OnPortDisconnected;
        }

        private void OnPortConnected(PortModel port, ConnectorModel connectorModel)
        {
            Tuple<int, NodeModel> input;

            if (!watch.TryGetInput(watch.InPorts.IndexOf(connectorModel.End), out input)
                || astBeingComputed == null) return;

            var astBeingWatched = input.Item2.GetAstIdentifierForOutputIndex(input.Item1);

            // In the case of code block nodes in error state, astBeingWatched can return null 
            // but since we still wish to retain its connectors (https://github.com/DynamoDS/Dynamo/pull/7401) 
            // we simply return from here.
            if (astBeingWatched == null) return;

            if (astBeingComputed.Value != astBeingWatched.Value)
            {
                // the input node has changed, we clear preview
                rootWatchViewModel.Children.Clear();
            }
            else
            {
                ResetWatch();
            }
        }

        private void OnPortDisconnected(PortModel obj)
        {
            ResetWatch();
        }

        private WatchViewModel GetWatchViewModel()
        {
            var inputVar = watch.IsPartiallyApplied
                    ? watch.AstIdentifierForPreview.Name
                    : watch.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name;

            var core = this.dynamoViewModel.Model.EngineController.LiveRunnerRuntimeCore;
            var watchHandler = this.dynamoViewModel.WatchHandler;

            return watchHandler.GenerateWatchViewModelForData(
                watch.CachedValue, watch.OutPorts.Select(p => p.Name),
                core,
                inputVar,
                rootWatchViewModel.ShowRawData);
        }

        private void ResetWatch()
        {
            // When the node has no input connected, the preview should be empty
            // Without doing this, the preview would say "null"
            if (watch.IsPartiallyApplied)
            {
                // There should be only one node in rootWatchViewModel.Children
                // as it is the parent node. Therefore, the iteration should only occur once.
                foreach (var node in rootWatchViewModel.Children)
                {
                    // remove all labels (in Watch 3D View) upon disconnect of Watch Node
                    dynamoViewModel.BackgroundPreviewViewModel.ClearPathLabel(node.Path);
                }

                rootWatchViewModel.Children.Clear();
                rootWatchViewModel.IsCollection = false;
                return;
            }

            // If the node hasn't been evaluated, no need to update the UI
            if (!watch.HasRunOnce)
            {
                return;
            }

            var s = dynamoViewModel.Model.Scheduler;

            WatchViewModel wvm = null;

            // prevent data race by running on scheduler
            var t = new DelegateBasedAsyncTask(s, () =>
            {
                wvm = GetWatchViewModel();
            });

            // then update on the ui thread
            t.ThenPost((_) =>
            {
                //If wvm is not computed successfully then don't post.
                if (wvm == null) return;

                // store in temp variable to silence binding
                var temp = rootWatchViewModel.Children;


                rootWatchViewModel.Children = null;
                temp.Clear();
                temp.Add(wvm);

                // rebind
                rootWatchViewModel.Children = temp;
                rootWatchViewModel.CountNumberOfItems();
                rootWatchViewModel.CountLevels();
                rootWatchViewModel.Children[0].IsTopLevel = true;
                
            }, syncContext);

            s.ScheduleForExecution(t);
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
            astBeingComputed = watch.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview;
        }
    }
}