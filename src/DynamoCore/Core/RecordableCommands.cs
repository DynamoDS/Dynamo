using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Utilities;

namespace Dynamo.Core
{
    partial class WorkspaceModel
    {
        private List<RecordableCommand> recordedCommands = null;

        internal void ExecuteCommand(RecordableCommand command)
        {
            // In the playback mode 'this.recordedCommands' will be 
            // 'null' so that the incoming command will not be recorded.
            if (null != recordedCommands)
                recordedCommands.Add(command);

            command.Execute(this); // Internally calls 'CreateNodeInternal'.
        }

        internal void CreateNodeInternal(CreateNodeCommand command)
        {
            System.Guid nodeId = command.NodeId;
            string nodeType = command.NodeType;

            // Existing codes that create the actual node...
        }
    }

    public class DynamoViewModel
    {
        // This can be either a DelegateCommand execution 
        // method, or an event handler of a UI component.
        internal void OnCreateNodeButtonClicked()
        {
            WorkspaceModel workspace = DynamoViewModel.CurrentSpace;

            System.Guid nodeId = Guid.NewGuid();
            string nodeType = "Dynamo.Nodes.Conditional";
            workspace.ExecuteCommand(new CreateNodeCommand(nodeId, nodeType));
        }

        private static WorkspaceModel CurrentSpace { get; set; }
    }

    /// <summary>
    /// This is the base class of all recordable commands. It provides the 
    /// contract between a UI event handler (e.g. delegate command method or 
    /// a button event handler) and the actual command handler in the 
    /// WorkspaceModel. It is mandatory for each RecordableCommand-derived 
    /// class to be serializable to/deserializable from an XmlElement.
    /// </summary>
    /// 
    internal abstract class RecordableCommand
    {
        #region Public Class Operational Methods

        /// <summary>
        /// Call this method to execute a RecordableCommand. A RecordableCommand 
        /// must be executed in the context of an existing WorkspaceModel.
        /// </summary>
        /// <param name="workspace">The workspace this RecordableCommand is 
        /// targeting.</param>
        /// 
        internal void Execute(WorkspaceModel workspace)
        {
            this.ExecuteCore(workspace);
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
            string commandName = this.GetType().ToString();
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
        /// FormatException.</returns>
        /// 
        internal static RecordableCommand Deserialize(XmlElement element)
        {
            switch (element.Name)
            {
                case "Dynamo.Core.CreateNodeCommand":
                    return CreateNodeCommand.Deserialize(element);
            }

            throw new ArgumentException("element");
        }

        #endregion

        #region Protected Overridable Methods

        /// <summary>
        /// Derived classes must implement this method to perform the actual
        /// command execution. A typical implementation of this method involves
        /// calling a corresponding method on WorkspaceModel by passing itself 
        /// as the only argument.
        /// </summary>
        /// <param name="workspace">The WorkspaceModel object on which this 
        /// command should be executed.</param>
        /// 
        protected abstract void ExecuteCore(WorkspaceModel workspace);

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

    internal class CreateNodeCommand : RecordableCommand
    {
        #region Public Class Methods and Properties

        internal CreateNodeCommand(Guid nodeId, string nodeType)
        {
            this.NodeId = nodeId;
            this.NodeType = nodeType;
        }

        /// <summary>
        /// Call this static method to reconstruct a RecordableCommand object 
        /// given an XmlElement previously saved with SerializeCore method.
        /// </summary>
        /// <param name="element">The XmlElement from which the RecordableCommand
        /// can be reconstructed.</param>
        /// <returns>Returns the reconstructed RecordableCommand object. If a 
        /// RecordableCommand cannot be reconstructed, this method throws a 
        /// FormatException.</returns>
        internal static CreateNodeCommand DeserializeCore(XmlElement element)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            System.Guid nodeId = helper.ReadGuid("NodeId");
            string nodeType = helper.ReadString("NodeType");
            return new CreateNodeCommand(nodeId, nodeType);
        }

        internal Guid NodeId { get; private set; }
        internal string NodeType { get; private set; }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore(WorkspaceModel workspace)
        {
            workspace.CreateNodeInternal(this);
        }

        protected override void SerializeCore(XmlElement element)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("NodeId", this.NodeId);
            helper.SetAttribute("NodeType", this.NodeType);
        }

        #endregion
    }
}
