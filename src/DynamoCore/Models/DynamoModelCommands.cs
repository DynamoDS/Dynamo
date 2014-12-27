using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    internal delegate void RecordableCommandHandler(DynamoModel.RecordableCommand command);

    partial class DynamoModel
    {
        internal event RecordableCommandHandler CommandStarting;
        internal event RecordableCommandHandler CommandCompleted;

        public void ExecuteCommand(RecordableCommand command)
        {
            if (CommandStarting != null)
                CommandStarting(command);

            command.Execute(this);

            if (CommandCompleted != null)
                CommandCompleted(command);
        }
        
        private PortModel activeStartPort;

        void OpenFileImpl(OpenFileCommand command)
        {
            string xmlFilePath = command.XmlFilePath;
            OpenFileFromPath(xmlFilePath);

            //clear the clipboard to avoid copying between dyns
            //ClipBoard.Clear();
        }

        void RunCancelImpl(RunCancelCommand command)
        {
            var model = CurrentWorkspace as HomeWorkspaceModel;
            if (model != null)
                model.Run();
        }

        void ForceRunCancelImpl(ForceRunCancelCommand command)
        {
            ForceRun();
        }

        void CreateNodeImpl(CreateNodeCommand command)
        {
            var node = command.Node;
            if (node == null)
            {
                if (command.NodeXml != null)
                {
                    // command was deserialized, we must create the node directly
                    node = NodeFactory.CreateNodeFromXml(command.NodeXml, SaveContext.File);
                }
                else
                {
                    // legacy command, hold on to your butts

                    var name = command.Name;
                    var nodeId = command.NodeId;

                    Guid customNodeId; // To be used in the event it's a custom node we're making.

                    // Then, we have to figure out what kind of node to make, based on the name.

                    // First, we check for a DSFunction by looking for a FunctionDescriptor
                    var functionItem = LibraryServices.GetFunctionDescriptor(name);
                    if (functionItem != null)
                    {
                        node = (functionItem.IsVarArg)
                            ? new DSVarArgFunction(functionItem) as NodeModel
                            : new DSFunction(functionItem);
                        node.GUID = nodeId;
                    }
                    // If that didn't work, let's try using the NodeFactory
                    else if (NodeFactory.CreateNodeFromTypeName(name, out node))
                    {
                        node.GUID = nodeId;
                    }
                    // And if that didn't work, then it must be a custom node.
                    else if (Guid.TryParse(name, out customNodeId))
                    {
                        node = CustomNodeManager.CreateCustomNodeInstance(customNodeId);
                        node.GUID = nodeId;
                    }
                    // We're out of ideas, log an error.
                    else
                    {
                        Logger.LogError("Could not create instance of node with name: " + name);
                        return;
                    }
                }
            }

            node.X = command.X;
            node.Y = command.Y;

            AddNodeToCurrentWorkspace(node, centered: command.DefaultPosition);
            CurrentWorkspace.RecordCreatedModel(node);
        }

        void CreateNoteImpl(CreateNoteCommand command)
        {
            NoteModel noteModel = CurrentWorkspace.AddNote(
                command.DefaultPosition,
                command.X,
                command.Y,
                command.NoteText,
                command.NodeId);

            CurrentWorkspace.RecordCreatedModel(noteModel);
        }

        void SelectModelImpl(SelectModelCommand command)
        {
            // Empty ModelGuid means clear selection.
            if (command.ModelGuid == Guid.Empty)
            {
                DynamoSelection.Instance.ClearSelection();
                return;
            }

            ModelBase model = CurrentWorkspace.GetModelInternal(command.ModelGuid);

            if (false == model.IsSelected)
            {
                if (!command.Modifiers.HasFlag(ModifierKeys.Shift))
                    DynamoSelection.Instance.ClearSelection();

                if (!DynamoSelection.Instance.Selection.Contains(model))
                    DynamoSelection.Instance.Selection.Add(model);
            }
            else
            {
                if (command.Modifiers.HasFlag(ModifierKeys.Shift))
                    DynamoSelection.Instance.Selection.Remove(model);
            }
        }

        void MakeConnectionImpl(MakeConnectionCommand command)
        {
            Guid nodeId = command.NodeId;

            switch (command.ConnectionMode)
            {
                case MakeConnectionCommand.Mode.Begin:
                    BeginConnection(nodeId, command.PortIndex, command.Type);
                    break;

                case MakeConnectionCommand.Mode.End:
                    EndConnection(nodeId, command.PortIndex, command.Type);
                    break;

                case MakeConnectionCommand.Mode.Cancel:
                    activeStartPort = null;
                    break;
            }
        }

        void BeginConnection(Guid nodeId, int portIndex, PortType portType)
        {
            bool isInPort = portType == PortType.Input;

            var node = CurrentWorkspace.GetModelInternal(nodeId) as NodeModel;
            if (node == null)
                return;
            PortModel portModel = isInPort ? node.InPorts[portIndex] : node.OutPorts[portIndex];

            // Test if port already has a connection, if so grab it and begin connecting 
            // to somewhere else (we don't allow the grabbing of the start connector).
            if (portModel.Connectors.Count > 0 && portModel.Connectors[0].Start != portModel)
            {
                activeStartPort = portModel.Connectors[0].Start;
                // Disconnect the connector model from its start and end ports
                // and remove it from the connectors collection. This will also
                // remove the view model.
                ConnectorModel connector = portModel.Connectors[0];
                if (CurrentWorkspace.Connectors.Contains(connector))
                {
                    var models = new List<ModelBase> { connector };
                    CurrentWorkspace.RecordAndDeleteModels(models);
                    connector.Delete();
                }
            }
            else
            {
                activeStartPort = portModel;
            }
        }

        void EndConnection(Guid nodeId, int portIndex, PortType portType)
        {
            bool isInPort = portType == PortType.Input;

            var node = CurrentWorkspace.GetModelInternal(nodeId) as NodeModel;
            if (node == null)
                return;
            
            PortModel portModel = isInPort ? node.InPorts[portIndex] : node.OutPorts[portIndex];
            ConnectorModel connectorToRemove = null;

            // Remove connector if one already exists
            if (portModel.Connectors.Count > 0 && portModel.PortType == PortType.Input)
            {
                connectorToRemove = portModel.Connectors[0];
                connectorToRemove.Delete();
            }

            // We could either connect from an input port to an output port, or 
            // another way around (in which case we swap first and second ports).
            PortModel firstPort, second;
            if (portModel.PortType != PortType.Input)
            {
                firstPort = portModel;
                second = activeStartPort;
            }
            else
            {
                // Create the new connector model
                firstPort = activeStartPort;
                second = portModel;
            }

            ConnectorModel newConnectorModel = ConnectorModel.Make(
                firstPort.Owner,
                second.Owner,
                firstPort.Index,
                second.Index);

            // Record the creation of connector in the undo recorder.
            var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>();
            if (connectorToRemove != null)
                models.Add(connectorToRemove, UndoRedoRecorder.UserAction.Deletion);
            models.Add(newConnectorModel, UndoRedoRecorder.UserAction.Creation);
            WorkspaceModel.RecordModelsForUndo(models, CurrentWorkspace.UndoRecorder);
            activeStartPort = null;
        }

        void DeleteModelImpl(DeleteModelCommand command)
        {
            var modelsToDelete = new List<ModelBase>();
            if (command.ModelGuid != Guid.Empty)
            {
                modelsToDelete.Add(CurrentWorkspace.GetModelInternal(command.ModelGuid));
            }
            else
            {
                // When nothing is specified then it means all selected models.
                modelsToDelete.AddRange(DynamoSelection.Instance.Selection.OfType<ModelBase>());
            }

            DeleteModelInternal(modelsToDelete);
        }

        void UndoRedoImpl(UndoRedoCommand command)
        {
            switch (command.CmdOperation)
            {
                case UndoRedoCommand.Operation.Undo:
                    CurrentWorkspace.Undo();
                    break;
                case UndoRedoCommand.Operation.Redo:
                    CurrentWorkspace.Redo();
                    break;
            }
        }

        void SendModelEventImpl(ModelEventCommand command)
        {
            CurrentWorkspace.SendModelEvent(command.ModelGuid, command.EventName);
        }

        void UpdateModelValueImpl(UpdateModelValueCommand command)
        {
            CurrentWorkspace.UpdateModelValue(command.ModelGuid,
                command.Name, command.Value);
        }

        [Obsolete("Node to Code not enabled, API subject to change.")]
        private void ConvertNodesToCodeImpl(ConvertNodesToCodeCommand command)
        {
            CurrentWorkspace.ConvertNodesToCodeInternal(
                command.NodeId,
                EngineController,
                DebugSettings.VerboseLogging);

            CurrentWorkspace.HasUnsavedChanges = true;
        }

        void CreateCustomNodeImpl(CreateCustomNodeCommand command)
        {
            var workspace = CustomNodeManager.CreateCustomNode(
                command.Name,
                command.Category,
                command.Description, 
                functionId: command.NodeId);

            AddWorkspace(workspace);
            CurrentWorkspace = workspace;
        }

        void SwitchTabImpl(SwitchTabCommand command)
        {
            // We don't attempt to null-check here, we need it to fail fast.
            CurrentWorkspace = Workspaces[command.TabIndex];
        }
    }
}
