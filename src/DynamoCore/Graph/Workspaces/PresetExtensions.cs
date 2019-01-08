using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Presets;
using Dynamo.Selection;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Extension methods for adding and removing Presets to Workspaces.
    /// </summary>
    public static class PresetExtensions
    {
        /// <summary>
        ///  this method creates a new preset state from a set of NodeModels and adds this new state to this presets collection
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="name">the name of preset state</param>
        /// <param name="description">a description of what the state does</param>
        /// <param name="currentSelection">a set of NodeModels that are to be serialized in this state</param>
        private static PresetModel AddPresetCore(this WorkspaceModel workspace, string name, string description, IEnumerable<NodeModel> currentSelection)
        {
            if (currentSelection == null || currentSelection.Count() < 1)
            {
                throw new ArgumentException("currentSelection is empty or null");
            }
            var inputs = currentSelection;

            var newstate = new PresetModel(name, description, inputs);
            if (workspace.Presets.Any(x => x.GUID == newstate.GUID))
            {
                throw new ArgumentException("duplicate id in collection");
            }

            workspace.presets.Add(newstate);
            return newstate;
        }

        /// <summary>
        /// Removes a specified <see cref="PresetModel"/> object from the preset collection of the workspace.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="state"><see cref="PresetModel"/> object to remove.</param>
        internal static void RemovePreset(this WorkspaceModel workspace, PresetModel state)
        {
            if (workspace.Presets.Contains(state))
            {
                workspace.presets.Remove(state);
            }
        }

        internal static bool ApplyPreset(this WorkspaceModel workspace, PresetModel state)
        {
            if (state == null)
            {
                return false;
            }

            //start an undoBeginGroup
            using (var undoGroup = workspace.UndoRecorder.BeginActionGroup())
            {
                //reload each node, and record each each modification in the undogroup
                foreach (var node in state.Nodes)
                {
                    //check that node still exists in this workspace,
                    //otherwise bail on this node, check by GUID instead of nodemodel
                    if (workspace.Nodes.Select(x => x.GUID).Contains(node.GUID))
                    {
                        var originalpos = node.Position;
                        var serializedNode = state.SerializedNodes.ToList().Find(x => Guid.Parse(x.GetAttribute("guid")) == node.GUID);
                        //overwrite the xy coords of the serialized node with the current position, so the node is not moved
                        serializedNode.SetAttribute("x", originalpos.X.ToString(CultureInfo.InvariantCulture));
                        serializedNode.SetAttribute("y", originalpos.Y.ToString(CultureInfo.InvariantCulture));
                        serializedNode.SetAttribute("isPinned", node.PreviewPinned.ToString());

                        workspace.UndoRecorder.RecordModificationForUndo(node);
                        workspace.ReloadModel(serializedNode);
                    }
                }
                //select all the modified nodes in the UI
                DynamoSelection.Instance.ClearSelection();
                foreach (var node in state.Nodes)
                {
                    DynamoSelection.Instance.Selection.Add(node);
                }
            }

            return true;
        }

        internal static PresetModel AddPreset(this WorkspaceModel workspace, string name, string description, IEnumerable<Guid> IDSToSave)
        {
            //lookup the nodes by their ID, can also check that we find all of them....
            var nodesFromIDs = workspace.Nodes.Where(node => IDSToSave.Contains(node.GUID));
            //access the presetsCollection and add a new state based on the current selection
            var newpreset = AddPresetCore(workspace, name, description, nodesFromIDs);
            workspace.HasUnsavedChanges = true;
            return newpreset;
        }

        /// <summary>
        /// Adds a specified collection <see cref="PresetModel"/> objects to the preset collection of the workspace.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="presetCollection"><see cref="PresetModel"/> objects to add.</param>
        public static void ImportPresets(this WorkspaceModel workspace, IEnumerable<PresetModel> presetCollection)
        {
            workspace.presets.AddRange(presetCollection);
        }
    }
}
