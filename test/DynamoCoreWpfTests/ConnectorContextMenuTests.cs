using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Dynamo.Selection;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;
using Dynamo.Utilities;
using Dynamo.Views;

namespace DynamoCoreWpfTests
{
    public class ConnectorContextMenuTests: DynamoTestUIBase
    {
        [TestOnSeparateThread]
        public void ConstructingContextMenuTest()
        {
            Open(@"UI/ConnectorContextMenuTestFile.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            //Creates contextMenu.
            connectorViewModel.InstantiateContextMenuCommand.Execute(null);

            var contextMenuViewModel = connectorViewModel.ConnectorContextMenuViewModel;
            //Assert it is getting created.
            Assert.IsNotNull(contextMenuViewModel);

            //Assert 'IsCollapsed' property matches that of connectorViewModel.
            Assert.AreEqual(connectorViewModel.IsHidden, contextMenuViewModel.IsCollapsed);

            //Assert position between the same is the same.
            Assert.AreEqual(connectorViewModel.MousePosition, contextMenuViewModel.CurrentPosition);
        }

        [TestOnSeparateThread]
        public void SetContextMenuToNullAfterUseTest()
        {
            Open(@"UI/ConnectorContextMenuTestFile.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            //Creates contextMenu.
            connectorViewModel.InstantiateContextMenuCommand.Execute(null);
            var contextMenuViewModel = connectorViewModel.ConnectorContextMenuViewModel;
            contextMenuViewModel.RequestDisposeViewModel();

            //Assert property on ConnectorViewModel is set to null.
            Assert.IsNull(connectorViewModel.ConnectorContextMenuViewModel);
        }
        [TestOnSeparateThread]
        public void BreakConnectionFromContextMenuTest()
        {
            Open(@"UI/ConnectorContextMenuTestFile.dyn");

            int connectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            //Creates contextMenu.
            connectorViewModel.InstantiateContextMenuCommand.Execute(null);
            var contextMenuViewModel = connectorViewModel.ConnectorContextMenuViewModel;
            contextMenuViewModel.BreakConnectionsSurrogateCommand.Execute(null);

            int updatedConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;

            //Assert that the only connector in the model has been destroyed.
            Assert.AreEqual(connectorCount-1, updatedConnectorCount);
        }
        [TestOnSeparateThread]
        public void SelectedConnectedFromContextMenuTest()
        {
            Open(@"UI/ConnectorContextMenuTestFile.dyn");

            int initialSelectedCount = DynamoSelection.Instance.Selection.Count;
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            //Creates contextMenu.
            connectorViewModel.InstantiateContextMenuCommand.Execute(null);
            var contextMenuViewModel = connectorViewModel.ConnectorContextMenuViewModel;
            contextMenuViewModel.SelectConnectedSurrogateCommand.Execute(null);

            //Assert that the selection of the two adjacent nodes is accurate.
            Assert.AreEqual(DynamoSelection.Instance.Selection.Count, initialSelectedCount + 2);
        }
        [TestOnSeparateThread]
        public void HideConnectorFromContextMenuTest()
        {
            Open(@"UI/ConnectorContextMenuTestFile.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            bool initialVisibility = connectorViewModel.IsHidden;
            //Creates contextMenu.
            connectorViewModel.InstantiateContextMenuCommand.Execute(null);
            var contextMenuViewModel = connectorViewModel.ConnectorContextMenuViewModel;
            ///Toggles hide (visibility == off)
            contextMenuViewModel.HideConnectorSurrogateCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.IsHidden, !initialVisibility);
        }


        [TestOnSeparateThread]
        public void AreNodeConnectionsInMenu()
        {
            // Mock a WorkspaceView
            var workspaceView = new WorkspaceView();

            // Search the associated context menu for the lacing sub-menu
            var contextMenu = workspaceView.FindName("NodeConnectionsMenu") as MenuItem;

            // Show All Wires, Hide All Wires
            Assert.AreEqual(contextMenu.Items.Count, 2);
        }

        [TestOnSeparateThread]
        public void ShowAllConnectorFromWorkspaceContextMenuTest()
        {
            Open(@"UI/ConnectorShowHideAllWires.dyn");

            var visibleConnectors = this.ViewModel.CurrentSpaceViewModel.Connectors.Where(x => !x.IsHidden);
            var hiddenConnectors = this.ViewModel.CurrentSpaceViewModel.Connectors.Where(x => x.IsHidden);

            // Default definition values
            Assert.AreEqual(3, visibleConnectors.Count());
            Assert.AreEqual(1, hiddenConnectors.Count());

            SelectAllNodes();

            this.ViewModel.CurrentSpaceViewModel.ShowAllWiresCommand.Execute(null);

            // Values after Show All Wires is run
            Assert.AreEqual(4, visibleConnectors.Count());
            Assert.AreEqual(0, hiddenConnectors.Count());

            this.ViewModel.CurrentSpaceViewModel.HideAllWiresCommand.Execute(null);

            // Values after Hide All Wires is run
            Assert.AreEqual(0, visibleConnectors.Count());
            Assert.AreEqual(4, hiddenConnectors.Count());
        }

        [TestOnSeparateThread]
        public void GoToStartNodeTest()
        {
            Open(@"UI/ConnectorContextMenuTestFile.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            Assert.AreEqual(connectorViewModel.ConnectorModel.Start.Owner.IsSelected, false);

            connectorViewModel.InstantiateContextMenuCommand.Execute(null);
            var contextMenuViewModel = connectorViewModel.ConnectorContextMenuViewModel;
            contextMenuViewModel.GoToStartNodeCommand.Execute(null);

            Assert.AreEqual(connectorViewModel.ConnectorModel.Start.Owner.IsSelected, true);
        }

        [TestOnSeparateThread]
        public void GoToEndNodeTest()
        {
            Open(@"UI/ConnectorContextMenuTestFile.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            Assert.AreEqual(connectorViewModel.ConnectorModel.End.Owner.IsSelected, false);

            connectorViewModel.InstantiateContextMenuCommand.Execute(null);
            var contextMenuViewModel = connectorViewModel.ConnectorContextMenuViewModel;
            contextMenuViewModel.GoToEndNodeCommand.Execute(null);

            Assert.AreEqual(connectorViewModel.ConnectorModel.End.Owner.IsSelected, true);
        }

        /// <summary>
        /// Helper method to select all (nodes) in the current Workspace
        /// </summary>
        /// <param name="nodes"></param>
        private void SelectAllNodes()
        {
            DynamoSelection.Instance.ClearSelection();
            foreach (var node in this.Model.CurrentWorkspace.Nodes)
            {
                DynamoSelection.Instance.Selection.Add(node);
            }
        }
    }
}
