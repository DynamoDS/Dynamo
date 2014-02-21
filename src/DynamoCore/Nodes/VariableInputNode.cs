using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    public abstract class VariableInputNode : NodeModel, IWpfNode
    {
        public virtual void SetupCustomUIElements(dynNodeView view)
        {
            var addButton = new DynamoNodeButton(this, "AddInPort") { Content = "+", Width = 20 };
            //addButton.Height = 20;

            var subButton = new DynamoNodeButton(this, "RemoveInPort") { Content = "-", Width = 20 };
            //subButton.Height = 20;

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            view.inputGrid.Children.Add(wp);
        }

        protected abstract string GetInputName(int index);
        protected abstract string GetInputTooltip(int index);

        /// <summary>
        /// Fetches the index number to use for the next port.
        /// </summary>
        protected virtual int GetInputIndex()
        {
            return InPortData.Count;
        }

        //TODO: Mark node as dirty if amount of inputs changes between runs

        /// <summary>
        /// Removes an input from this node. Called when the '-' button is clicked.
        /// </summary>
        protected virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
            }
        }

        /// <summary>
        /// Adds an input to this node. Called when the '+' button is clicked.
        /// </summary>
        protected virtual void AddInput()
        {
            var idx = GetInputIndex();
            InPortData.Add(
                new PortData(GetInputName(idx), GetInputTooltip(idx), typeof(object)));
        }

        /// <summary>
        /// Set the number of inputs.  
        /// </summary>
        /// <param name="numInputs"></param>
        public void SetNumInputs(int numInputs)
        {
            if (numInputs <= 0 || numInputs == InPorts.Count)
            {
                return;
            }

            InPortData.Clear();

            for (var i = 0; i < numInputs; i++)
            {
                AddInput();
            }

            RegisterAllPorts();
        }

        #region Load/Save

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("inputcount", InPortData.Count.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            int amt = Convert.ToInt32(nodeElement.Attributes["inputcount"].Value);

            SetNumInputs(amt);

            RegisterAllPorts();
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            XmlDocument xmlDoc = element.OwnerDocument;
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");
                input.SetAttribute("name", inport.NickName);
                element.AppendChild(input);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                //Reads in the new number of ports required from the data stored in the Xml Element
                //during Serialize (nextLength). Changes the current In Port Data to match the
                //required size by adding or removing port data.
                int currLength = InPortData.Count;
                XmlNodeList inNodes = element.SelectNodes("Input");
                int nextLength = inNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = inNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        InPortData.Add(new PortData(nickName, "", typeof(object)));
                    }
                }
                else if (nextLength < currLength)
                    InPortData.RemoveRange(nextLength, currLength - nextLength);

                RegisterAllPorts();
            }
        }

        #endregion

        #region Undo/Redo

        private void RecordModels()
        {
            var connectors = InPorts[InPorts.Count - 1].Connectors;
            if (connectors.Count != 0)
            {
                if (connectors.Count != 1)
                {
                    throw new InvalidOperationException(
                        "There should be only one connection to an input port");
                }
                var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>
                {
                    { connectors[0], UndoRedoRecorder.UserAction.Deletion },
                    { this, UndoRedoRecorder.UserAction.Modification }
                };
                WorkSpace.RecordModelsForUndo(models);
            }
            else
                WorkSpace.RecordModelForModification(this);
        }

        protected override bool HandleModelEventCore(string eventName)
        {
            if (eventName == "AddInPort")
            {
                AddInput();
                RegisterAllPorts();
                return true; // Handled here.
            }

            if (eventName == "RemoveInPort")
            {
                // When an in-port is removed, it is possible that a connector 
                // is almost removed along with it. Both node modification and 
                // connector deletion have to be recorded as one action group.
                // But before HandleModelEventCore is called, node modification 
                // has already been recorded (in WorkspaceModel.SendModelEvent).
                // For that reason, that entry on the undo-stack needs to be 
                // popped (the node modification will be recorded here instead).
                // 
                WorkSpace.UndoRecorder.PopFromUndoGroup();

                RecordModels();
                RemoveInput();
                RegisterAllPorts();
                return true; // Handled here.
            }

            return base.HandleModelEventCore(eventName);
        }

        #endregion
    }
}
