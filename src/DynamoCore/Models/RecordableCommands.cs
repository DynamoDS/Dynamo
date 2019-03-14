using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Properties;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Globalization;

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
        [DataContract]
        public abstract class RecordableCommand
        {
            #region Class Data Members

            // See property for more details.
            protected bool redundant = false;

            /// <summary>
            /// Settings that is used for serializing commands
            /// </summary>
            protected static JsonSerializerSettings jsonSettings;

            /// <summary>
            /// Initialize commands serializing settings
            /// </summary>
            static RecordableCommand()
            {
                jsonSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Culture = CultureInfo.InvariantCulture
                };
            }

            #endregion

            #region Public Class Operational Methods

            /// <summary>
            /// Constructs an instance of RecordableCommand derived class. This
            /// constructor is made protected to indicate that the class instance
            /// can only be instantiated through a derived class.
            /// </summary>
            protected RecordableCommand()
                : this(string.Empty) { }

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
            /// This method serializes the RecordableCommand object in the json form.
            /// The resulting string contains command type name and all the
            /// arguments that are required by this command.
            /// </summary>
            /// <returns>The string can be used for reconstructing RecordableCommand
            /// using Deserialize method</returns>
            internal string Serialize()
            {
                return JsonConvert.SerializeObject(this, jsonSettings);
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
                    case "CreateAnnotationCommand":
                        command = CreateAnnotationCommand.DeserializeCore(element);
                        break;
                    case "UngroupModelCommand":
                        command = UngroupModelCommand.DeserializeCore(element);
                        break;
                    case "AddPresetCommand":
                        command = AddPresetCommand.DeserializeCore(element);
                        break;
                    case "ApplyPresetCommand":
                        command = ApplyPresetCommand.DeserializeCore(element);
                        break;
                    case "CreateAndConnectNodeCommand":
                        command = CreateAndConnectNodeCommand.DeserializeCore(element);
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

            /// <summary>
            /// Call this static method to reconstruct a RecordableCommand from json
            /// string that contains command name - name of corresponding class inherited
            /// from RecordableCommand, - and all the arguments that are required by this
            /// command.
            /// </summary>
            /// <param name="jsonString">Json string that contains command name and all
            /// its arguments.</param>
            /// <returns>Reconstructed RecordableCommand</returns>
            internal static RecordableCommand Deserialize(string jsonString)
            {
                RecordableCommand command = null;
                try
                {
                    command = JsonConvert.DeserializeObject(jsonString, jsonSettings) as RecordableCommand;
                    command.IsInPlaybackMode = true;
                    return command;
                }
                catch
                {
                    throw new ApplicationException("Invalid jsonString for creating RecordableCommand");
                }
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
            [DataMember]
            internal bool IsInPlaybackMode { get; private set; }

            /// <summary>
            /// This is an optional tag for each of the recorded commands in a
            /// command Xml file. A command can only be tagged from within a
            /// command Xml file manually, and a tag is useful for unit test
            /// verification passes. See PlaybackStateChangedEventArgs class for
            /// possible usage of command tags. If a command is not tagged, its
            /// default tag value is an empty string.
            /// </summary>
            [DataMember]
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

            /// <summary>
            /// This method provides derived class implementation of analytics
            /// tracking, and only gets called if analytics tracking is enabled.
            /// </summary>
            internal virtual void TrackAnalytics() { }

            #endregion
        }

        /// <summary>
        /// This class is base for those RecordableCommands that should have
        /// Guid that causes the problems during deserialization
        /// </summary>
        [DataContract]
        public abstract class ModelBasedRecordableCommand : RecordableCommand
        {
            /// <summary>
            /// The first <see cref="Guid"/> in the <seealso cref="ModelGuid"/> collection, or an empty <see cref="Guid"/>.
            /// </summary>
            public Guid ModelGuid
            {
                get
                {
                    return ModelGuids.Any() ? ModelGuids.First() : Guid.Empty;
                }
            }

            /// <summary>
            /// A collection of <see cref="Guid"/>.
            /// </summary>
            public IEnumerable<Guid> ModelGuids { get; private set; }

            /// <summary>
            /// A collection of <see cref="Guid"/> identifying the objects on which to operate.
            /// </summary>
            /// <param name="guids"></param>
            protected ModelBasedRecordableCommand(IEnumerable<Guid> guids)
            {
                ModelGuids = guids;
            }

            protected override void SerializeCore(XmlElement element)
            {
                var document = element.OwnerDocument;
                foreach (var modelGuid in ModelGuids)
                {
                    var childNode = document.CreateElement("ModelGuid");
                    childNode.InnerText = modelGuid.ToString();
                    element.AppendChild(childNode);
                }
            }

            protected static IEnumerable<Guid> DeserializeGuid(XmlElement element, XmlElementHelper helper)
            {
                // Deserialize old type of commands
                if (helper.HasAttribute("ModelGuid"))
                {
                    Guid modelGuid = helper.ReadGuid("ModelGuid", Guid.Empty);
                    return new[] { modelGuid };
                }

                if (helper.HasAttribute("NodeId"))
                {
                    Guid modelGuid = helper.ReadGuid("NodeId", Guid.Empty);
                    return new[] { modelGuid };
                }

                return (from XmlNode xmlNode in element.ChildNodes
                        where xmlNode.Name.Equals("ModelGuid")
                        select Guid.Parse(xmlNode.InnerText)).ToArray();
            }
        }

        /// <summary>
        /// A command used to pause command playback.
        /// </summary>
        [DataContract]
        public class PausePlaybackCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="pauseDurationInMs">The duration to pause the playback in milliseconds.</param>
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

            [DataMember]
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

        /// <summary>
        /// A command to open a file.
        /// </summary>
        [DataContract]
        public class OpenFileCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="filePath">The path to the file.</param>
            /// <param name="forceManualExecutionMode">Should the file be opened in manual execution mode?</param>
            public OpenFileCommand(string filePath, bool forceManualExecutionMode = false)
            {
                FilePath = filePath;
                ForceManualExecutionMode = forceManualExecutionMode;
            }

            private static string TryFindFile(string xmlFilePath, string uriString = null)
            {
                if (File.Exists(xmlFilePath))
                    return xmlFilePath;

                var xmlFileName = Path.GetFileName(xmlFilePath);
                if (uriString != null)
                {
                    // Try to find the file right next to the command XML file.
                    Uri uri = new Uri(uriString);
                    string directory = Path.GetDirectoryName(uri.AbsolutePath);
                    xmlFilePath = Path.Combine(directory, xmlFileName);

                    // If it still cannot be resolved, fall back to system search.
                    if (!File.Exists(xmlFilePath))
                        xmlFilePath = Path.GetFullPath(xmlFileName);

                    if (File.Exists(xmlFilePath))
                        return xmlFilePath;
                }

                var message = "Target file cannot be found!";
                throw new FileNotFoundException(message, xmlFileName);
            }

            internal static OpenFileCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                string xmlFilePath = TryFindFile(helper.ReadString("XmlFilePath"), element.OwnerDocument.BaseURI);
                return new OpenFileCommand(xmlFilePath);
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal string FilePath { get; private set; }
            internal bool ForceManualExecutionMode { get; private set; }
            private DynamoModel dynamoModel;

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                this.dynamoModel = dynamoModel;
                dynamoModel.OpenFileImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("XmlFilePath", FilePath);
            }

            internal override void TrackAnalytics()
            {
                // Log file open action and the number of nodes in the opened workspace
                Dynamo.Logging.Analytics.TrackFileOperationEvent(
                    FilePath,
                    Logging.Actions.Open,
                    dynamoModel.CurrentWorkspace.Nodes.Count());

                // If there are unresolved nodes in the opened workspace, log the node names and count
                var unresolvedNodes = dynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>();
                if (unresolvedNodes != null && unresolvedNodes.Any())
                {
                    Dynamo.Logging.Analytics.TrackEvent(
                        Logging.Actions.Unresolved,
                        Logging.Categories.NodeOperations,
                        unresolvedNodes.Select(n => string.Format("{0}:{1}", n.LegacyAssembly, n.LegacyFullName))
                            .Aggregate((x, y) => string.Format("{0}, {1}", x, y)),
                        unresolvedNodes.Count());
                }
            }

            #endregion
        }

        /// <summary>
        /// A command used to execute or cancel execution.
        /// </summary>
        [DataContract]
        public class RunCancelCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="showErrors">Should errors be shown?</param>
            /// <param name="cancelRun">True to cancel execution. False to execute.</param>
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

            [DataMember]
            internal bool ShowErrors { get; private set; }

            [DataMember]
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

        /// <summary>
        /// A command used to force cancellation of execution.
        /// </summary>
        [DataContract]
        public class ForceRunCancelCommand : RunCancelCommand
        {
            /// <summary>
            ///
            /// </summary>
            /// <param name="showErrors">Should errors be shown?</param>
            /// <param name="cancelRun">True to cancel execution. False to execute.</param>
            public ForceRunCancelCommand(bool showErrors, bool cancelRun)
                : base(showErrors, cancelRun) { }

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.ForceRunCancelImpl(this);
            }
        }

        /// <summary>
        /// A command used to mutate commands during testing.
        /// </summary>
        public class MutateTestCommand : RecordableCommand
        {
            protected override void ExecuteCore(DynamoModel dynamoModel) { }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
            }
        }

        /// <summary>
        /// A command used to create a node.
        /// </summary>
        [DataContract]
        public class CreateNodeCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            private void SetProperties(double x, double y, bool defaultPosition, bool transformCoordinates)
            {
                X = x;
                Y = y;
                DefaultPosition = defaultPosition;
                TransformCoordinates = transformCoordinates;
            }

            /// <summary>
            /// </summary>
            /// <param name="node">The node.</param>
            /// <param name="x">The x location.</param>
            /// <param name="y">The y location.</param>
            /// <param name="defaultPosition"></param>
            /// <param name="transformCoordinates"></param>
            public CreateNodeCommand(
                NodeModel node, double x, double y, bool defaultPosition, bool transformCoordinates)
                : base(node != null ? new[] { node.GUID } : new[] { Guid.Empty })
            {
                Node = node;
                SetProperties(x, y, defaultPosition, transformCoordinates);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="node"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            /// <param name="transformCoordinates"></param>
            private CreateNodeCommand(
               XmlElement node, double x, double y, bool defaultPosition, bool transformCoordinates)
                : base(new[] { Guid.Empty })
            {
                NodeXml = node;
                SetProperties(x, y, defaultPosition, transformCoordinates);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="nodeName"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            /// <param name="transformCoordinates"></param>
            public CreateNodeCommand(IEnumerable<Guid> nodeId, string nodeName,
                double x, double y, bool defaultPosition, bool transformCoordinates)
                : base(nodeId)
            {
                Name = nodeName;
                SetProperties(x, y, defaultPosition, transformCoordinates);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="nodeName"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            /// <param name="transformCoordinates"></param>
            [JsonConstructor]
            public CreateNodeCommand(string nodeId, string nodeName,
                double x, double y, bool defaultPosition, bool transformCoordinates)
                : base(new[] { Guid.Parse(nodeId) })
            {
                Name = nodeName;
                SetProperties(x, y, defaultPosition, transformCoordinates);
            }

            internal static CreateNodeCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");
                bool defaultPos = helper.ReadBoolean("DefaultPosition");
                bool transformCoords = helper.ReadBoolean("TransformCoordinates");

                var nodeElement = element.ChildNodes.OfType<XmlElement>().FirstOrDefault(el => el.Name != "ModelGuid");

                if (nodeElement == null)
                {
                    // Get the old NodeId and NodeName attributes
                    var nodeId = DeserializeGuid(element, helper);
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


            [DataMember]
            internal double X { get; private set; }

            [DataMember]
            internal double Y { get; private set; }

            [DataMember]
            internal bool DefaultPosition { get; private set; }

            [DataMember]
            internal bool TransformCoordinates { get; private set; }

            [DataMember]
            //Legacy properties
            public string Name { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateNodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
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
                    helper.SetAttribute("NodeName", Name);
                }
            }

            internal override void TrackAnalytics()
            {
                Dynamo.Logging.Analytics.TrackEvent(
                    Logging.Actions.Create,
                    Logging.Categories.NodeOperations,
                    (Node != null) ? Node.GetOriginalName() : Name ?? "");
            }

            #endregion
        }

        /// <summary>
        /// A command used to create a new upstream/downstream node and connect
        /// it to an existing node in a single step
        /// </summary>
        [DataContract]
        public class CreateAndConnectNodeCommand : ModelBasedRecordableCommand
        {

            #region Public Class Methods

            /// <summary>
            /// Creates a new CreateAndConnectNodeCommand with the given inputs
            /// </summary>
            /// <param name="newNodeGuid"></param>
            /// <param name="existingNodeGuid"></param>
            /// <param name="newNodeName">The name of node to create</param>
            /// <param name="outPortIndex"></param>
            /// <param name="inPortIndex"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="createAsDownstreamNode">
            /// new node to be created as downstream or upstream node wrt the existing node
            /// </param>
            /// <param name="addNewNodeToSelection">select the new node after it is created by default</param>
            public CreateAndConnectNodeCommand(Guid newNodeGuid, Guid existingNodeGuid, string newNodeName, int outPortIndex, int inPortIndex,
                double x, double y, bool createAsDownstreamNode, bool addNewNodeToSelection)
                : base(new[] { newNodeGuid, existingNodeGuid })
            {
                NewNodeName = newNodeName;
                OutputPortIndex = outPortIndex;
                InputPortIndex = inPortIndex;
                X = x;
                Y = y;

                CreateAsDownstreamNode = createAsDownstreamNode;
                AddNewNodeToSelection = addNewNodeToSelection;
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            public string NewNodeName { get; private set; }

            [DataMember]
            internal int OutputPortIndex { get; private set; }

            [DataMember]
            internal int InputPortIndex { get; private set; }

            [DataMember]
            internal double X { get; private set; }

            [DataMember]
            internal double Y { get; private set; }

            [DataMember]
            internal bool CreateAsDownstreamNode { get; private set; }

            [DataMember]
            internal bool AddNewNodeToSelection { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateAndConnectNodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);

                var helper = new XmlElementHelper(element);
                helper.SetAttribute("NewNodeName", NewNodeName);
                helper.SetAttribute("X", X);
                helper.SetAttribute("Y", Y);
                helper.SetAttribute("CreateAsDownstreamNode", CreateAsDownstreamNode);
                helper.SetAttribute("AddNewNodeToSelection", AddNewNodeToSelection);

                helper.SetAttribute("OutPortIndex", OutputPortIndex);
                helper.SetAttribute("InPortIndex", InputPortIndex);
            }

            #endregion

            internal static CreateAndConnectNodeCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                string newNodeName = helper.ReadString("NewNodeName");
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");
                bool createAsDownstreamNode = helper.ReadBoolean("CreateAsDownstreamNode");
                bool addNewNodeToSelection = helper.ReadBoolean("AddNewNodeToSelection");

                var guids = DeserializeGuid(element, helper).ToList();
                var newNodeGuid = guids.ElementAt(0);
                var existingNodeGuid = guids.ElementAt(1);

                int outPortIndex = helper.ReadInteger("OutPortIndex");
                int inPortIndex = helper.ReadInteger("InPortIndex");

                return new CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid, newNodeName, outPortIndex, inPortIndex,
                    x, y, createAsDownstreamNode, addNewNodeToSelection);
            }

        }

        /// <summary>
        /// A command containing additional information needed for creating a proxy custom node.
        /// </summary>
        [DataContract]
        public class CreateProxyNodeCommand : CreateNodeCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="customnodeFunctionId"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            /// <param name="transformCoordinates"></param>
            /// <param name="name"></param>
            /// <param name="inputs"></param>
            /// <param name="outputs"></param>
            [JsonConstructor]
            public CreateProxyNodeCommand(string nodeId, string customnodeFunctionId,
                double x, double y,
                bool defaultPosition, bool transformCoordinates,
                string name, int inputs, int outputs)
                : base(nodeId, customnodeFunctionId, x, y, defaultPosition, transformCoordinates)
            {
                this.NickName = name;
                this.Inputs = inputs;
                this.Outputs = outputs;
            }

            internal static CreateProxyNodeCommand DeserializeCore(XmlElement element)
            {
                var baseCommand = CreateNodeCommand.DeserializeCore(element);
                var helper = new XmlElementHelper(element);
                string nickName = helper.ReadString("NickName");
                int inputs = helper.ReadInteger("Inputs");
                int outputs = helper.ReadInteger("Outputs");

                return new CreateProxyNodeCommand(
                    baseCommand.ModelGuids.First().ToString(),
                    baseCommand.Name,
                    baseCommand.X,
                    baseCommand.Y,
                    baseCommand.DefaultPosition,
                    baseCommand.TransformCoordinates,
                    nickName,
                    inputs,
                    outputs);
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal string NickName { get; private set; }

            [DataMember]
            internal int Inputs { get; private set; }

            [DataMember]
            internal int Outputs { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("NickName", NickName);
                helper.SetAttribute("Inputs", Inputs);
                helper.SetAttribute("Outputs", Outputs);
            }

            #endregion
        }

        /// <summary>
        /// A command to create a note.
        /// </summary>
        [DataContract]
        public class CreateNoteCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            private void SetProperties(string noteText,
                double x, double y, bool defaultPosition)
            {
                if (string.IsNullOrEmpty(noteText))
                    noteText = string.Empty;

                NoteText = noteText;
                X = x;
                Y = y;
                DefaultPosition = defaultPosition;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="noteText"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            public CreateNoteCommand(Guid nodeId, string noteText,
                double x, double y, bool defaultPosition)
                : base(new[] { nodeId })
            {
                SetProperties(noteText, x, y, defaultPosition);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeIds"></param>
            /// <param name="noteText"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            public CreateNoteCommand(IEnumerable<Guid> nodeIds, string noteText,
                double x, double y, bool defaultPosition)
                : base(nodeIds)
            {
                SetProperties(noteText, x, y, defaultPosition);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="noteText"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            [JsonConstructor]
            public CreateNoteCommand(string nodeId, string noteText,
                double x, double y, bool defaultPosition)
                : base(new[] { Guid.Parse(nodeId) })
            {
                SetProperties(noteText, x, y, defaultPosition);
            }

            internal static CreateNoteCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var nodeId = DeserializeGuid(element, helper);
                string noteText = helper.ReadString("NoteText");
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");

                return new CreateNoteCommand(nodeId, noteText, x, y,
                    helper.ReadBoolean("DefaultPosition"));
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal string NoteText { get; private set; }

            [DataMember]
            internal double X { get; private set; }

            [DataMember]
            internal double Y { get; private set; }

            [DataMember]
            internal bool DefaultPosition { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateNoteImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("NoteText", NoteText);
                helper.SetAttribute("X", X);
                helper.SetAttribute("Y", Y);
                helper.SetAttribute("DefaultPosition", DefaultPosition);
            }

            #endregion
        }

        /// <summary>
        /// A command to select a model object.
        /// </summary>
        [DataContract]
        public class SelectModelCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            /// <param name="modifiers"></param>
            [JsonConstructor]
            public SelectModelCommand(string modelGuid, ModifierKeys modifiers)
                : base(new[] { Guid.Parse(modelGuid) })
            {
                Modifiers = modifiers;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            /// <param name="modifiers"></param>
            public SelectModelCommand(Guid modelGuid, ModifierKeys modifiers)
                : base(new[] { modelGuid })
            {
                Modifiers = modifiers;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuids"></param>
            /// <param name="modifiers"></param>
            public SelectModelCommand(IEnumerable<Guid> modelGuids, ModifierKeys modifiers)
                : base(modelGuids)
            {
                Modifiers = modifiers;
            }

            internal static SelectModelCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var modifiers = ((ModifierKeys)helper.ReadInteger("Modifiers"));

                var modelGuids = DeserializeGuid(element, helper);

                return new SelectModelCommand(modelGuids, modifiers);
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal ModifierKeys Modifiers { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.SelectModelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("Modifiers", ((int)Modifiers));
            }

            #endregion
        }

        /// <summary>
        /// A command to select model objects within a region.
        /// </summary>
        [DataContract]
        public class SelectInRegionCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="region"></param>
            /// <param name="isCrossSelection"></param>
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

            [DataMember]
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

        /// <summary>
        /// A command to begin or end dragging a selection.
        /// </summary>
        [DataContract]
        public class DragSelectionCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            /// The operation to perform.
            /// </summary>
            public enum Operation
            {
                /// <summary>
                /// Begin dragging.
                /// </summary>
                BeginDrag,
                /// <summary>
                /// End dragging.
                /// </summary>
                EndDrag
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="mouseCursor"></param>
            /// <param name="operation"></param>
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

            [DataMember]
            internal Operation DragOperation { get; private set; }

            [DataMember]
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

        /// <summary>
        /// A command used to create a connection.
        /// </summary>
        [DataContract]
        public class MakeConnectionCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            /// The connection mode.
            /// </summary>
            public enum Mode
            {
                /// <summary>
                /// Begin connection.
                /// </summary>
                Begin,
                /// <summary>
                /// End connection.
                /// </summary>
                End,
                /// <summary>
                /// Begin shift reconnections.
                /// </summary>
                BeginShiftReconnections,
                /// <summary>
                /// End shift reconnections.
                /// </summary>
                EndShiftReconnections,
                /// <summary>
                /// End and start control connections.
                /// </summary>
                [Obsolete("Replaced with BeginDuplicateConnection")]
                EndAndStartCtrlConnection,
                /// <summary>
                /// Cancel connection.
                /// </summary>
                Cancel,
                /// <summary>
                /// Begin duplicate connection.
                /// </summary>
                BeginDuplicateConnection,
                /// <summary>
                /// End current connection and create new connections.
                /// </summary>
                BeginCreateConnections
            }

            void setProperties(int portIndex, PortType portType, Mode mode)
            {
                PortIndex = portIndex;
                Type = portType;
                ConnectionMode = mode;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="portIndex"></param>
            /// <param name="portType"></param>
            /// <param name="mode"></param>
            [JsonConstructor]
            public MakeConnectionCommand(string nodeId, int portIndex, PortType portType, Mode mode)
                : base(new[] { Guid.Parse(nodeId) })
            {
                setProperties(portIndex, portType, mode);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="portIndex"></param>
            /// <param name="portType"></param>
            /// <param name="mode"></param>
            public MakeConnectionCommand(Guid nodeId, int portIndex, PortType portType, Mode mode)
                : base(new[] { nodeId })
            {
                setProperties(portIndex, portType, mode);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="portIndex"></param>
            /// <param name="portType"></param>
            /// <param name="mode"></param>
            public MakeConnectionCommand(IEnumerable<Guid> nodeId, int portIndex, PortType portType, Mode mode)
                : base(nodeId)
            {
                setProperties(portIndex, portType, mode);
            }

            internal static MakeConnectionCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                int portIndex = helper.ReadInteger("PortIndex");
                var portType = ((PortType)helper.ReadInteger("Type"));
                var mode = ((Mode)helper.ReadInteger("ConnectionMode"));

                var modelGuids = DeserializeGuid(element, helper);

                return new MakeConnectionCommand(modelGuids, portIndex, portType, mode);
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            public int PortIndex { get; private set; }

            [DataMember]
            internal PortType Type { get; private set; }

            [DataMember]
            public Mode ConnectionMode { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.MakeConnectionImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("PortIndex", PortIndex);
                helper.SetAttribute("Type", ((int)Type));
                helper.SetAttribute("ConnectionMode", ((int)ConnectionMode));
            }

            #endregion
        }

        /// <summary>
        /// A command used to delete a model object.
        /// </summary>
        [DataContract]
        public class DeleteModelCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            [JsonConstructor]
            public DeleteModelCommand(string modelGuid) : base(new[] { Guid.Parse(modelGuid) }) { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            public DeleteModelCommand(Guid modelGuid) : base(new[] { modelGuid }) { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuids"></param>
            public DeleteModelCommand(IEnumerable<Guid> modelGuids) : base(modelGuids) { }

            internal static DeleteModelCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var modelGuids = DeserializeGuid(element, helper);
                return new DeleteModelCommand(modelGuids);
            }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.DeleteModelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
            }

            #endregion
        }

        /// <summary>
        /// A command used to trigger an undo or redo operation.
        /// </summary>
        [DataContract]
        public class UndoRedoCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            /// The operation to perform.
            /// </summary>
            public enum Operation
            {
                /// <summary>
                /// Undo.
                /// </summary>
                Undo,
                /// <summary>
                /// Redo.
                /// </summary>
                Redo
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="operation">The operation to perform.</param>
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

            [DataMember]
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

            internal override void TrackAnalytics()
            {
                Dynamo.Logging.Analytics.TrackCommandEvent(
                    CmdOperation.ToString()); // "Undo" or "Redo"
            }

            #endregion
        }

        /// <summary>
        /// A command used
        /// </summary>
        [DataContract]
        public class ModelEventCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            [JsonConstructor]
            public ModelEventCommand(string modelGuid, string eventName, int value)
                : base(new[] { Guid.Parse(modelGuid) })
            {
                EventName = eventName;
                Value = value;
            }

         
            public ModelEventCommand(string modelGuid, string eventName)
                : base(new[] { Guid.Parse(modelGuid) })
            {
                EventName = eventName;
                Value = 1;
            }

            public ModelEventCommand(Guid modelGuid, string eventName, int value)
                : base(new[] { modelGuid })
            {
                EventName = eventName;
                Value = value;
            }
            public ModelEventCommand(Guid modelGuid, string eventName)
               : base(new[] { modelGuid })
            {
                EventName = eventName;
                Value = 1;
            }

            public ModelEventCommand(IEnumerable<Guid> modelGuid, string eventName, int value)
                : base(modelGuid)
            {
                EventName = eventName;
                Value = value;
            }

            public ModelEventCommand(IEnumerable<Guid> modelGuid, string eventName)
               : base(modelGuid)
            {
                EventName = eventName;
                Value = 1;
            }

            internal static ModelEventCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var modelGuids = DeserializeGuid(element, helper);
                string eventName = helper.ReadString("EventName");
                int value = helper.ReadInteger("Value", 1);
                return new ModelEventCommand(modelGuids, eventName, value);
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            public string EventName { get; private set; }

            [DataMember]
            public int Value { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.SendModelEventImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("EventName", EventName);
                helper.SetAttribute("Value", Value);
            }

            #endregion
        }

        /// <summary>
        /// A command used to update the value of a property on a model object.
        /// </summary>
        [DataContract]
        public class UpdateModelValueCommand : ModelBasedRecordableCommand
        {
            private readonly List<Guid> modelGuids;

            #region Public Class Methods

            /// <summary>
            /// </summary>
            /// <param name="workspaceGuid">Guid of the target workspace. Guid.Empty means current workspace</param>
            /// <param name="modelGuid">Guid of node model</param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public UpdateModelValueCommand(Guid workspaceGuid, Guid modelGuid, string name, string value)
                : this(workspaceGuid, new[] { modelGuid }, name, value) { }

            /// <summary>
            /// </summary>
            /// <param name="modelGuid"></param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            [JsonConstructor]
            public UpdateModelValueCommand(string modelGuid, string name, string value)
                : base(new[] { Guid.Parse(modelGuid) })
            {
                this.modelGuids = new List<Guid> { Guid.Parse(modelGuid) };
                Name = name;
                Value = value;
            }

            /// <summary>
            /// </summary>
            /// <param name="modelGuid"></param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public UpdateModelValueCommand(Guid modelGuid, string name, string value)
                : this(Guid.Empty, new[] { modelGuid }, name, value) { }

            /// <summary>
            /// </summary>
            /// <param name="modelGuid"></param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public UpdateModelValueCommand(IEnumerable<Guid> modelGuid, string name, string value)
                : this(Guid.Empty, modelGuid, name, value) { }

            /// <summary>
            /// </summary>
            /// <param name="workspaceGuid">Guid of the target workspace. Guid.Empty means current workspace</param>
            /// <param name="modelGuid"></param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public UpdateModelValueCommand(Guid workspaceGuid, IEnumerable<Guid> modelGuid, string name, string value)
                : base(modelGuid != null && modelGuid.Any() ? modelGuid : new[] { Guid.Empty })
            {
                this.modelGuids = modelGuid.ToList();
                WorkspaceGuid = workspaceGuid;
                Name = name;
                Value = value;
            }

            internal static UpdateModelValueCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                string name = helper.ReadString("Name");
                string value = helper.ReadString("Value");

                Guid workspaceGuid = helper.ReadGuid("WorkspaceGuid", Guid.Empty);

                // TODO: once recordable framework serialize workspace's GUID,
                // we should use the GUID that read from file instead of using
                // empty GUID. Empty GUID means the command will be executed on
                // the current workspace, but it may not be the desired target
                // workspace.
                workspaceGuid = Guid.Empty;

                var modelGuids = DeserializeGuid(element, helper);

                return new UpdateModelValueCommand(workspaceGuid, modelGuids, name, value);
            }

            #endregion

            #region Public Command Properties

            /// <summary>
            /// A collection of <see cref="Guid"/> which identify the model objects to update.
            /// </summary>
            public IEnumerable<Guid> ModelGuids { get { return modelGuids; } }

            /// <summary>
            /// The name of the property to update.
            /// </summary>
            [DataMember]
            public string Name { get; private set; }

            /// <summary>
            /// The new value to apply to the property.
            /// </summary>
            [DataMember]
            public string Value { get; private set; }

            internal Guid WorkspaceGuid { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.UpdateModelValueImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("Name", Name);
                helper.SetAttribute("Value", Value);
                helper.SetAttribute("WorkspaceGuid", WorkspaceGuid.ToString());
            }

            public override string ToString()
            {
                // This method will be removed if no one is referencing it.
                throw new NotImplementedException("UpdateModelValueCommand.ToString");
            }

            #endregion
        }

        /// <summary>
        /// A command to convert nodes to code.
        /// </summary>
        [DataContract]
        public class ConvertNodesToCodeCommand : RecordableCommand
        {
            #region Public Class Methods

            [JsonConstructor]
            internal ConvertNodesToCodeCommand() { }

            internal static ConvertNodesToCodeCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                return new ConvertNodesToCodeCommand();
            }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.ConvertNodesToCodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
            }

            #endregion
        }

        /// <summary>
        /// A command to create a custom node.
        /// </summary>
        [DataContract]
        public class CreateCustomNodeCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            private void SetProperties(string name,
                string category, string description, bool makeCurrent)
            {
                Name = name;
                Category = category;
                Description = description;
                MakeCurrent = makeCurrent;
            }

            /// <summary>
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="name"></param>
            /// <param name="category"></param>
            /// <param name="description"></param>
            /// <param name="makeCurrent"></param>
            [JsonConstructor]
            public CreateCustomNodeCommand(string nodeId, string name,
                string category, string description, bool makeCurrent)
                : base(new[] { Guid.Parse(nodeId) })
            {
                SetProperties(name, category, description, makeCurrent);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="name"></param>
            /// <param name="category"></param>
            /// <param name="description"></param>
            /// <param name="makeCurrent"></param>
            public CreateCustomNodeCommand(Guid nodeId, string name,
                string category, string description, bool makeCurrent)
                : base(new[] { nodeId })
            {
                SetProperties(name, category, description, makeCurrent);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeId"></param>
            /// <param name="name"></param>
            /// <param name="category"></param>
            /// <param name="description"></param>
            /// <param name="makeCurrent"></param>
            public CreateCustomNodeCommand(IEnumerable<Guid> nodeId, string name,
                string category, string description, bool makeCurrent)
                : base(nodeId)
            {
                SetProperties(name, category, description, makeCurrent);
            }

            internal static CreateCustomNodeCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var modelGuids = DeserializeGuid(element, helper);

                return new CreateCustomNodeCommand(
                    modelGuids,
                    helper.ReadString("Name"),
                    helper.ReadString("Category"),
                    helper.ReadString("Description"),
                    helper.ReadBoolean("MakeCurrent"));
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal string Name { get; private set; }

            [DataMember]
            internal string Category { get; private set; }

            [DataMember]
            internal string Description { get; private set; }

            [DataMember]
            internal bool MakeCurrent { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateCustomNodeImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("Name", Name);
                helper.SetAttribute("Category", Category);
                helper.SetAttribute("Description", Description);
                helper.SetAttribute("MakeCurrent", MakeCurrent);
            }

            #endregion
        }

        /// <summary>
        /// A command to switch tabs.
        /// </summary>
        [DataContract]
        public class SwitchTabCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelIndex"></param>
            [JsonConstructor]
            public SwitchTabCommand(int modelIndex)
            {
                WorkspaceModelIndex = modelIndex;
            }

            internal static SwitchTabCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                return new SwitchTabCommand(helper.ReadInteger("TabIndex"));
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal int WorkspaceModelIndex { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.SwitchWorkspaceImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("TabIndex", WorkspaceModelIndex);
            }

            #endregion
        }

        /// <summary>
        /// A command to create a group.
        /// </summary>
        public class CreateAnnotationCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="annotationId"></param>
            /// <param name="annotationText"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            public CreateAnnotationCommand(Guid annotationId, string annotationText,
                double x, double y, bool defaultPosition)
                : base(new List<Guid> { annotationId })
            {
                if (string.IsNullOrEmpty(annotationText))
                    annotationText = Resources.GroupDefaultText;

                AnnotationText = annotationText;
                X = x;
                Y = y;
                DefaultPosition = defaultPosition;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="annotationId"></param>
            /// <param name="annotationText"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="defaultPosition"></param>
            public CreateAnnotationCommand(IEnumerable<Guid> annotationId, string annotationText,
                double x, double y, bool defaultPosition)
                : base(annotationId)
            {
                if (string.IsNullOrEmpty(annotationText))
                    annotationText = Resources.GroupDefaultText;

                AnnotationText = annotationText;
                X = x;
                Y = y;
                DefaultPosition = defaultPosition;
            }

            internal static CreateAnnotationCommand DeserializeCore(XmlElement element)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                var modelGuids = DeserializeGuid(element, helper);
                string annotationText = helper.ReadString("AnnotationText");
                double x = helper.ReadDouble("X");
                double y = helper.ReadDouble("Y");

                return new CreateAnnotationCommand(modelGuids, annotationText, x, y,
                    helper.ReadBoolean("DefaultPosition"));
            }

            #endregion

            #region Public Command Properties

            internal string AnnotationText { get; private set; }
            internal double X { get; private set; }
            internal double Y { get; private set; }
            internal bool DefaultPosition { get; private set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreateAnnotationImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("AnnotationText", AnnotationText);
                helper.SetAttribute("X", X);
                helper.SetAttribute("Y", Y);
                helper.SetAttribute("DefaultPosition", DefaultPosition);
            }

            #endregion
        }

        /// <summary>
        /// A command to ungroup model objects.
        /// </summary>
        [DataContract]
        public class UngroupModelCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            [JsonConstructor]
            public UngroupModelCommand(string modelGuid) : base(new[] { Guid.Parse(modelGuid) }) { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            public UngroupModelCommand(Guid modelGuid) : base(new[] { modelGuid }) { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            public UngroupModelCommand(IEnumerable<Guid> modelGuid) : base(modelGuid) { }

            internal static UngroupModelCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var modelGuids = DeserializeGuid(element, helper);
                return new UngroupModelCommand(modelGuids);
            }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.UngroupModelImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
            }

            #endregion
        }

        /// <summary>
        /// A command to add a model object to a group.
        /// </summary>
        [DataContract]
        public class AddModelToGroupCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            [JsonConstructor]
            public AddModelToGroupCommand(string modelGuid) : base(new[] { Guid.Parse(modelGuid) }) { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            public AddModelToGroupCommand(Guid modelGuid) : base(new[] { modelGuid }) { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="modelGuid"></param>
            public AddModelToGroupCommand(IEnumerable<Guid> modelGuid) : base(modelGuid) { }

            internal static AddModelToGroupCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var modelGuids = DeserializeGuid(element, helper);
                return new AddModelToGroupCommand(modelGuids);
            }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.AddToGroupImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
            }

            #endregion
        }

        /// <summary>
        /// A command to add a preset.
        /// </summary>
        [DataContract]
        public class AddPresetCommand : ModelBasedRecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="name"></param>
            /// <param name="description"></param>
            /// <param name="currentSelectionIds"></param>
            public AddPresetCommand(string name, string description, IEnumerable<Guid> currentSelectionIds )
                :base(currentSelectionIds)
            {
                PresetStateName = name;
                PresetStateDescription = description;
            }

            internal static AddPresetCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                var modelGuids = DeserializeGuid(element, helper);
                if (modelGuids.Count()<1)
                {
                    throw new ArgumentNullException("No IDs were deserialized during load of preset creation command");
                }

                return new AddPresetCommand(helper.ReadString("name"), helper.ReadString("description"),modelGuids);
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal string PresetStateName { get; set; }
            [DataMember]
            internal string PresetStateDescription { get; set; }
             #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.CreatePresetStateImpl(this);

            }

            protected override void SerializeCore(XmlElement element)
            {
                base.SerializeCore(element);
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("name", PresetStateName);
                helper.SetAttribute("description", PresetStateDescription);

            }
              #endregion
        }

        /// <summary>
        /// A command to apply a preset.
        /// </summary>
        [DataContract]
        public class ApplyPresetCommand : RecordableCommand
        {
            #region Public Class Methods

            /// <summary>
            ///
            /// </summary>
            /// <param name="workspaceID"></param>
            /// <param name="stateID"></param>
            public ApplyPresetCommand(Guid workspaceID, Guid stateID)
            {
                StateID = stateID;
                WorkSpaceID = workspaceID;
            }

            internal static ApplyPresetCommand DeserializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);

                return new ApplyPresetCommand(helper.ReadGuid("WorkspaceID"), helper.ReadGuid("StateID"));
            }

            #endregion

            #region Public Command Properties

            [DataMember]
            internal Guid StateID { get; set; }
            [DataMember]
            internal Guid WorkSpaceID { get; set; }

            #endregion

            #region Protected Overridable Methods

            protected override void ExecuteCore(DynamoModel dynamoModel)
            {
                dynamoModel.SetWorkSpaceToStateImpl(this);
            }

            protected override void SerializeCore(XmlElement element)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("WorkspaceID", WorkSpaceID.ToString());
                helper.SetAttribute("StateID", StateID.ToString());

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
