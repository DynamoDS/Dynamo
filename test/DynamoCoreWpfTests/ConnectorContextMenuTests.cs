using System.Linq;
using Dynamo.Selection;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;
using Dynamo.Utilities;

namespace DynamoCoreWpfTests
{
    public class ConnectorContextMenuTests: DynamoTestUIBase
    {
        [Test]
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

        [Test]
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
        [Test]
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
        [Test]
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
        [Test]
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

  
    }
}
