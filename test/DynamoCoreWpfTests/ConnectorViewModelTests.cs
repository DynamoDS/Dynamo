using System.Linq;
using Dynamo.Selection;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Xml;
using System.IO;

namespace DynamoCoreWpfTests
{
    public class ConnectorViewModelTests : DynamoTestUIBase
    {

        #region Regular Connector Tests
        /// <summary>
        /// Check to see if a connector is visible after pre 2.13 graph open
        /// </summary>
        [TestOnSeparateThread]
        public void ConnectorVisibilityForLegacyGraphTest()
        {
            Open(@"UI/ConnectorPinTests.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //Default IsHidden state should be false when opening legacy graph
            Assert.AreEqual(connectorViewModel.IsHidden, false);
        }

        /// <summary>
        /// Check to see a connector is visible after xml graph open
        /// </summary>
        [TestOnSeparateThread]
        public void ConnectorVisibilityForLegacyXMLGraphTest()
        {
            var filePath = @"UI/UI_visual_test.dyn";
            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), filePath);
            // Assert xml graph open
            Assert.IsTrue(DynamoUtilities.PathHelper.isValidXML(openPath, out _, out _));

            Open(filePath);

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //Default IsHidden state should be false when opening legacy graph
            Assert.AreEqual(connectorViewModel.IsHidden, false);
        }

        /// <summary>
        /// Check to see a pin can be added to a connector
        /// </summary>
        [TestOnSeparateThread]
        public void CanPinConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            int initialConnectorCount = connectorViewModel.ConnectorPinViewCollection.Count;

            //hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialConnectorCount + 1);

            //hard-coded position (2) on wire
            connectorViewModel.PanelX = 419.33333;
            connectorViewModel.PanelY = 347.33333;

