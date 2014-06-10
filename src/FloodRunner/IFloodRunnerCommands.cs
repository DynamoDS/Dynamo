using System;
using System.Windows;

using Dynamo.Models;
using Dynamo.ViewModels;

namespace FloodRunner
{
    interface IFloodRunnerCommands
    {
        void OpenFile(string filePath);

        void Run(bool showErrors, bool cancel);

        void ForceRun(bool showErrors, bool cancel);

        void CreateNode(Guid id, string name, double x, double y, bool defaultPosition, bool transformCoordinates);

        void CreateNote(Guid id, string text, double x, double y, bool defaultPosition);

        void SelectModel(Guid id, System.Windows.Input.ModifierKeys modifiers);

        void SelectInRegion(Rect region, bool isCrossSelection);

        void DragSelection(Point cursor, DynamoViewModel.DragSelectionCommand.Operation operation);

        void MakeConnection(Guid nodeId, int portIndex, PortType type, DynamoViewModel.MakeConnectionCommand.Mode mode);

        void DeleteModel(Guid id);

        void UndoRedo(DynamoViewModel.UndoRedoCommand.Operation operation);

        void ModelEvent(Guid id, string eventName);

        void UpdateModelValue(Guid id, string name, string value);

        void ConvertNodesToCode(Guid id);

        void CreateCustomNode(Guid id, string name, string category, string description, bool makeCurrent);

        void SwitchTab(int tabIndex);
    }
}
