using System;
using System.Linq;
using System.Xml;

using Dynamo.Models;

namespace DSCoreNodesUI
{
    public abstract class BasicInteractive<T> : NodeModel
    {
        private T value;
        public T Value
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
                        ForceReExecuteOfNode = true;
                        OnAstUpdated();
                    }
                    RaisePropertyChanged("Value");
                }
            }
        }

        // Making these abstract so that derived classes are forced to come up 
        // with their implementations rather than default silently taking over.
        protected abstract T DeserializeValue(string val);
        protected abstract string SerializeValue();

        protected BasicInteractive()
        {
            Type type = typeof(T);
            OutPortData.Add(new PortData("", type.Name));
            RegisterAllPorts();
        }

        public override string PrintExpression()
        {
            return Value.ToString();
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

            foreach (XmlNode subNode in nodeElement.ChildNodes.Cast<XmlNode>()
                .Where(subNode => subNode.Name.Equals(typeof(T).FullName)))
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
