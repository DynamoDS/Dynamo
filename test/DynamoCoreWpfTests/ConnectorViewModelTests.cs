using System.Linq;
using Dynamo.Selection;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;
using Dynamo.Utilities;
using System.IO;
using System.Collections.Generic;
using Dynamo.Configuration;
using Dynamo.Models;
using System.Reflection;
using DynamoShapeManager;
using TestServices;
using Dynamo.ViewModels;
using Dynamo.Controls;
using Dynamo.Graph.Connectors;

namespace DynamoCoreWpfTests
{
    public class ConnectorViewModelTests : DynamoTestUIBase
    {

        #region Regular Connector Tests
        /// <summary>
        /// Check to see a pin can be added to a connector
        /// </summary>
        [Test]
        public void ConnectorVisibilityForLegacyGraphTest()
        {
            Open(@"UI/ConnectorPinTests.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            // Default collapse state should be false when opening legacy graph
            Assert.AreEqual(connectorViewModel.IsHidden, false);
        }

        /// <summary>
        /// Check to see a pin can be added to a connector
        /// </summary>
        [Test]
        public void CanPinConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            int initialConnectorCount = connectorViewModel.ConnectorPinViewCollection.Count;

            ///hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            ///Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            ///Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialConnectorCount + 1);

            ///hard-coded position (2) on wire
            connectorViewModel.PanelX = 419.33333;
            connectorViewModel.PanelY = 347.33333;

            connectorViewModel.PinConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialConnectorCount + 2);
        }

        /// <summary>
        /// Check to see a pin can be removed from a connector
        /// </summary>
        [Test]
        public void CanUnPinFromConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            ///hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            ///Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            ///Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            int connectorCountAfterAddingOne = connectorViewModel.ConnectorPinViewCollection.Count;

