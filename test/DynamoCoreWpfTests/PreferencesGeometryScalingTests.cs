using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PreferencesGeometryScalingTests : DynamoTestUIBase
    {
        public override void Open(string path)
        {
            base.Open(path);
            DispatcherUtil.DoEvents();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// The test validates that when RunType=Automatic and the MarkNodesAsModifiedAndRequestRun() method is called the graph execution is kicked off
        /// </summary>
        [Test]
        public void PreferencesGeoScaling_RunGraph_Automatic()
        {
            //The GeometryScalingCodeBlock.dyn contains a CodeBlock with a large number that needs ScaleFactor > Medium
            Open(@"core\GeometryScalingCodeBlock.dyn");

            //Change the RunType to Automatic, so when the MarkNodesAsModifiedAndRequestRun() is called a graph execution will be kicked off 
            View.RunSettingsControl.RunTypesComboBox.SelectedItem = View.RunSettingsControl.RunTypesComboBox.Items[1];
            DispatcherUtil.DoEvents();

            //This will get the Circle.ByCenterPointRadius node (connected to the CodeBlock)
            var nodeView = NodeViewWithGuid("bcf1c893-5311-4dff-9a04-203ffb9d426c");

            //Checking that the node is not in a warning state before changing the scale factor
            Assert.AreEqual(nodeView.ViewModel.State, ElementState.Active);

            //Creates the Preferences dialog and the ScaleFactor = 2 ( Medium)
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();

            //Change the RadioButton checked so the ScaleFactor is updated to -2 (Small)
            preferencesWindow.RadioSmall.IsChecked = true;
            DispatcherUtil.DoEvents();

            //Close the Preferences Dialog and due that the ScaleFactor was updated the MarkNodesAsModifiedAndRequestRun() method will be called
            preferencesWindow.CloseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();        

            //When RunType = Automatic when the graph is executed the ByCenterPointRadius node change status to Warning due that ScaleFactor = Small
            Assert.AreEqual(nodeView.ViewModel.State, ElementState.Warning);
        }

        /// <summary>
        /// The test validates that when RunType=Manual and the MarkNodesAsModifiedAndRequestRun() method is called the graph execution is NOT kicked off
        /// </summary>
        [Test]
        public void PreferencesGeoScaling_RunGraph_Manual_Mode()
        {
            //The GeometryScalingCodeBlock.dyn contains a CodeBlock with a large number that needs ScaleFactor > Medium
            Open(@"core\GeometryScalingCodeBlock.dyn");

            //Creates the Preferences dialog and the ScaleFactor = 2 ( Medium)
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();

            //Change the RadioButton checked so the ScaleFactor is updated to -2 (Small)
            preferencesWindow.RadioSmall.IsChecked = true;
            DispatcherUtil.DoEvents();

            //Close the Preferences Dialog and due that the ScaleFactor was updated the MarkNodesAsModifiedAndRequestRun() method will be called
            preferencesWindow.CloseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //This will get the Circle.ByCenterPointRadius node (connected to the CodeBlock)
            var nodeView = NodeViewWithGuid("bcf1c893-5311-4dff-9a04-203ffb9d426c");

            //When RunType = Manual and the MarkNodesAsModifiedAndRequestRun() method is called, the graph won't be executed and the node state will remain in Active
            Assert.AreEqual(nodeView.ViewModel.State, ElementState.Active);
        }
    }
}
