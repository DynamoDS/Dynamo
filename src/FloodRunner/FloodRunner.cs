using Dynamo.Models;
using FloodRunner.Managers;

namespace FloodRunner
{
    class FloodRunner : iFloodRunner, IFloodRunnerCommands
    {
        #region iFloodRunner Members

        public bool AddNode(NodeModel node, object whereToAdd)
        {
            return NodeManager.AddNode(node, whereToAdd);
        }

        public bool DeleteNode(NodeModel node, object whereFromDelete)
        {
            return NodeManager.DeleteNode(node, whereFromDelete);
        }

        public bool AddConnector(ConnectorModel connector, WorkspaceModel whereToAdd)
        {
            return ConnectorManager.AddConnector(connector, whereToAdd);
        }

        public bool DeleteConnector(ConnectorModel connector, WorkspaceModel whereFromDelete)
        {
            return ConnectorManager.DeleteConnector(connector, whereFromDelete);
        }

        public WorkspaceModel NewWorkspace(string name)
        {
            return WorkspaceManager.NewWorkspace(name);
        }

        public WorkspaceModel LoadWorkspace(string filePath)
        {
            return WorkspaceManager.LoadWorkspace(filePath);
        }

        public bool SaveWorkspace(WorkspaceModel workspace, string filePath)
        {
            return WorkspaceManager.SaveWorkspace(workspace, filePath);
        }

        public bool AddNote(NoteModel note, object whereToAdd)
        {
            return NoteManager.AddNote(note, whereToAdd);
        }

        public bool DeleteNote(NoteModel note, object whereFromDelete)
        {
            return NoteManager.DeleteNote(note, whereFromDelete);
        }

        public bool AddPort(PortModel port, object whereToAdd)
        {
            return PortManager.AddPort(port, whereToAdd);
        }

        public bool DeletePort(PortModel port, object whereFromDelete)
        {
            return PortManager.DeletePort(port, whereFromDelete);
        }

        #endregion

        public void OpenFile(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public void Run(bool showErrors, bool cancel)
        {
            throw new System.NotImplementedException();
        }

        public void ForceRun(bool showErrors, bool cancel)
        {
            throw new System.NotImplementedException();
        }

        public void CreateNode(System.Guid id, string name, double x, double y, bool defaultPosition, bool transformCoordinates)
        {
            throw new System.NotImplementedException();
        }

        public void CreateNote(System.Guid id, string text, double x, double y, bool defaultPosition)
        {
            throw new System.NotImplementedException();
        }

        public void SelectModel(System.Guid id, System.Windows.Input.ModifierKeys modifiers)
        {
            throw new System.NotImplementedException();
        }

        public void SelectInRegion(System.Windows.Rect region, bool isCrossSelection)
        {
            throw new System.NotImplementedException();
        }

        public void DragSelection(System.Windows.Point cursor, Dynamo.ViewModels.DynamoViewModel.DragSelectionCommand.Operation operation)
        {
            throw new System.NotImplementedException();
        }

        public void MakeConnection(System.Guid nodeId, int portIndex, PortType type, Dynamo.ViewModels.DynamoViewModel.MakeConnectionCommand.Mode mode)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteModel(System.Guid id)
        {
            throw new System.NotImplementedException();
        }

        public void UndoRedo(Dynamo.ViewModels.DynamoViewModel.UndoRedoCommand.Operation operation)
        {
            throw new System.NotImplementedException();
        }

        public void ModelEvent(System.Guid id, string eventName)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateModelValue(System.Guid id, string name, string value)
        {
            throw new System.NotImplementedException();
        }

        public void ConvertNodesToCode(System.Guid id)
        {
            throw new System.NotImplementedException();
        }

        public void CreateCustomNode(System.Guid id, string name, string category, string description, bool makeCurrent)
        {
            throw new System.NotImplementedException();
        }

        public void SwitchTab(int tabIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}
