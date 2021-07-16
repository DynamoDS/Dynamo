using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Graph.Nodes;
using Dynamo.Tests;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class PreferencesGeometryScalingTests : DynamoTestUIBase
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
            libraries.Add("DesignScriptBuiltin.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// The test validates that when RunType=Automatic and the MarkNodesAsModifiedAndRequestRun() method is called the graph execution is kicked off
        /// The test validates that when RunType=Manual and the MarkNodesAsModifiedAndRequestRun() method is called the graph execution is NOT kicked off
        /// </summary>
        [Test]
        [TestCase("Automatic", ElementState.Warning)]
        [TestCase("Manual", ElementState.Active)]
        public void PreferencesGeoScaling_RunGraph_Automatic_Mode(string runMode, ElementState expectedNodeState)
        {
            //The GeometryScalingCodeBlock.dyn contains a CodeBlock with a large number that needs ScaleFactor > Medium
            Open(@"core\GeometryScalingCodeBlock.dyn");

            if (runMode.Equals("Automatic"))
            {
                //Change the RunType to Automatic, so when the MarkNodesAsModifiedAndRequestRun() is called a graph execution will be kicked off 
                View.RunSettingsControl.RunTypesComboBox.SelectedItem = View.RunSettingsControl.RunTypesComboBox.Items[1];
                DispatcherUtil.DoEvents();
            }

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

            //When RunType = Automatic when the graph is executed the ByCenterPointRadius node change status to Warning due that ScaleFactor = Small
            //When RunType = Manual when the graph is NOT executed then the ByCenterPointRadius status will be Active
            Assert.AreEqual(nodeView.ViewModel.State, expectedNodeState);
        }
    }
}
