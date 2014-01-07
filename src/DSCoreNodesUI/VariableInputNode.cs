using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.Utilities;

namespace DSCoreNodesUI
{
    public abstract class VariableInputNode : NodeModel, IWpfNode
    {
        public void SetupCustomUIElements(dynNodeView view)
        {
            throw new NotImplementedException();
        }

        protected abstract string GetInputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        //TODO: Mark node as dirty if amount of inputs changes between runs

        protected virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
            }
        }

        protected virtual void AddInput()
        {
            var idx = GetInputNameIndex();
            InPortData.Add(
                new PortData(GetInputRootName() + idx, GetTooltipRootName() + idx, typeof(object)));
        }

        #region Load/Save

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("inputcount", InPortData.Count.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            int i = InPortData.Count;
            int amt = Convert.ToInt32(nodeElement.Attributes["inputcount"].Value);

            if (i > amt)
            {
                for (int j = 0; j < i - amt; j++)
                    RemoveInput();
            }
            else
            {
                for (int j = 0; j < amt - i; j++)
                    AddInput();
            }

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
    }
}
