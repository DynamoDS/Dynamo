using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Core.Automation
{
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

        internal CreateNodeCommand(Guid nodeId, string nodeName,
            double x, double y, bool transformCoordinates)
        {
            this.NodeId = nodeId;
            this.NodeName = nodeName;
            this.X = x;
            this.Y = y;
            this.TransformCoordinates = transformCoordinates;
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
            string nodeName = helper.ReadString("NodeName");
            double x = helper.ReadDouble("X");
            double y = helper.ReadDouble("Y");
            bool transform = helper.ReadBoolean("TransformCoordinates");
            return new CreateNodeCommand(nodeId, nodeName, x, y, transform);
        }

        internal Guid NodeId { get; private set; }
        internal string NodeName { get; private set; }
        internal double X { get; private set; }
        internal double Y { get; private set; }
        internal bool TransformCoordinates { get; private set; }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore(WorkspaceModel workspace)
        {
            workspace.CreateNodeImpl(this);
        }

        protected override void SerializeCore(XmlElement element)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("NodeId", this.NodeId);
            helper.SetAttribute("NodeName", this.NodeName);
            helper.SetAttribute("X", this.X);
            helper.SetAttribute("Y", this.Y);
            helper.SetAttribute("TransformCoordinates", TransformCoordinates);
        }

        #endregion
    }
}
