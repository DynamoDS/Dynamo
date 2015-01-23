﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    public partial class DynamoModel
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
                : this(string.Empty)
            {
            }

            /// <summary>
            /// Constructs an instance of RecordableCommand derived class, 
            /// assigning a new tag to it.
            /// </summary>
            /// <param name="tag">A string tag to be assigned to the command.
            /// This parameter can be any string, even an empty one. However, 
            /// it should not be null. A null "tag" parameter causes the 
            /// ArgumentNullException to be thrown.</param>
            protected RecordableCommand(string tag)
            {
                if (tag == null)
                    throw new ArgumentNullException("tag");

                Tag = tag;
                IsInPlaybackMode = false;
            }

            /// <summary>
            /// Call this method to execute a RecordableCommand. A RecordableCommand 
            /// must be executed in the context of an existing DynamoViewModel.
            /// </summary>
            /// <param name="dynamoModel">The DynamoModel object this 
            /// RecordableCommand is targeting.</param>
            /// 
            internal void Execute(DynamoModel dynamoModel)
            {
                ExecuteCore(dynamoModel);
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
                string commandName = GetType().Name;
                XmlElement element = document.CreateElement(commandName);
                SerializeCore(element);
                return element;
            }

            /// <summary>
            /// Call this static method to reconstruct a RecordableCommand-derived 
            /// object given an XmlElement that was previously saved with Serialize 
            /// method. This method simply redirects the XmlElement to respective 
            /// RecordableCommand-derived classes based on its type.
            /// </summary>
            /// <param name="element">The XmlElement from which the RecordableCommand
            ///     can be reconstructed.</param>
            /// <returns>Returns the reconstructed RecordableCommand object. If a 
            /// RecordableCommand cannot be reconstructed, this method throws a 
            /// relevant exception.</returns>
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
                    case "PausePlaybackCommand":
                        command = PausePlaybackCommand.DeserializeCore(element);
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
                    case "CreateCustomNodeCommand":
                        command = CreateCustomNodeCommand.DeserializeCore(element);
                        break;
                    case "SwitchTabCommand":
                        command = SwitchTabCommand.DeserializeCore(element);
                        break;
                }

                if (null != command)
                {
                    command.IsInPlaybackMode = true;
                    command.Tag = element.GetAttribute("Tag");
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
            internal bool Redundant { get { return redundant; } }

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

            /// <summary>
            /// This is an optional tag for each of the recorded commands in a 
            /// command Xml file. A command can only be tagged from within a 
            /// command Xml file manually, and a tag is useful for unit test 
            /// verification passes. See PlaybackStateChangedEventArgs class for 
            /// possible usage of command tags. If a command is not tagged, its 
            /// default tag value is an empty string.
            /// </summary>
            internal string Tag { get; private set; }

            #endregion

            #region Protected Overridable Methods

            /// <summary>
            /// Derived classes must implement this method to perform the actual
            /// command execution. A typical implementation of this method involves
            /// calling a corresponding method on DynamoModel by passing itself as
            /// the only argument.
            /// </summary>
            /// <param name="dynamoModel">The DynamoModel object on which 
            /// this command should be executed.</param>
            /// 
            protected abstract void ExecuteCore(DynamoModel dynamoModel);

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

        public class PausePlaybackCommand : RecordableCommand
        {
            #region Public Class Methods

            public PausePlaybackCommand(int pauseDurationInMs)
                : base(GenerateRandomTag())
            {
                PauseDurationInMs = pauseDurationInMs;
            }

            internal static PausePlaybackCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var pauseDurationInMs = helper.ReadInteger("PauseDurationInMs");
                return new PausePlaybackCommand(pauseDurationInMs);
            }

            #endregion

            #region Public Command Properties

            internal int PauseDurationInMs { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                // A PausePlaybackCommand should never be executed.
                throw new NotImplementedException();
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("Tag", Tag);
                helper.SetAttribute("PauseDurationInMs", PauseDurationInMs);
            }

            #endregion

            #region Private Class Helper Methods

            private static string GenerateRandomTag()
            {
                // Given a GUID in the form AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE,
                // extract out only the AAAAAAAA part (we don't want the tag name
                // to be that long, considering the chances of collision in the 
                // same recorded XML file is near zero, and that these tags are 
                // usually renamed after they are recorded by a test developer).
                // 
                string guid = Guid.NewGuid().ToString();
                guid = guid.Substring(0, guid.IndexOf('-'));
                return string.Format("Tag-{0}", guid);
            }

            #endregion
        }

        public class OpenFileCommand : RecordableCommand
        {
            #region Public Class Methods

            public OpenFileCommand(string xmlFilePath)
            {
                XmlFilePath = xmlFilePath;
            }

            internal static OpenFileCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                string xmlFilePath = helper.ReadString("XmlFilePath");
                if (File.Exists(xmlFilePath)) 
                    return new OpenFileCommand(xmlFilePath);

                // Try to find the file right next to the command XML file.
                string xmlFileName = Path.GetFileName(xmlFilePath);
                var uri = new Uri(element.OwnerDocument.BaseURI);
                string directory = Path.GetDirectoryName(uri.AbsolutePath);
                xmlFilePath = Path.Combine(directory, xmlFileName);

                // If it still cannot be resolved, fall back to system search.
                if (File.Exists(xmlFilePath) == false)
                    xmlFilePath = Path.GetFullPath(xmlFileName);

                if (File.Exists(xmlFilePath)) 
                    return new OpenFileCommand(xmlFilePath);

                const string message = "Target file cannot be found!";
                throw new FileNotFoundException(message, xmlFileName);
            }

            #endregion

            #region Public Command Properties

            internal string XmlFilePath { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.OpenFileImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("XmlFilePath", XmlFilePath);
            }

            #endregion
        }

        public class RunCancelCommand : RecordableCommand
        {
            #region Public Class Methods

            public RunCancelCommand(bool showErrors, bool cancelRun)
            {
                ShowErrors = showErrors;
                CancelRun = cancelRun;
            }

            internal static RunCancelCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
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

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.RunCancelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("ShowErrors", ShowErrors);
                helper.SetAttribute("CancelRun", CancelRun);
            }

            #endregion
        }





        public class ForceRunCancelCommand : RunCancelCommand
        {

            public ForceRunCancelCommand(bool showErrors, bool cancelRun)
                : base(showErrors, cancelRun)
            {
            }


            protected override void ExecuteCore(DynamoModel dynamoModel)
            {

                dynamoModel.ForceRunCancelImpl(this);
            }

        }

        public class MutateTestCommand : RecordableCommand
        {

            protected override void ExecuteCore(DynamoModel dynamoModel) { }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
            }

        }
        
        public class CreateNodeCommand : RecordableCommand
        {
            #region Public Class Methods

            public CreateNodeCommand(
                NodeModel node, double x, double y, bool defaultPosition, bool transformCoordinates)
                : this(x, y, defaultPosition, transformCoordinates)
            {
                Node = node;
            }

            private CreateNodeCommand(
               XmlElement node, double x, double y, bool defaultPosition, bool transformCoordinates)
                : this(x, y, defaultPosition, transformCoordinates)
            {
                NodeXml = node;
            }

            public CreateNodeCommand(Guid nodeId, string name, double x, double y, bool defaultPos, bool transformCoords)
                : this(x, y, defaultPos, transformCoords)
            {
                NodeId = nodeId;
                Name = name;
            }

            private CreateNodeCommand(double x, double y, bool defaultPos, bool transformCoords)
            {
                X = x;
                Y = y;
                DefaultPosition = defaultPos;
                TransformCoordinates = transformCoords;
            }

            internal static CreateNodeCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");
                bool defaultPos = helper.ReadBoolean("DefaultPosition");
                bool transformCoords = helper.ReadBoolean("TransformCoordinates");

                var nodeElement = element.ChildNodes.OfType<XmlElement>().FirstOrDefault();
                
                if (nodeElement == null)
                {
                    // Get the old NodeId and NodeName attributes
                    Guid nodeId = helper.ReadGuid("NodeId");
                    string name = helper.ReadString("NodeName");

                    return new CreateNodeCommand(nodeId, name, x, y, defaultPos, transformCoords);
                }

                return new CreateNodeCommand(nodeElement, x, y, defaultPos, transformCoords);
            }

            #endregion

            #region Public Command Properties

            // Faster, direct creation
            internal NodeModel Node { get; private set; }

            // If it was deserialized
            internal XmlElement NodeXml { get; private set; }

            internal double X { get; private set; }
            internal double Y { get; private set; }
            internal bool DefaultPosition { get; private set; }
            internal bool TransformCoordinates { get; private set; }

            //Legacy properties
            internal string Name { get; private set; }
            internal Guid NodeId { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateNodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("X", X);
                helper.SetAttribute("Y", Y);
                helper.SetAttribute("DefaultPosition", DefaultPosition);
                helper.SetAttribute("TransformCoordinates", TransformCoordinates);

                if (Node != null)
                {
                    var nodeElement = Node.Serialize(element.OwnerDocument, SaveContext.File);
                    element.AppendChild(nodeElement);
                }
                else if (NodeXml != null)
                {
                    element.AppendChild(NodeXml);
                }
                else
                {
                    helper.SetAttribute("NodeId", NodeId);
                    helper.SetAttribute("NodeName", Name);
                }
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

                NodeId = nodeId;
                NoteText = noteText;
                X = x;
                Y = y;
                DefaultPosition = defaultPosition;
            }

            internal static CreateNoteCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                Guid nodeId = helper.ReadGuid("NodeId");
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

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateNoteImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", NodeId);
                helper.SetAttribute("NoteText", NoteText);
                helper.SetAttribute("X", X);
                helper.SetAttribute("Y", Y);
                helper.SetAttribute("DefaultPosition", DefaultPosition);
            }

            #endregion
        }

        public class SelectModelCommand : RecordableCommand
        {
            #region Public Class Methods

            public SelectModelCommand(Guid modelGuid, ModifierKeys modifiers)
            {
                ModelGuid = modelGuid;
                Modifiers = modifiers;
            }

            internal static SelectModelCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                Guid modelGuid = helper.ReadGuid("ModelGuid");
                var modifiers = ((ModifierKeys)helper.ReadInteger("Modifiers"));
                return new SelectModelCommand(modelGuid, modifiers);
            }

            #endregion

            #region Public Command Properties

            internal Guid ModelGuid { get; private set; }
            internal ModifierKeys Modifiers { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.SelectModelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("ModelGuid", ModelGuid);
                helper.SetAttribute("Modifiers", ((int)Modifiers));
            }

            #endregion
        }

        public class SelectInRegionCommand : RecordableCommand
        {
            #region Public Class Methods

            public SelectInRegionCommand(Rect2D region, bool isCrossSelection)
            {
                redundant = true; // High-frequency command.

                Region = region;
                IsCrossSelection = isCrossSelection;
            }

            internal static SelectInRegionCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);

                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");
                double width = helper.ReadDouble("Width");
                double height = helper.ReadDouble("Height");

                var region = new Rect2D(x, y, width, height);
                bool isCrossSelection = helper.ReadBoolean("IsCrossSelection");
                return new SelectInRegionCommand(region, isCrossSelection);
            }

            #endregion

            #region Public Command Properties

            internal Rect2D Region { get; private set; }
            internal bool IsCrossSelection { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel) { }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("X", Region.X);
                helper.SetAttribute("Y", Region.Y);
                helper.SetAttribute("Width", Region.Width);
                helper.SetAttribute("Height", Region.Height);
                helper.SetAttribute("IsCrossSelection", IsCrossSelection);
            }

            #endregion
        }

        public class DragSelectionCommand : RecordableCommand
        {
            #region Public Class Methods

            public enum Operation { BeginDrag, EndDrag }

            public DragSelectionCommand(Point2D mouseCursor, Operation operation)
            {
                MouseCursor = mouseCursor;
                DragOperation = operation;
            }

            internal static DragSelectionCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");
                int op = helper.ReadInteger("DragOperation");
                return new DragSelectionCommand(new Point2D(x, y), ((Operation)op));
            }

            #endregion

            #region Public Command Properties

            internal Operation DragOperation { get; private set; }
            internal Point2D MouseCursor { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel) { }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("X", MouseCursor.X);
                helper.SetAttribute("Y", MouseCursor.Y);
                helper.SetAttribute("DragOperation", ((int)DragOperation));
            }

            #endregion
        }

        public class MakeConnectionCommand : RecordableCommand
        {
            #region Public Class Methods

            public enum Mode { Begin, End, Cancel }

            public MakeConnectionCommand(Guid nodeId, int portIndex, PortType portType, Mode mode)
            {
                NodeId = nodeId;
                PortIndex = portIndex;
                Type = portType;
                ConnectionMode = mode;
            }

            internal static MakeConnectionCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                Guid nodeId = helper.ReadGuid("NodeId");
                int portIndex = helper.ReadInteger("PortIndex");
                var portType = ((PortType)helper.ReadInteger("Type"));
                var mode = ((Mode)helper.ReadInteger("ConnectionMode"));
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

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.MakeConnectionImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", NodeId);
                helper.SetAttribute("PortIndex", PortIndex);
                helper.SetAttribute("Type", ((int)Type));
                helper.SetAttribute("ConnectionMode", ((int)ConnectionMode));
            }

            #endregion
        }

        public class DeleteModelCommand : RecordableCommand
        {
            #region Public Class Methods

            public DeleteModelCommand(Guid modelGuid)
            {
                ModelGuid = modelGuid;
            }

            internal static DeleteModelCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                Guid modelGuid = helper.ReadGuid("ModelGuid");
                return new DeleteModelCommand(modelGuid);
            }

            #endregion

            #region Public Command Properties

            internal Guid ModelGuid { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.DeleteModelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("ModelGuid", ModelGuid);
            }

            #endregion
        }

        public class UndoRedoCommand : RecordableCommand
        {
            #region Public Class Methods

            public enum Operation { Undo, Redo }

            public UndoRedoCommand(Operation operation)
            {
                CmdOperation = operation;
            }

            internal static UndoRedoCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                int operation = helper.ReadInteger("CmdOperation");
                return new UndoRedoCommand((Operation)operation);
            }

            #endregion

            #region Public Command Properties

            internal Operation CmdOperation { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.UndoRedoImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("CmdOperation", ((int)CmdOperation));
            }

            #endregion
        }

        public class ModelEventCommand : RecordableCommand
        {
            #region Public Class Methods

            internal ModelEventCommand(Guid modelGuid, string eventName)
            {
                ModelGuid = modelGuid;
                EventName = eventName;
            }

            internal static ModelEventCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
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

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.SendModelEventImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("ModelGuid", ModelGuid);
                helper.SetAttribute("EventName", EventName);
            }

            #endregion
        }

        public class UpdateModelValueCommand : RecordableCommand
        {
            private readonly List<Guid> modelGuids;

            #region Public Class Methods

            public UpdateModelValueCommand(Guid modelGuid, string name, string value)
                : this(new[] {modelGuid}, name, value)
            {
            }

            public UpdateModelValueCommand(IEnumerable<Guid> modelGuids, string name, string value)
            {
                this.modelGuids = new List<Guid>(modelGuids);
                Name = name;
                Value = value;
            }

            internal static UpdateModelValueCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                string name = helper.ReadString("Name");
                string value = helper.ReadString("Value");

                Guid modelGuid = helper.ReadGuid("ModelGuid", Guid.Empty);
                if (modelGuid != Guid.Empty)
                {
                    // An old type of 'UpdateModelValueCommand' works for only one 
                    // 'NodeModel' whose Guid is stored under 'ModelGuid' attribute.
                    return new UpdateModelValueCommand(modelGuid, name, value);
                }
                else
                {
                    // Parsing a new type of 'UpdateModelValueCommand' that works 
                    // for multiple 'NodeModel' whose Guid values are each stored 
                    // as a 'ModelGuid' child element under the main element.
                    // 
                    var modelGuids = (from XmlNode xmlNode in element.ChildNodes
                                      where xmlNode.Name.Equals("ModelGuid")
                                      select Guid.Parse(xmlNode.InnerText)).ToList();

                    return new UpdateModelValueCommand(modelGuids, name, value);
                }
            }

            #endregion

            #region Public Command Properties

            internal IEnumerable<Guid> ModelGuids { get { return modelGuids; } }
            internal string Name { get; private set; }
            internal string Value { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.UpdateModelValueImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("Name", Name);
                helper.SetAttribute("Value", Value);

                var document = element.OwnerDocument;
                foreach (var modelGuid in modelGuids)
                {
                    var childNode = document.CreateElement("ModelGuid");
                    childNode.InnerText = modelGuid.ToString();
                    element.AppendChild(childNode);
                }
            }

            public override string ToString()
            {
                // This method will be removed if no one is referencing it.
                throw new NotImplementedException("UpdateModelValueCommand.ToString");
            }

            #endregion
        }

        [Obsolete("Node to Code not enabled, API subject to change.")]
        public class ConvertNodesToCodeCommand : RecordableCommand
        {
            #region Public Class Methods

            internal ConvertNodesToCodeCommand(Guid nodeId)
            {
                NodeId = nodeId;
            }

            internal static ConvertNodesToCodeCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                Guid nodeId = helper.ReadGuid("NodeId");
                return new ConvertNodesToCodeCommand(nodeId);
            }

            #endregion

            #region Public Command Properties

            internal Guid NodeId { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.ConvertNodesToCodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", NodeId);
            }

            #endregion
        }

        public class CreateCustomNodeCommand : RecordableCommand
        {
            #region Public Class Methods

            internal CreateCustomNodeCommand(Guid nodeId, string name,
                string category, string description, bool makeCurrent)
            {
                NodeId = nodeId;
                Name = name;
                Category = category;
                Description = description;
                MakeCurrent = makeCurrent;
            }

            internal static CreateCustomNodeCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);

                return new CreateCustomNodeCommand(
                    helper.ReadGuid("NodeId"),
                    helper.ReadString("Name"),
                    helper.ReadString("Category"),
                    helper.ReadString("Description"),
                    helper.ReadBoolean("MakeCurrent"));
            }

            #endregion

            #region Public Command Properties

            internal Guid NodeId { get; private set; }
            internal string Name { get; private set; }
            internal string Category { get; private set; }
            internal string Description { get; private set; }
            internal bool MakeCurrent { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateCustomNodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("NodeId", NodeId);
                helper.SetAttribute("Name", Name);
                helper.SetAttribute("Category", Category);
                helper.SetAttribute("Description", Description);
                helper.SetAttribute("MakeCurrent", MakeCurrent);
            }

            #endregion
        }

        public class SwitchTabCommand : RecordableCommand
        {
            #region Public Class Methods

            public SwitchTabCommand(int tabIndex)
            {
                TabIndex = tabIndex;
            }

            internal static SwitchTabCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                return new SwitchTabCommand(helper.ReadInteger("TabIndex"));
            }

            #endregion

            #region Public Command Properties

            internal int TabIndex { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.SwitchTabImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("TabIndex", TabIndex);
            }

            #endregion
        }
    }

    // public class XxxYyyCommand : RecordableCommand
    // {
    //     #region Public Class Methods
    // 
    //     internal XxxYyyCommand()
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
    //     protected override void ExecuteCore(DynamoModel dynamoModel)
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
