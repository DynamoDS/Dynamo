using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        /// <summary>
        /// This is the base class of all recordable commands. It provides the 
        /// contract between a UI event handler (e.g. delegate command method or 
        /// a button event handler) and the actual command handler in the 
        /// DynamoViewModel. It is mandatory for each RecordableCommand-derived 
        /// class to be serializable to/deserializable from an XmlElement.
        /// </summary>
        /// 
        public abstract class RecordableCommand
        {
            #region Class Data Members

            // See property for more details.
            protected bool redundant = false;

            #endregion

            #region Public Class Operational Methods

            /// <summary>
            /// Constructs an instance of RecordableCommand derived class. This 
            /// constructor is made protected to indicate that the class instance 
            /// can only be instantiated through a derived class.
            /// </summary>
            protected RecordableCommand()
            {
                this.IsInPlaybackMode = false;
            }

            /// <summary>
            /// Call this method to execute a RecordableCommand. A RecordableCommand 
            /// must be executed in the context of an existing DynamoViewModel.
            /// </summary>
            /// <param name="dynamoViewModel">The DynamoViewModel object this 
            /// RecordableCommand is targeting.</param>
            /// 
            internal void Execute(DynamoViewModel dynamoViewModel)
            {
                this.ExecuteCore(dynamoViewModel);
            }

            /// <summary>
            /// This method serializes the RecordableCommand object in the form of 
            /// XmlElement for storage. The resulting XmlElement contains all the 
            /// arguments that are required by this command.
            /// </summary>
            /// <param name="document">The XmlDocument from which an XmlElement can
            /// be created.</param>
            /// <returns>The XmlElement representation of this RecordableCommand 
            /// object. It will be used in RecordableCommand.Deserialize method to 
            /// completely reconstruct the RecordableCommand it represents.</returns>
            /// 
            internal XmlElement Serialize(XmlDocument document)
            {
                string commandName = this.GetType().Name;
                XmlElement element = document.CreateElement(commandName);
                this.SerializeCore(element);
                return element;
            }

            /// <summary>
            /// Call this static method to reconstruct a RecordableCommand-derived 
            /// object given an XmlElement that was previously saved with Serialize 
            /// method. This method simply redirects the XmlElement to respective 
            /// RecordableCommand-derived classes based on its type.
            /// </summary>
            /// <param name="element">The XmlElement from which the RecordableCommand
            /// can be reconstructed.</param>
            /// <returns>Returns the reconstructed RecordableCommand object. If a 
            /// RecordableCommand cannot be reconstructed, this method throws a 
            /// relevant exception.</returns>
            /// 
            internal static RecordableCommand Deserialize(XmlElement element)
            {
                if (string.IsNullOrEmpty(element.Name))
                    throw new ArgumentException("XmlElement without name");

                RecordableCommand command = null;

                switch (element.Name)
                {
                    case "OpenFileCommand":
                        command = OpenFileCommand.DeserializeCore(element);
                        break;
                    case "RunCancelCommand":
                        command = RunCancelCommand.DeserializeCore(element);
                        break;
                    case "CreateNodeCommand":
                        command = CreateNodeCommand.DeserializeCore(element);
                        break;
                    case "SelectModelCommand":
                        command = SelectModelCommand.DeserializeCore(element);
                        break;
                    case "CreateNoteCommand":
                        command = CreateNoteCommand.DeserializeCore(element);
                        break;
                    case "SelectInRegionCommand":
                        command = SelectInRegionCommand.DeserializeCore(element);
                        break;
                    case "DragSelectionCommand":
                        command = DragSelectionCommand.DeserializeCore(element);
                        break;
                    case "MakeConnectionCommand":
                        command = MakeConnectionCommand.DeserializeCore(element);
                        break;
                    case "DeleteModelCommand":
                        command = DeleteModelCommand.DeserializeCore(element);
                        break;
                    case "UndoRedoCommand":
                        command = UndoRedoCommand.DeserializeCore(element);
                        break;
                    case "ModelEventCommand":
                        command = ModelEventCommand.DeserializeCore(element);
                        break;
                    case "UpdateModelValueCommand":
                        command = UpdateModelValueCommand.DeserializeCore(element);
                        break;
                    case "ConvertNodesToCodeCommand":
                        command = ConvertNodesToCodeCommand.DeserializeCore(element);
                        break;
                }

                if (null != command)
                {
                    command.IsInPlaybackMode = true;
                    return command;
                }

                string message = string.Format("Unknown command: {0}", element.Name);
                throw new ArgumentException(message);
            }

            #endregion

            #region Public Command Properties

            /// <summary>
            /// Some commands are fired at high frequency (e.g. dragging and window 
            /// selection related commands), and can be simulated during playback by
            /// issuing the final occurrence of the command. For example, window 
            /// selection command is fired for each mouse-move event, but the end 
            /// result will be same if only the final selection command is recorded.
            /// If this property is set to 'true', then only the last occurrence will
            /// be recorded for playback.
            /// </summary>
            internal bool Redundant { get { return this.redundant; } }

            /// <summary>
            /// This flag will be set to true only during playback. Derived classes
            /// can use this to decide their actions accordingly. For example, 
            /// UpdateModelValueCommand doesn't change the value of a property 
            /// during recording time, it is created for the sole purpose of being
            /// recorded. During playback, then UpdateModelValueCommand will update
            /// the property that it is bound to. This is a runtime flag, it is not 
            /// serialized in anyway.
            /// </summary>
            internal bool IsInPlaybackMode { get; private set; }

            #endregion

            #region Protected Overridable Methods

            /// <summary>
            /// Derived classes must implement this method to perform the actual
            /// command execution. A typical implementation of this method involves
            /// calling a corresponding method on DynamoViewModel by passing itself
            /// as the only argument.
            /// </summary>
            /// <param name="dynamoViewModel">The DynamoViewModel object on which 
            /// this command should be executed.</param>
            /// 
            protected abstract void ExecuteCore(DynamoViewModel dynamoViewModel);

            /// <summary>
            /// Derived classes must implement this method to serialize all relevant
            /// information into the XmlElement supplied to it. Typically the method
            /// is a direct mirror of DeserializeCore.
            /// </summary>
            /// <param name="element">All arguments that are required for this 
            /// command are written into this XmlElement. The information written 
            /// here must be exactly what DeserializeCore method expects.</param>
            /// 
            protected abstract void SerializeCore(XmlElement element);

            #endregion
        }

        public class OpenFileCommand : RecordableCommand
        {
            #region Public Class Methods

            public OpenFileCommand(string xmlFilePath)
            {
                this.XmlFilePath = xmlFilePath;
            }

            internal static OpenFileCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                string xmlFilePath = helper.ReadString("XmlFilePath");
                if (File.Exists(xmlFilePath) == false)
                {
                    // Try to find the file right next to the command XML file.
                    string xmlFileName = Path.GetFileName(xmlFilePath);
                    Uri uri = new Uri(element.OwnerDocument.BaseURI);
                    string directory = Path.GetDirectoryName(uri.AbsolutePath);
                    xmlFilePath = Path.Combine(directory, xmlFileName);

                    // If it still cannot be resolved, fall back to system search.
                    if (File.Exists(xmlFilePath) == false)
                        xmlFilePath = Path.GetFullPath(xmlFileName);

                    if (File.Exists(xmlFilePath) == false) // When all else fail.
                    {
                        var message = "Target file cannot be found!";
                        throw new FileNotFoundException(message, xmlFileName);
                    }
                }

                return new OpenFileCommand(xmlFilePath);
            }

            #endregion

            #region Public Command Properties

            internal string XmlFilePath { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.OpenFileImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("XmlFilePath", this.XmlFilePath);
            }

            #endregion
        }

        public class RunCancelCommand : RecordableCommand
        {
            #region Public Class Methods

            public RunCancelCommand(bool showErrors, bool cancelRun)
            {
                this.ShowErrors = showErrors;
                this.CancelRun = cancelRun;
            }

            internal static RunCancelCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                bool showErrors = helper.ReadBoolean("ShowErrors");
                bool cancelRun = helper.ReadBoolean("CancelRun");
                return new RunCancelCommand(showErrors, cancelRun);
            }

            #endregion

            #region Public Command Properties

            internal bool ShowErrors { get; private set; }
            internal bool CancelRun { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.RunCancelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("ShowErrors", this.ShowErrors);
                helper.SetAttribute("CancelRun", this.CancelRun);
            }

            #endregion
        }

        public class CreateNodeCommand : RecordableCommand
        {
            #region Public Class Methods

            public CreateNodeCommand(Guid nodeId, string nodeName,
                double x, double y, bool defaultPosition, bool transformCoordinates)
            {
                this.NodeId = nodeId;
                this.NodeName = nodeName;
                this.X = x;
                this.Y = y;
                this.DefaultPosition = defaultPosition;
                this.TransformCoordinates = transformCoordinates;
            }

            internal static CreateNodeCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                System.Guid nodeId = helper.ReadGuid("NodeId");
                string nodeName = helper.ReadString("NodeName");
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");

                return new CreateNodeCommand(nodeId, nodeName, x, y,
                    helper.ReadBoolean("DefaultPosition"),
                    helper.ReadBoolean("TransformCoordinates"));
            }

            #endregion

            #region Public Command Properties

            internal Guid NodeId { get; private set; }
            internal string NodeName { get; private set; }
            internal double X { get; private set; }
            internal double Y { get; private set; }
            internal bool DefaultPosition { get; private set; }
            internal bool TransformCoordinates { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.CreateNodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", this.NodeId);
                helper.SetAttribute("NodeName", this.NodeName);
                helper.SetAttribute("X", this.X);
                helper.SetAttribute("Y", this.Y);
                helper.SetAttribute("DefaultPosition", this.DefaultPosition);
                helper.SetAttribute("TransformCoordinates", TransformCoordinates);
            }

            #endregion
        }

        public class CreateNoteCommand : RecordableCommand
        {
            #region Public Class Methods

            public CreateNoteCommand(Guid nodeId, string noteText,
                double x, double y, bool defaultPosition)
            {
                if (string.IsNullOrEmpty(noteText))
                    noteText = string.Empty;

                this.NodeId = nodeId;
                this.NoteText = noteText;
                this.X = x;
                this.Y = y;
                this.DefaultPosition = defaultPosition;
            }

            internal static CreateNoteCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                System.Guid nodeId = helper.ReadGuid("NodeId");
                string noteText = helper.ReadString("NoteText");
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");

                return new CreateNoteCommand(nodeId, noteText, x, y,
                    helper.ReadBoolean("DefaultPosition"));
            }

            #endregion

            #region Public Command Properties

            internal Guid NodeId { get; private set; }
            internal string NoteText { get; private set; }
            internal double X { get; private set; }
            internal double Y { get; private set; }
            internal bool DefaultPosition { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.CreateNoteImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", this.NodeId);
                helper.SetAttribute("NoteText", this.NoteText);
                helper.SetAttribute("X", this.X);
                helper.SetAttribute("Y", this.Y);
                helper.SetAttribute("DefaultPosition", this.DefaultPosition);
            }

            #endregion
        }

        public class SelectModelCommand : RecordableCommand
        {
            #region Public Class Methods

            public SelectModelCommand(Guid modelGuid, ModifierKeys modifiers)
            {
                this.ModelGuid = modelGuid;
                this.Modifiers = modifiers;
            }

            internal static SelectModelCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                System.Guid modelGuid = helper.ReadGuid("ModelGuid");
                ModifierKeys modifiers = ((ModifierKeys)helper.ReadInteger("Modifiers"));
                return new SelectModelCommand(modelGuid, modifiers);
            }

            #endregion

            #region Public Command Properties

            internal Guid ModelGuid { get; private set; }
            internal ModifierKeys Modifiers { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.SelectModelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("ModelGuid", this.ModelGuid);
                helper.SetAttribute("Modifiers", ((int)this.Modifiers));
            }

            #endregion
        }

        public class SelectInRegionCommand : RecordableCommand
        {
            #region Public Class Methods

            public SelectInRegionCommand(Rect region, bool isCrossSelection)
            {
                this.redundant = true; // High-frequency command.

                this.Region = region;
                this.IsCrossSelection = isCrossSelection;
            }

            internal static SelectInRegionCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);

                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");
                double width = helper.ReadDouble("Width");
                double height = helper.ReadDouble("Height");

                Rect region = new Rect(x, y, width, height);
                bool isCrossSelection = helper.ReadBoolean("IsCrossSelection");
                return new SelectInRegionCommand(region, isCrossSelection);
            }

            #endregion

            #region Public Command Properties

            internal Rect Region { get; private set; }
            internal bool IsCrossSelection { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.SelectInRegionImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("X", this.Region.X);
                helper.SetAttribute("Y", this.Region.Y);
                helper.SetAttribute("Width", this.Region.Width);
                helper.SetAttribute("Height", this.Region.Height);
                helper.SetAttribute("IsCrossSelection", this.IsCrossSelection);
            }

            #endregion
        }

        public class DragSelectionCommand : RecordableCommand
        {
            #region Public Class Methods

            public enum Operation { BeginDrag, EndDrag }

            public DragSelectionCommand(Point mouseCursor, Operation operation)
            {
                this.MouseCursor = mouseCursor;
                this.DragOperation = operation;
            }

            internal static DragSelectionCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");
                int op = helper.ReadInteger("DragOperation");
                return new DragSelectionCommand(new Point(x, y), ((Operation)op));
            }

            #endregion

            #region Public Command Properties

            internal Operation DragOperation { get; private set; }
            internal Point MouseCursor { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.DragSelectionImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("X", this.MouseCursor.X);
                helper.SetAttribute("Y", this.MouseCursor.Y);
                helper.SetAttribute("DragOperation", ((int)this.DragOperation));
            }

            #endregion
        }

        public class MakeConnectionCommand : RecordableCommand
        {
            #region Public Class Methods

            public enum Mode { Begin, End, Cancel }

            public MakeConnectionCommand(Guid nodeId, int portIndex, PortType portType, Mode mode)
            {
                this.NodeId = nodeId;
                this.PortIndex = portIndex;
                this.Type = portType;
                this.ConnectionMode = mode;
            }

            internal static MakeConnectionCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Guid nodeId = helper.ReadGuid("NodeId");
                int portIndex = helper.ReadInteger("PortIndex");
                PortType portType = ((PortType)helper.ReadInteger("Type"));
                Mode mode = ((Mode)helper.ReadInteger("ConnectionMode"));
                return new MakeConnectionCommand(nodeId, portIndex, portType, mode);
            }

            #endregion

            #region Public Command Properties

            internal Guid NodeId { get; private set; }
            internal int PortIndex { get; private set; }
            internal PortType Type { get; private set; }
            internal Mode ConnectionMode { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.MakeConnectionImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", this.NodeId);
                helper.SetAttribute("PortIndex", this.PortIndex);
                helper.SetAttribute("Type", ((int)this.Type));
                helper.SetAttribute("ConnectionMode", ((int)this.ConnectionMode));
            }

            #endregion
        }

        public class DeleteModelCommand : RecordableCommand
        {
            #region Public Class Methods

            public DeleteModelCommand(Guid modelGuid)
            {
                this.ModelGuid = modelGuid;
            }

            internal static DeleteModelCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                System.Guid modelGuid = helper.ReadGuid("ModelGuid");
                return new DeleteModelCommand(modelGuid);
            }

            #endregion

            #region Public Command Properties

            internal Guid ModelGuid { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.DeleteModelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("ModelGuid", this.ModelGuid);
            }

            #endregion
        }

        public class UndoRedoCommand : RecordableCommand
        {
            #region Public Class Methods

            public enum Operation { Undo, Redo }

            public UndoRedoCommand(Operation operation)
            {
                this.CmdOperation = operation;
            }

            internal static UndoRedoCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                int operation = helper.ReadInteger("CmdOperation");
                return new UndoRedoCommand((Operation)operation);
            }

            #endregion

            #region Public Command Properties

            internal Operation CmdOperation { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.UndoRedoImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("CmdOperation", ((int)this.CmdOperation));
            }

            #endregion
        }

        public class ModelEventCommand : RecordableCommand
        {
            #region Public Class Methods

            internal ModelEventCommand(Guid modelGuid, string eventName)
            {
                this.ModelGuid = modelGuid;
                this.EventName = eventName;
            }

            internal static ModelEventCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Guid modelGuid = helper.ReadGuid("ModelGuid");
                string eventName = helper.ReadString("EventName");
                return new ModelEventCommand(modelGuid, eventName);
            }

            #endregion

            #region Public Command Properties

            internal Guid ModelGuid { get; private set; }
            internal string EventName { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.SendModelEventImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("ModelGuid", this.ModelGuid);
                helper.SetAttribute("EventName", this.EventName);
            }

            #endregion
        }

        public class UpdateModelValueCommand : RecordableCommand
        {
            #region Public Class Methods

            public UpdateModelValueCommand(Guid modelGuid, string name, string value)
            {
                this.ModelGuid = modelGuid;
                this.Name = name;
                this.Value = value;
            }

            internal static UpdateModelValueCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Guid modelGuid = helper.ReadGuid("ModelGuid");
                string name = helper.ReadString("Name");
                string value = helper.ReadString("Value");
                return new UpdateModelValueCommand(modelGuid, name, value);
            }

            #endregion

            #region Public Command Properties

            internal Guid ModelGuid { get; private set; }
            internal string Name { get; private set; }
            internal string Value { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.UpdateModelValueImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("ModelGuid", this.ModelGuid);
                helper.SetAttribute("Name", this.Name);
                helper.SetAttribute("Value", this.Value);
            }

            #endregion
        }

        public class ConvertNodesToCodeCommand : RecordableCommand
        {
            #region Public Class Methods

            internal ConvertNodesToCodeCommand(Guid nodeId)
            {
                this.NodeId = nodeId;
            }

            internal static ConvertNodesToCodeCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                System.Guid nodeId = helper.ReadGuid("NodeId");
                return new ConvertNodesToCodeCommand(nodeId);
            }

            #endregion

            #region Public Command Properties

            internal Guid NodeId { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
            {
                dynamoViewModel.ConvertNodesToCodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", this.NodeId);
            }

            #endregion
        }
    }

    // public class XxxYyyCommand : RecordableCommand
    // {
    //     #region Public Class Methods
    // 
    //     public XxxYyyCommand()
    //     {
    //     }
    // 
    //     internal static XxxYyyCommand DeserializeCore(XmlElement element)
    //     {
    //         throw new NotImplementedException();
    //     }
    // 
    //     #endregion
    // 
    //     #region Public Command Properties
    //     #endregion
    // 
    //     #region Protected Overridable Methods
    // 
    //     protected override void ExecuteCore(DynamoViewModel dynamoViewModel)
    //     {
    //         throw new NotImplementedException();
    //     }
    // 
    //     protected override void SerializeCore(XmlElement element)
    //     {
    //         throw new NotImplementedException();
    //     }
    // 
    //     #endregion
    // }
}
