using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using ViewModels.Core;

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
            var dynamoViewModel = (View.DataContext as DynamoViewModel);
            Assert.IsNotNull(dynamoViewModel);

            //The GeometryScalingCodeBlock.dyn contains a CodeBlock with a large number that needs ScaleFactor > Medium
            Open(@"core\GeometryScalingInfoSlider.dyn");

            //Change the RunType to Automatic, so when the MarkNodesAsModifiedAndRequestRun() is called a graph execution will be kicked off 
            View.RunSettingsControl.RunTypesComboBox.SelectedItem = View.RunSettingsControl.RunTypesComboBox.Items[1];
            DispatcherUtil.DoEvents();

            //This will get the Circle.ByCenterPointRadius node (connected to the CodeBlock)
            var nodeView = NodeViewWithGuid("bcf1c893-5311-4dff-9a04-203ffb9d426c");

            //Checking that the node is not in a warning state before changing the scale factor
            Assert.AreEqual(nodeView.ViewModel.State, ElementState.Active);

            //Creates the Geometry Scaling Popup and set the ScaleFactor = 2 ( Medium)
            var geoScalingPopup = new GeometryScalingPopup(dynamoViewModel);
            geoScalingPopup.IsOpen = true;
            DispatcherUtil.DoEvents();

            //Clicks the Small button so the ScaleFactor is updated to -2 (Small) - Run is automatically executed after changing the Scale Factor value
            geoScalingPopup.Small.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //When RunType = Automatic when the graph is executed the ByCenterPointRadius node change status to Info due that ScaleFactor = Small
            Assert.AreEqual(nodeView.ViewModel.State, ElementState.Info);

            // Get the int slider and update to a value within small scaling recommendation
            var slider = NodeViewWithGuid("7db0c3d3-00b6-46e0-ab41-a5759bb66107");
            var param = new UpdateValueParams("Value", "50");
            slider.ViewModel.NodeModel.UpdateValue(param);
            // The node info should clear after the change
            Assert.AreEqual(nodeView.ViewModel.State, ElementState.Active);
        }

        [Test]
        public void GeoScalingPopup_ShowsCorrectSetting() {
            var dynamoViewModel = (View.DataContext as DynamoViewModel);
            Assert.IsNotNull(dynamoViewModel);

            //The PolycurveByPoints.dyn has ScaleFactor set to Extra Large
            Open(@"core\PolycurveByPoints.dyn");

            //Creates the Geometry Scaling Popup and set the ScaleFactor = 2 ( Medium)
            var geoScalingPopup = new GeometryScalingPopup(dynamoViewModel);
            geoScalingPopup.IsOpen = true;
            DispatcherUtil.DoEvents();

            //Check that the Medium radio button is checked
            var geoScalingVM = geoScalingPopup.ExtraLarge.DataContext as GeometryScalingViewModel;
            Assert.NotNull(geoScalingVM);

            Assert.True(geoScalingVM.ScaleValue == 4);
        }

        /// <summary>
        /// The test validates that when RunType=Manual and the MarkNodesAsModifiedAndRequestRun() method is called the graph execution is NOT kicked off
        /// </summary>
        [Test]
        public void PreferencesGeoScaling_RunGraph_Manual_Mode()
        {
            //The GeometryScalingCodeBlock.dyn contains a CodeBlock with a large number that needs ScaleFactor > Medium
            Open(@"core\GeometryScalingInfoSlider.dyn");

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
