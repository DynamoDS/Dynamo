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
        private WatchTree watchTree;

        public void CustomizeView(Nodes.Watch nodeModel, NodeView nodeView)
        {
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            // TODO(Peter): Watch should not know about the ViewModel layer MAGN-5590
            nodeModel.DynamoViewModel = this.dynamoViewModel;
            this.watchNodeModel = nodeModel;

            watchTree = new WatchTree();

            // MAGN-2446: Fixes the maximum width/height of watch node so it won't 
            // go too crazy on us. Note that this is only applied to regular watch 
            // node so it won't be limiting the size of image/3D watch nodes.
            // 
            nodeView.PresentationGrid.MaxWidth = Configurations.MaxWatchNodeWidth;
            nodeView.PresentationGrid.MaxHeight = Configurations.MaxWatchNodeHeight;
            nodeView.PresentationGrid.Children.Add(watchTree);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;

            if (watchNodeModel.Root == null)
                watchNodeModel.Root = new WatchViewModel(this.dynamoViewModel.VisualizationManager);

            watchTree.DataContext = watchNodeModel.Root;

            watchNodeModel.RequestBindingUnhook += delegate
            {
                BindingOperations.ClearAllBindings(watchTree.treeView1);
            };

            watchNodeModel.RequestBindingRehook += delegate
            {
                var sourceBinding = new Binding("Children")
                {
                    Mode = BindingMode.TwoWay,
                    Source = watchNodeModel.Root,
                };
                watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty, sourceBinding);
            };

            var checkedBinding = new Binding("ShowRawData")
            {
                Mode = BindingMode.TwoWay,
                Source = watchNodeModel.Root
            };

            var rawDataMenuItem = new MenuItem
            {
                Header = "Show Raw Data",
                IsCheckable = true,
            };
            rawDataMenuItem.SetBinding(MenuItem.IsCheckedProperty, checkedBinding);

            nodeView.MainContextMenu.Items.Add(rawDataMenuItem);

            watchNodeModel.Workspace.DynamoModel.PreferenceSettings.PropertyChanged += PreferenceSettings_PropertyChanged;
            watchNodeModel.Root.PropertyChanged += Root_PropertyChanged;

            DataBridge.Instance.RegisterCallback(watchNodeModel.GUID.ToString(), EvaluationCompleted);
        }

        private void EvaluationCompleted(object o)
        {
            watchNodeModel.CachedValue = o;
            watchNodeModel.DispatchOnUIThread(
                delegate
                {
                    //unhook the binding
                    watchNodeModel.OnRequestBindingUnhook(EventArgs.Empty);

                    watchNodeModel.Root.Children.Clear();
                    watchNodeModel.Root.Children.Add(watchNodeModel.GetWatchNode());

                    //rehook the binding
                    watchNodeModel.OnRequestBindingRehook(EventArgs.Empty);
                }
                );
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

        private void ResetWatch()
        {
            int count = 0;
            watchNodeModel.DispatchOnUIThread(
                delegate
                {
                    //unhook the binding
                    watchNodeModel.OnRequestBindingUnhook(EventArgs.Empty);

                    watchNodeModel.Root.Children.Clear();

                    watchNodeModel.Root.Children.Add(watchNodeModel.GetWatchNode());

                    count++;

                    //rehook the binding
                    watchNodeModel.OnRequestBindingRehook(EventArgs.Empty);
                });
        }

        public void Dispose()
        {
            watchNodeModel.Workspace.DynamoModel.PreferenceSettings.PropertyChanged -= PreferenceSettings_PropertyChanged;
            watchNodeModel.Root.PropertyChanged -= Root_PropertyChanged;
        }

    }
}