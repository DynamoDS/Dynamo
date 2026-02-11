using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace CoreNodeModels.Input
{
    public abstract class BasicInteractive<T> : NodeModel
    {
        private T value;

        [JsonProperty("InputValue", Order = 9)]
        public virtual T Value
        {
            get
            {
                return value;
            }
            set
            {
                if (Equals(this.value, null) || !this.value.Equals(value))
                {
                    this.value = value;
                    if (!Equals(value, null))
                    {
                        OnNodeModified();
                    }
                    else
                    {
                        ClearDirtyFlag();
                    }
                    RaisePropertyChanged("Value");
                }               
            }
        }
        public override NodeInputData InputData
        {
           
            get
            {
                return new NodeInputData()
                {
                    Id = this.GUID,
                    Name = this.Name,
                    //use the <T> type to convert to the correct nodeTypeString defined by
                    //the schema
                    Type = NodeInputData.GetNodeInputTypeFromType(typeof(T)),
                    Type2 = NodeInputData.GetNodeInputTypeFromType(typeof(T)),
                    Description = this.Description,
                    Value = Value.ToString(),
                };
            }
        }

        // Making these abstract so that derived classes are forced to come up 
        // with their implementations rather than default silently taking over.
        protected abstract T DeserializeValue(string val);
        protected abstract string SerializeValue();

        protected BasicInteractive(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        protected BasicInteractive()
        {
            Type type = typeof(T);
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", type.Name)));
            RegisterAllPorts();
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called

            var xmlDocument = element.OwnerDocument;
            var subNode = xmlDocument.CreateElement(typeof(T).FullName);
            subNode.InnerText = SerializeValue();
            element.AppendChild(subNode);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context); // Base implementation must be called

            var subNodes = nodeElement.ChildNodes.Cast<XmlNode>()
                .Where(subNode => subNode.Name.Equals(typeof(T).FullName));

            foreach (XmlNode subNode in subNodes)
            {
                var attrs = subNode.Attributes;

                Value = attrs != null && attrs["value"] != null
                    ? DeserializeValue(attrs["value"].Value) //Legacy behavior
                    : DeserializeValue(subNode.InnerText);
            }
        }

        #endregion
    }
}
