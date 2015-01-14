using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.ViewModels;

using VMDataBridge;

namespace Dynamo.Wpf
{
    public class WatchNodeViewCustomization : INodeViewCustomization<Dynamo.Nodes.Watch>
    {
        private DynamoViewModel dynamoViewModel;
        private Nodes.Watch watchNodeModel;
        private WatchViewModel root;
        private WatchTree watchTree;

        public void CustomizeView(Nodes.Watch nodeModel, NodeView nodeView)
        {
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            this.watchNodeModel = nodeModel;

            watchTree = new WatchTree();
            root = new WatchViewModel(this.dynamoViewModel.VisualizationManager);

            // Fix the maximum width/height of watch node.
            nodeView.PresentationGrid.MaxWidth = Configurations.MaxWatchNodeWidth;
            nodeView.PresentationGrid.MaxHeight = Configurations.MaxWatchNodeHeight;
            nodeView.PresentationGrid.Children.Add(watchTree);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;

            nodeModel.InPorts[0].PortConnected += InputPortConnected;
            
            watchTree.DataContext = root;

            var checkedBinding = new Binding("ShowRawData")
            {
                Mode = BindingMode.TwoWay,
                Source = root
            };

            var rawDataMenuItem = new MenuItem
            {
                Header = "Show Raw Data",
                IsCheckable = true,
            };
            rawDataMenuItem.SetBinding(MenuItem.IsCheckedProperty, checkedBinding);

            nodeView.MainContextMenu.Items.Add(rawDataMenuItem);

            this.dynamoViewModel.Model.PreferenceSettings.PropertyChanged += PreferenceSettings_PropertyChanged;
            root.PropertyChanged += Root_PropertyChanged;

            DataBridge.Instance.RegisterCallback(watchNodeModel.GUID.ToString(), EvaluationCompleted);
        }

        /// <summary>
        ///     Callback for port connection. Handles clearing the watch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputPortConnected(object sender, EventArgs e)
        {
            Tuple<int, NodeModel> input;
            if (watchNodeModel.TryGetInput(watchNodeModel.InPorts.IndexOf(sender as PortModel), out input))
            {
                var oldId = astBeingWatched;
                astBeingWatched = input.Item2.GetAstIdentifierForOutputIndex(input.Item1);
                if (oldId != null && astBeingWatched.Value != oldId.Value)
                {
                    CachedValue = null;
                    if (Root != null)
                        Root.Children.Clear();
                }
            }
        }

        /// <summary>
        /// This method returns a WatchNode for it's preview AST node.
        /// This method gets called on ui thread when "IsUpdated" property
        /// change is notified. This method is responsible for populating the 
        /// watch node with evaluated value of the input. Gets the MirrorData
        /// for the input/preview AST and then processes the mirror data to
        /// render the watch content properly.
        /// </summary>
        /// <returns>WatchNode</returns>
        internal WatchViewModel GetWatchNode()
        {
            var inputVar = watchNodeModel.IsPartiallyApplied
                ? watchNodeModel.AstIdentifierForPreview.Name
                : watchNodeModel.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name;

            var core = this.dynamoViewModel.Model.EngineController.LiveRunnerCore;

            if (root != null)
            {
                return this.dynamoViewModel.WatchHandler.GenerateWatchViewModelForData(
                    watchNodeModel.CachedValue,
                    core,
                    inputVar,
                    root.ShowRawData);
            }
            else
                return this.dynamoViewModel.GenerateWatchViewModelForData(CachedValue, core, inputVar);
        }

        void Root_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowRawData")
            {
                ResetWatch();
            }
        }

        void PreferenceSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if the units settings have been modified in the UI, watch has
            //to immediately update to show unit objects in the correct format
            if (e.PropertyName == "LengthUnit" ||
                e.PropertyName == "AreaUnit" ||
                e.PropertyName == "VolumeUnit" ||
                e.PropertyName == "NumberFormat")
            {
                ResetWatch();
            }
        }

        protected void OnBuilt()
        {
            DataBridge.Instance.RegisterCallback(watchNodeModel.GUID.ToString(), EvaluationCompleted);
        }

        private void EvaluationCompleted(object o)
        {
            watchNodeModel.CachedValue = o;
            ResetWatch();
        }

        private void ResetWatch()
        {
            watchNodeModel.DispatchOnUIThread(
                delegate
                {
                    var temp = root;
                    root = null;

                    temp.Children.Clear();
                    temp.Children.Add(this.GetWatchNode());

                    root = temp;
                });
        }

        public void Dispose()
        {
            nodeModel.InPorts[0].PortConnected += InputPortConnected;
            dynamoViewModel.Model.PreferenceSettings.PropertyChanged -= PreferenceSettings_PropertyChanged;
            root.PropertyChanged -= Root_PropertyChanged;
            DataBridge.Instance.UnregisterCallback(watchNodeModel.GUID.ToString());
        }

    }
}