            connectorViewModel.PinConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialConnectorCount + 2);
        }

        /// <summary>
        /// Check to see a pin can be removed from a connector
        /// </summary>
        [TestOnSeparateThread]
        public void CanUnPinFromConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            int connectorCountAfterAddingOne = connectorViewModel.ConnectorPinViewCollection.Count;

            //Request remove pin
            var firstPin = connectorViewModel.ConnectorPinViewCollection.First();
            firstPin.OnRequestRemove(firstPin, System.EventArgs.Empty);

            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, connectorCountAfterAddingOne - 1);
        }

        /// <summary>
        /// Check to see if can select connected nodes.
        /// </summary>
        [TestOnSeparateThread]
        public void CanSelectConnectedNodes()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            int initialSelectedCount = DynamoSelection.Instance.Selection.Count;

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            connectorViewModel.SelectConnectedCommand.Execute(null);
            //Should result in 2 selected nodes.
            Assert.AreEqual(DynamoSelection.Instance.Selection.Count, initialSelectedCount + 2);
        }

        /// <summary>
        /// Check to see if can break connection.
        /// </summary>
        [TestOnSeparateThread]
        public void CanBreakConnection()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            int initialConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            connectorViewModel.BreakConnectionCommand.Execute(null);
            //This should result in no connectors existing anymore.
            Assert.AreEqual(this.ViewModel.CurrentSpaceViewModel.Connectors.Count, initialConnectorCount - 1);
        }

        /// <summary>
        /// Check to see if can hide connector. 'HideConnection' is really 
        /// a command that toggles between 'IsVisible' and '!IsVisible'. So this test
        /// verifies that this works as expected.
        /// </summary>
        [TestOnSeparateThread]
        public void CanHideConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            bool initialVisibility = connectorViewModel.IsHidden;
            //Toggles hide (visibility == off)
            connectorViewModel.ShowhideConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.IsHidden, !initialVisibility);
        }

        /// <summary>
        /// Check to see if can unhide connector. 'HideConnection' toggles the visibility
        /// of the wire, so calling it twice should unhide the connector.
        /// </summary>
        [TestOnSeparateThread]
        public void CanUnhideConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            bool initialVisibility = connectorViewModel.IsHidden;
            //Toggles hide (visibility == off)
            connectorViewModel.ShowhideConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.IsHidden, !initialVisibility);
            //Toggles hide on/off (visibility == on)
            connectorViewModel.ShowhideConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.IsHidden, initialVisibility);
        }

        /// <summary>
        /// Can place WatchNode where specified along a connector
        /// </summary>
        [TestOnSeparateThread]
        public void CanPlaceWatchNode()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            int initialConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;
            int initialNodeCount = this.ViewModel.CurrentSpaceViewModel.Nodes.Count;
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Place watch node
            connectorViewModel.ConnectorAnchorViewModel.PlaceWatchNodeCommand.Execute(null);
            //Two connectors should result when you place a new (watch) node in between.
            Assert.AreEqual(this.ViewModel.CurrentSpaceViewModel.Connectors.Count, initialConnectorCount + 1);
            //Three nodes should be the total number of nodes after this operation.
            Assert.AreEqual(this.ViewModel.CurrentSpaceViewModel.Nodes.Count, initialNodeCount + 1);
        }

        /// <summary>
        /// Assert that a watch node is placed on the correct connector,
        /// and that it rewires new wires to the correct startind/ending ports.
        /// </summary>
        [TestOnSeparateThread]
        public void PlaceWatchNodeAndRewireCorrectly()
        {
            Open(@"UI/WatchNodePlacement.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.FirstOrDefault(c=>c.ConnectorModel.GUID == Guid.Parse("f49e39e1-4eb0-47da-9dbe-bbf0badddc71"));

            //initial connector port ids
            int connectorStartPortIndex = connectorViewModel.ConnectorModel.Start.Index;
            int connectorEndPortIndex = connectorViewModel.ConnectorModel.End.Index;

            //start and end nodes sandwiching watch node
            NodeViewModel startNode = this.ViewModel.CurrentSpaceViewModel.Nodes.FirstOrDefault(n => n.NodeModel.GUID == connectorViewModel.ConnectorModel.Start.Owner.GUID);
            NodeViewModel endNode = this.ViewModel.CurrentSpaceViewModel.Nodes.FirstOrDefault(n => n.NodeModel.GUID == connectorViewModel.ConnectorModel.End.Owner.GUID);

            //Record the target port GUIDs.
            Guid startPortGuid = startNode.OutPorts[connectorStartPortIndex].PortModel.GUID;
            Guid endPortGuid = endNode.InPorts[connectorEndPortIndex].PortModel.GUID;

            //mimic correct mouse placement over this connector
            connectorViewModel.PanelX = 435;
            connectorViewModel.PanelY = 394;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Place watch node
            connectorViewModel.ConnectorAnchorViewModel.PlaceWatchNodeCommand.Execute(null);

            //get new connectors if they are connected to correct nodes/ports
            var firstConnector = this.ViewModel.CurrentSpaceViewModel.Connectors
                .FirstOrDefault(c => c.ConnectorModel.Start.Owner.GUID == startNode.NodeModel.GUID
                && c.ConnectorModel.Start.Index == connectorStartPortIndex
                && c.ConnectorModel.GUID != connectorViewModel.ConnectorModel.GUID);
            
            var secondConnector = this.ViewModel.CurrentSpaceViewModel.Connectors
                .FirstOrDefault(c => c.ConnectorModel.End.Owner.GUID == endNode.NodeModel.GUID
                && c.ConnectorModel.End.Index == connectorEndPortIndex
                && c.ConnectorModel.GUID != connectorViewModel.ConnectorModel.GUID);

            //assert new connectors created wiring to correct nodes and ports.
            Assert.IsNotNull(firstConnector);
            Assert.IsNotNull(secondConnector);

           //assert new connectors are targeting the correct ports
            Assert.AreEqual(firstConnector.ConnectorModel.Start.GUID, startPortGuid);
            Assert.AreEqual(secondConnector.ConnectorModel.End.GUID, endPortGuid);
        }

        /// <summary>
        ///  Assert that a newly placed pin is placed on the correct (new) wire (before watch node) when rewired. 
        /// This occurs when a watch node is placed. New connectors are created and the old pin 
        /// locations are used to place new pins where the old ones were.
        /// </summary>
        [TestOnSeparateThread]
        public void PinPlacedOnCorrectWireToWatchNode()
        {
            Open(@"UI/WatchNodePlacement.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.FirstOrDefault(c => c.ConnectorModel.GUID == Guid.Parse("f747aafa-c276-4b48-8054-a66ac16e08ba"));

            //initial connector port ids
            int connectorStartPortIndex = connectorViewModel.ConnectorModel.Start.Index;
            int connectorEndPortIndex = connectorViewModel.ConnectorModel.End.Index;
            int initialPinCount = this.ViewModel.CurrentSpaceViewModel.Pins.Count;

            //start and end nodes sandwiching watch node
            NodeViewModel startNode = this.ViewModel.CurrentSpaceViewModel.Nodes.FirstOrDefault(n => n.NodeModel.GUID == connectorViewModel.ConnectorModel.Start.Owner.GUID);
            NodeViewModel endNode = this.ViewModel.CurrentSpaceViewModel.Nodes.FirstOrDefault(n => n.NodeModel.GUID == connectorViewModel.ConnectorModel.End.Owner.GUID);

            //Record the target port GUIDs.
            Guid startPortGuid = startNode.OutPorts[connectorStartPortIndex].PortModel.GUID;

            //mimic correct mouse placement over this connector
            connectorViewModel.PanelX = 600.6666;
            connectorViewModel.PanelY = 265.3333;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Place watch node
            connectorViewModel.ConnectorAnchorViewModel.PlaceWatchNodeCommand.Execute(null);

            int newPinCount = this.ViewModel.CurrentSpaceViewModel.Pins.Count;
            //Assert that the amount of pins is the same as it was initially
            //ie pin was replaced after re-wire action
            Assert.AreEqual(initialPinCount, newPinCount);

            //should only be one pin in the graph
            var connectorPin = this.ViewModel.CurrentSpaceViewModel.Pins.FirstOrDefault();

            //get the connector where the new pin was placed
            var newConnectorWherePinWasPlaced = this.ViewModel.CurrentSpaceViewModel.Connectors
                .FirstOrDefault(c => c.ConnectorModel.GUID == connectorPin.ConnectorGuid);

            //assert new connector when pin was placed has the correct startPortGuid
            Assert.AreEqual(newConnectorWherePinWasPlaced.ConnectorModel.Start.GUID, startPortGuid);
        }

        #endregion

        #region Undo/Redo Tests
        /// <summary>
        /// Can undo 'placepin' command.
        /// </summary>
        [TestOnSeparateThread]
        public void CanUndoPin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            int initialPinCount = connectorViewModel.ConnectorPinViewCollection.Count;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            //Should be 1 more than the original count
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinCount + 1);
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            //Should be the original count
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinCount);
        }
        /// <summary>
        /// Can undo 'Unpin' command
        /// </summary>
        [TestOnSeparateThread]
        public void CanUndoUnpin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            int initialPinsAfterOneAdded = connectorViewModel.ConnectorPinViewCollection.Count;

            //Request remove pin
            var firstPin = connectorViewModel.ConnectorPinViewCollection.First();
            firstPin.OnRequestRemove(firstPin, System.EventArgs.Empty);
            //Pin count should be zero
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded - 1);
            //Undo 'unpin' pin
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            //Pin count should be one
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded);
        }
        /// <summary>
        /// Can undo 'delete pin' command.
        /// </summary>
        [TestOnSeparateThread]
        public void CanUndoDeletePin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            int initialPinsAfterOneAdded = connectorViewModel.ConnectorPinViewCollection.Count;

            //Request delete pin
            var firstPin = connectorViewModel.ConnectorPinViewCollection.First();
            Model.ExecuteCommand(new DeleteModelCommand(firstPin.Model.GUID));
            //Pin count should be zero
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded - 1);
            //Undo delete pin
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            //Pin count should be one
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded);
        }
        /// <summary>
        /// Can undo 'drag pin'.
        /// </summary>
        [TestOnSeparateThread]
        public void CanUndoDragPin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            //hard-coded position on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            //deltas to move by
            double xMove = 250;
            double yMove = 150;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, 1);

            // add connectorpin to selection
            var connectorPin = connectorViewModel.ConnectorModel.ConnectorPinModels.First();
            DynamoSelection.Instance.Selection.Add(connectorPin);

            //initial x, y position of pin
            double initialX = connectorPin.CenterX;
            double initialY = connectorPin.CenterY;

            var point = new Point2D(initialX, initialY);
            var operation = DragSelectionCommand.Operation.BeginDrag;
            //'begin drag' command defined
            var command = new DragSelectionCommand(point, operation);
            //execute 'begin drag'
            Model.ExecuteCommand(command);

            operation = DragSelectionCommand.Operation.EndDrag;
            point.X += xMove;
            point.Y += yMove;
            //'end drag' command defined
            command = new DragSelectionCommand(point, operation);
            //execute 'end drag'
            Model.ExecuteCommand(command);

            //assert initial position and end position is NOT the same
            Assert.AreNotEqual(initialX, connectorPin.CenterX);
            Assert.AreNotEqual(initialY, connectorPin.CenterY);

            //undo drag
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));

            //assert that after undo initial position and end position IS the same
            Assert.AreEqual(initialX, connectorPin.CenterX);
            Assert.AreEqual(initialY, connectorPin.CenterY);
        }

        [TestOnSeparateThread]
        public void CanUndoPlaceWatchNode()
        {
            Open(@"UI/WatchNodePlacement.dyn");
            int initialConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.FirstOrDefault(c => c.ConnectorModel.GUID == Guid.Parse("f49e39e1-4eb0-47da-9dbe-bbf0badddc71"));
            //mimic correct mouse placement over this connector
            connectorViewModel.PanelX = 435;
            connectorViewModel.PanelY = 394;

            //Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            //Place watch node
            connectorViewModel.ConnectorAnchorViewModel.PlaceWatchNodeCommand.Execute(null);
            //asserts an additional connector exists
            int partialConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;
            Assert.AreEqual(initialConnectorCount+1, partialConnectorCount);
            //Undo watch node placement
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            //assert after one undo action we are back at the original connector count
            int finalConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;
            Assert.AreEqual(initialConnectorCount, finalConnectorCount);
        }
        #endregion
    }
}