            ///Request remove pin
            var firstPin = connectorViewModel.ConnectorPinViewCollection.First();
            firstPin.OnRequestRemove(firstPin, System.EventArgs.Empty);

            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, connectorCountAfterAddingOne - 1);
        }

        /// <summary>
        /// Check to see if can select connected nodes.
        /// </summary>
        [Test]
        public void CanSelectConnectedNodes()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            int initialSelectedCount = DynamoSelection.Instance.Selection.Count;

            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            connectorViewModel.SelectConnectedCommand.Execute(null);
            ///Should result in 2 selected nodes.
            Assert.AreEqual(DynamoSelection.Instance.Selection.Count, initialSelectedCount + 2);
        }

        /// <summary>
        /// Check to see if can break connection.
        /// </summary>
        [Test]
        public void CanBreakConnection()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            int initialConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            connectorViewModel.BreakConnectionCommand.Execute(null);
            ///This should result in no connectors existing anymore.
            Assert.AreEqual(this.ViewModel.CurrentSpaceViewModel.Connectors.Count, initialConnectorCount - 1);
        }

        /// <summary>
        /// Check to see if can hide connector. 'HideConnection' is really 
        /// a command that toggles between 'IsVisible' and '!IsVisible'. So this test
        /// verifies that this works as expected.
        /// </summary>
        [Test]
        public void CanHideConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            bool initialVisibility = connectorViewModel.IsHidden;
            ///Toggles hide (visibility == off)
            connectorViewModel.HideConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.IsHidden, !initialVisibility);
        }

        /// <summary>
        /// Check to see if can unhide connector. 'HideConnection' toggles the visibility
        /// of the wire, so calling it twice should unhide the connector.
        /// </summary>
        [Test]
        public void CanUnhideConnector()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();
            bool initialVisibility = connectorViewModel.IsHidden;
            ///Toggles hide (visibility == off)
            connectorViewModel.HideConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.IsHidden, !initialVisibility);
            ///Toggles hide on/off (visibility == on)
            connectorViewModel.HideConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.IsHidden, initialVisibility);
        }

        /// <summary>
        /// Can place WatchNode in the center of a connector
        /// </summary>
        [Test]
        public void CanPlaceWatchNode()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            int initialConnectorCount = this.ViewModel.CurrentSpaceViewModel.Connectors.Count;
            int initialNodeCount = this.ViewModel.CurrentSpaceViewModel.Nodes.Count;
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            ///Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            ///Place watch node
            connectorViewModel.ConnectorAnchorViewModel.PlaceWatchNodeCommand.Execute(null);
            ///Two connectors should result when you place a new (watch) node in between.
            Assert.AreEqual(this.ViewModel.CurrentSpaceViewModel.Connectors.Count, initialConnectorCount + 1);
            ///Three nodes should be the total number of nodes after this operation.
            Assert.AreEqual(this.ViewModel.CurrentSpaceViewModel.Nodes.Count, initialNodeCount + 1);
        }

        #endregion

        #region Undo/Redo Tests
        /// <summary>
        /// Can undo 'placepin' command.
        /// </summary>
        [Test]
        public void CanUndoPin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            ///hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            int initialPinCount = connectorViewModel.ConnectorPinViewCollection.Count;

            ///Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            ///Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            ///Should be 1 more than the original count
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinCount + 1);
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            ///Should be the original count
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinCount);
        }
        /// <summary>
        /// Can undo 'Unpin' command
        /// </summary>
        [Test]
        public void CanUndoUnpin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            ///hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            ///Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            ///Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            int initialPinsAfterOneAdded = connectorViewModel.ConnectorPinViewCollection.Count;

            ///Request remove pin
            var firstPin = connectorViewModel.ConnectorPinViewCollection.First();
            firstPin.OnRequestRemove(firstPin, System.EventArgs.Empty);
            ///Pin count should be zero
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded - 1);
            ///Undo 'unpin' pin
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            ///Pin count should be one
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded);
        }
        /// <summary>
        /// Can undo 'delete pin' command.
        /// </summary>
        [Test]
        public void CanUndoDeletePin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            ///hard-coded position (1) on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            ///Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            ///Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            int initialPinsAfterOneAdded = connectorViewModel.ConnectorPinViewCollection.Count;

            ///Request delete pin
            var firstPin = connectorViewModel.ConnectorPinViewCollection.First();
            Model.ExecuteCommand(new DeleteModelCommand(firstPin.Model.GUID));
            ///Pin count should be zero
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded - 1);
            ///Undo delete pin
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            ///Pin count should be one
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, initialPinsAfterOneAdded);
        }
        /// <summary>
        /// Can undo 'drag pin'.
        /// </summary>
        [Test]
        public void CanUndoDragPin()
        {
            Open(@"UI/ConnectorPinTests.dyn");
            var connectorViewModel = this.ViewModel.CurrentSpaceViewModel.Connectors.First();

            ///hard-coded position on wire
            connectorViewModel.PanelX = 292.66666;
            connectorViewModel.PanelY = 278;

            ///deltas to move by
            double xMove = 250;
            double yMove = 150;

            ///Action triggered when hovering over a connector
            connectorViewModel.FlipOnConnectorAnchor();
            ///Pin command
            connectorViewModel.PinConnectorCommand.Execute(null);
            Assert.AreEqual(connectorViewModel.ConnectorPinViewCollection.Count, 1);

            /// add connectorpin to selection
            var connectorPin = connectorViewModel.ConnectorModel.ConnectorPinModels.First();
            DynamoSelection.Instance.Selection.Add(connectorPin);

            ///initial x, y position of pin
            double initialX = connectorPin.CenterX;
            double initialY = connectorPin.CenterY;

            var point = new Point2D(initialX, initialY);
            var operation = DragSelectionCommand.Operation.BeginDrag;
            ///'begin drag' command defined
            var command = new DragSelectionCommand(point, operation);
            ///execute 'begin drag'
            Model.ExecuteCommand(command);

            operation = DragSelectionCommand.Operation.EndDrag;
            point.X += xMove;
            point.Y += yMove;
            ///'end drag' command defined
            command = new DragSelectionCommand(point, operation);
            ///execute 'end drag'
            Model.ExecuteCommand(command);

            ///assert initial position and end position is NOT the same
            Assert.AreNotEqual(initialX, connectorPin.CenterX);
            Assert.AreNotEqual(initialY, connectorPin.CenterY);

            ///undo drag
            Model.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));

            ///assert that after undo initial position and end position IS the same
            Assert.AreEqual(initialX, connectorPin.CenterX);
            Assert.AreEqual(initialY, connectorPin.CenterY);
        }
        #endregion
    }
}