﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Dynamo.Core;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Engine;
using Dynamo.Engine.NodeToCode;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;

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

        protected virtual void OpenFileImpl(OpenFileCommand command)
        {
            string xmlFilePath = command.XmlFilePath;
            bool forceManualMode = command.ForceManualExecutionMode;
            OpenFileFromPath(xmlFilePath, forceManualMode);

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
            var node = GetNodeFromCommand(command);
            if (node == null)
                return;

            node.X = command.X;
            node.Y = command.Y;

            AddNodeToCurrentWorkspace(node, centered: command.DefaultPosition);
            CurrentWorkspace.RecordCreatedModel(node);
        }

        void CreateAndConnectNodeImpl(CreateAndConnectNodeCommand command)
        {
            using (CurrentWorkspace.UndoRecorder.BeginActionGroup())
            {
                var newNode = CreateNodeFromNameOrType(command.ModelGuid, command.NewNodeName);
                newNode.X = command.X;
                newNode.Y = command.Y;
                var existingNode = CurrentWorkspace.GetModelInternal(command.ModelGuids.ElementAt(1)) as NodeModel;
                
                if(newNode == null || existingNode == null) return;

                AddNodeToCurrentWorkspace(newNode, false, command.AddNewNodeToSelection);
                CurrentWorkspace.UndoRecorder.RecordCreationForUndo(newNode);

                PortModel inPortModel, outPortModel;
                if (command.CreateAsDownstreamNode)
                {
                    // Connect output port of Existing Node to input port of New node
                    outPortModel = existingNode.OutPorts[command.OutputPortIndex];
                    inPortModel = newNode.InPorts[command.InputPortIndex];
                }
                else
                {
                    // Connect output port of New Node to input port of existing node
                    outPortModel = newNode.OutPorts[command.OutputPortIndex];
                    inPortModel = existingNode.InPorts[command.InputPortIndex];
                }

                var models = GetConnectorsToAddAndDelete(inPortModel, outPortModel);

                foreach (var modelPair in models)
                {
                    switch (modelPair.Value)
                    {
                        case UndoRedoRecorder.UserAction.Creation:
                            CurrentWorkspace.UndoRecorder.RecordCreationForUndo(modelPair.Key);
                            break;
                        case UndoRedoRecorder.UserAction.Deletion:
                            CurrentWorkspace.UndoRecorder.RecordDeletionForUndo(modelPair.Key);
                            break;
                    }
                }
            }
        }

        NodeModel GetNodeFromCommand(CreateNodeCommand command)
        {
            if (command.Node != null)
            {
                return command.Node;
            }

            if (command.NodeXml != null)
            {
                // command was deserialized, we must create the node directly
                return NodeFactory.CreateNodeFromXml(command.NodeXml, SaveContext.File, currentWorkspace.ElementResolver);
            }

            // legacy command, hold on to your butts
            var name = command.Name;
            var nodeId = command.ModelGuid;

            // find nodes with of the same type with the same GUID
            var query = CurrentWorkspace.Nodes.Where(n => n.GUID.Equals(nodeId) && n.Name.Equals(name));

            // safely ignore a node of the same type with the same GUID
            if (query.Any())
            {
                return query.First();
            }

            // To be used in the event it's a custom node we're making.
            Guid customNodeId;

            if (command is CreateProxyNodeCommand)
            {
                var proxyCommand = command as CreateProxyNodeCommand;

                return NodeFactory.CreateProxyNodeInstance(nodeId, name,
                    proxyCommand.NickName, proxyCommand.Inputs, proxyCommand.Outputs);
            }

            // Then, we have to figure out what kind of node to make, based on the name.

            NodeModel node = CreateNodeFromNameOrType(nodeId, name);
            if (node != null) return node;

            // And if that didn't work, then it must be a custom node.
            if (Guid.TryParse(name, out customNodeId))
            {
                node = CustomNodeManager.CreateCustomNodeInstance(customNodeId);
                node.GUID = nodeId;
                return node;
            }

            // We're out of ideas, log an error.
            Logger.LogError("Could not create instance of node with name: " + name);
            return null;
        }

        private NodeModel CreateNodeFromNameOrType(Guid nodeId, string name)
        {
            NodeModel node;

            // First, we check for a DSFunction by looking for a FunctionDescriptor
            var functionItem = LibraryServices.GetFunctionDescriptor(name);
            if (functionItem != null)
            {
                node = (functionItem.IsVarArg)
                    ? new DSVarArgFunction(functionItem) as NodeModel
                    : new DSFunction(functionItem);
                node.GUID = nodeId;
                return node;
            }

            // If that didn't work, let's try using the NodeFactory
            if (NodeFactory.CreateNodeFromTypeName(name, out node))
            {
                node.GUID = nodeId;
            }
            return node;
        }

        void CreateNoteImpl(CreateNoteCommand command)
        {
            NoteModel noteModel = CurrentWorkspace.AddNote(
                command.DefaultPosition,
                command.X,
                command.Y,
                command.NoteText,
                command.ModelGuid);

            CurrentWorkspace.RecordCreatedModel(noteModel);
        }

        void CreateAnnotationImpl(CreateAnnotationCommand command)
        {
            AnnotationModel annotationModel = currentWorkspace.AddAnnotation(command.AnnotationText, command.ModelGuid);
            
            CurrentWorkspace.RecordCreatedModel(annotationModel);
        }

        void SelectModelImpl(SelectModelCommand command)
        {
            // Empty ModelGuid means clear selection.
            if (command.ModelGuid == Guid.Empty)
            {
                DynamoSelection.Instance.ClearSelection();
                return;
            }

            foreach (var guid in command.ModelGuids)
            {
                ModelBase model = CurrentWorkspace.GetModelInternal(guid);

                if (!model.IsSelected)
                {
                    if (!command.Modifiers.HasFlag(ModifierKeys.Shift) && command.ModelGuids.Count() == 1)
                        DynamoSelection.Instance.ClearSelection();

                    DynamoSelection.Instance.Selection.AddUnique(model);
                }
                else
                {
                    if (command.Modifiers.HasFlag(ModifierKeys.Shift))
                        DynamoSelection.Instance.Selection.Remove(model);
                }
            }
        }

        void MakeConnectionImpl(MakeConnectionCommand command)
        {
            Guid nodeId = command.ModelGuid;

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
            activeStartPort = null;

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
            // Check if the node from which the connector starts is valid and has not been deleted
            if (activeStartPort.Owner == null) return;

            var startNode = CurrentWorkspace.GetModelInternal(activeStartPort.Owner.GUID);
            if (startNode == null) return;

            var node = CurrentWorkspace.GetModelInternal(nodeId) as NodeModel;
            if (node == null) return;

            bool isInPort = portType == PortType.Input;
            
            PortModel portModel = isInPort ? node.InPorts[portIndex] : node.OutPorts[portIndex];

            var models = GetConnectorsToAddAndDelete(portModel, activeStartPort);

            WorkspaceModel.RecordModelsForUndo(models, CurrentWorkspace.UndoRecorder);
            activeStartPort = null;
        }

        static Dictionary<ModelBase, UndoRedoRecorder.UserAction> GetConnectorsToAddAndDelete(
            PortModel endPort, PortModel startPort)
        {
            ConnectorModel connectorToRemove = null;

            // Remove connector if one already exists
            if (endPort.Connectors.Count > 0 && endPort.PortType == PortType.Input)
            {
                connectorToRemove = endPort.Connectors[0];
                connectorToRemove.Delete();
            }

            // We could either connect from an input port to an output port, or 
            // another way around (in which case we swap first and second ports).
            PortModel firstPort, secondPort;
            if (endPort.PortType != PortType.Input)
            {
                firstPort = endPort;
                secondPort = startPort;
            }
            else
            {
                // Create the new connector model
                firstPort = startPort;
                secondPort = endPort;
            }

            ConnectorModel newConnectorModel = ConnectorModel.Make(
                firstPort.Owner,
                secondPort.Owner,
                firstPort.Index,
                secondPort.Index);

            // Record the creation of connector in the undo recorder.
            var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>();
            if (connectorToRemove != null)
            {
                models.Add(connectorToRemove, UndoRedoRecorder.UserAction.Deletion);
            }
            models.Add(newConnectorModel, UndoRedoRecorder.UserAction.Creation);
            return models;
        }

        void DeleteModelImpl(DeleteModelCommand command)
        {
            var modelsToDelete = new List<ModelBase>();
            if (command.ModelGuid == Guid.Empty)
            {
                // When nothing is specified then it means all selected models.
                modelsToDelete.AddRange(DynamoSelection.Instance.Selection.OfType<ModelBase>());
            }
            else
            {
                modelsToDelete.AddRange(command.ModelGuids.Select(guid => CurrentWorkspace.GetModelInternal(guid)));
            }

            DeleteModelInternal(modelsToDelete);
        }

        void UngroupModelImpl(UngroupModelCommand command)
        {
            if (command.ModelGuid == Guid.Empty)
                return;

            var modelsToUngroup = command.ModelGuids.Select(guid => CurrentWorkspace.GetModelInternal(guid)).ToList();

            UngroupModel(modelsToUngroup);
        }

        void AddToGroupImpl(AddModelToGroupCommand command)
        {
            if (command.ModelGuid == Guid.Empty)
                return;

            var modelsToGroup = command.ModelGuids.Select(guid => CurrentWorkspace.GetModelInternal(guid)).ToList();

            AddToGroup(modelsToGroup);
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
            foreach (var guid in command.ModelGuids)
            {
                CurrentWorkspace.SendModelEvent(guid, command.EventName);
            }
        }

        void UpdateModelValueImpl(UpdateModelValueCommand command)
        {
            WorkspaceModel targetWorkspace = CurrentWorkspace;
            if (!command.WorkspaceGuid.Equals(Guid.Empty))
                targetWorkspace = Workspaces.FirstOrDefault(w => w.Guid.Equals(command.WorkspaceGuid));

            targetWorkspace.UpdateModelValue(command.ModelGuids,
                command.Name, command.Value);
        }

        private void ConvertNodesToCodeImpl(ConvertNodesToCodeCommand command)
        {
            var libServices = new LibraryCustomizationServices(pathManager);
            var namingProvider = new NamingProvider(EngineController.LibraryServices.LibraryManagementCore, libServices);
            CurrentWorkspace.ConvertNodesToCodeInternal(EngineController, namingProvider);

            CurrentWorkspace.HasUnsavedChanges = true;
        }

        void CreateCustomNodeImpl(CreateCustomNodeCommand command)
        {
            var workspace = CustomNodeManager.CreateCustomNode(
                command.Name,
                command.Category,
                command.Description, 
                command.ModelGuid);

            AddWorkspace(workspace);
            CurrentWorkspace = workspace;
        }

        void SwitchWorkspaceImpl(SwitchTabCommand command)
        {
            // We don't attempt to null-check here, we need it to fail fast.
            CurrentWorkspace = Workspaces.ElementAt(command.WorkspaceModelIndex);
        }

        void CreatePresetStateImpl(AddPresetCommand command)
        {
            var preset = this.CurrentWorkspace.AddPreset(command.PresetStateName,command.PresetStateDescription,command.ModelGuids);

            CurrentWorkspace.RecordCreatedModel(preset);
        }
        void SetWorkSpaceToStateImpl(ApplyPresetCommand command)
        {
            var workspaceToSet = this.Workspaces.Where(x => x.Guid == command.WorkSpaceID).First();
            if (workspaceToSet == null)
            {
                return;
            }
            var state = workspaceToSet.Presets.Where(x => x.GUID == command.StateID).FirstOrDefault();

            workspaceToSet.ApplyPreset (state);
        }

    }
}
