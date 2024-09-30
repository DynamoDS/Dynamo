using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    public class AnnotationViewTests : DynamoTestUIBase
    {      
        public AnnotationView NodeViewWithGuid(string guid)
        {
            //dynamoView.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First().ChildNodeViews();
            var annotationView =
                View.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First().ChildrenOfType<AnnotationView>();
            var annotationViewOfType = annotationView.Where(x => x.ViewModel.AnnotationModel.GUID.ToString() == guid);
            Assert.AreEqual(1, annotationViewOfType.Count(), "Expected a single Annotation View with guid: " + guid);

            return annotationViewOfType.First();
        }

        public NodeView NodeModelViewWithGuid(string guid)
        {
            var nodeViews = View.NodeViewsInFirstWorkspace();
            var nodeViewsOfType = nodeViews.Where(x => x.ViewModel.NodeLogic.GUID.ToString() == guid);
            Assert.AreEqual(1, nodeViewsOfType.Count(), "Expected a single NodeView with guid: " + guid);

            return nodeViewsOfType.First();
        }

        public override void Open(string path)
        {
            base.Open(path);
            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public  void TestAnnotationMouseClickEvent()
        {
            Open(@"UI\GroupTest.dyn");

            var annotationView = NodeViewWithGuid("a432d63f-7a36-45ad-b30a-7924beb20e90");
            
            //Raise a click event to check whether the group and the models are selected
            annotationView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UserControl.MouseLeftButtonDownEvent
            });

            //The group and the node should be selected
            Assert.AreEqual(2, DynamoSelection.Instance.Selection.Count());
        }

        [Test]
        public void TestAnnotationOnDummyNodes()
        {
            Open(@"UI\GroupDummyNodes.dyn");
           
            //check whether the Dummy nodes have the same GUID.
            var nodeView = NodeModelViewWithGuid("de745c17-d6ef-4ff8-ac09-0d1703f58de5");
            Assert.IsNotNull(nodeView);

            //check whether there is a GROUP around Dummy nodes.
            var annotationView = NodeViewWithGuid("ce97bb23-ad59-461b-ae81-17e8c48cf8de");
            Assert.IsNotNull(annotationView);

            var modelCount = annotationView.ViewModel.AnnotationModel.Nodes.Count();
            Assert.AreEqual(1,modelCount);
        }

        [Test]
        public void UngroupingCollapsedGroupWillUnCollapseAllGroupContent()
        {
            // Arrange
            Open(@"core\annotationViewModelTests\groupsTestFile.dyn");

            var annotationView = NodeViewWithGuid("a87c3469-dc5d-4475-849e-85ccd5fbae78");
            var groupContent = annotationView.ViewModel.ViewModelBases;

            Assert.IsFalse(annotationView.ViewModel.IsExpanded);
            Assert.That(groupContent.All(x => x.IsCollapsed == true));

            // Act
            annotationView.UngroupAnnotation.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            // Assert
            Assert.That(groupContent.All(x => x.IsCollapsed == false));
        }

        [Test]
        public void AddConnectorPinsToGroups()
        {
            // Arrange
            Open(@"core\annotationViewModelTests\groupsTestFile.dyn");

            var groupGuid = new Guid("8324afb7-2d77-4a75-aa5e-f10e59964c2b");
            var connectorGuid = new Guid("17318da5-dd19-4962-a7b7-51344001f14b");

            // Act
            var ws = this.Model.CurrentWorkspace;
            var group1 = ws.Annotations.FirstOrDefault(annotation => annotation.GUID == groupGuid);
            var connector = ws.Connectors.FirstOrDefault(connector => connector.GUID == connectorGuid);
            var connectorPin = connector.ConnectorPinModels.FirstOrDefault();

            // Assert
            Assert.IsNotNull(group1, $"Expected to find annotation group with GUID {groupGuid}, but it was not found.");
            Assert.IsNotNull(connector, $"Expected to find connector with GUID {connectorGuid}, but it was not found.");
            Assert.IsNotNull(connectorPin, "Expected to find a ConnectorPinModel associated with the connector, but it was not found.");

            var initialNodeCount = group1.Nodes.Count();
            Assert.AreEqual(2, initialNodeCount, $"Expected the group to contain 2 nodes initially, but found {initialNodeCount}.");

            // Act: Add the connectorPin to the group
            group1.AddToTargetAnnotationModel(connectorPin);

            // Assert
            var finalNodeCount = group1.Nodes.Count();
            Assert.AreEqual(3, finalNodeCount, $"Expected the group to contain 3 nodes after adding the connector pin, but found {finalNodeCount}.");
        }
    }
}
