using System;
using System.Linq;
using System.Xml;

using Dynamo.Models;

namespace DSCoreNodesUI
{
    public abstract class BasicInteractive<T> : NodeModel
    {
        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (Equals(_value, null) || !_value.Equals(value))
                {
                    _value = value;
                    RequiresRecalc = !Equals(value, null);
                    RaisePropertyChanged("Value");
                }
            }
        }

        // Making these abstract so that derived classes are forced to come up 
        // with their implementations rather than default silently taking over.
        protected abstract T DeserializeValue(string val);
        protected abstract string SerializeValue();

        protected BasicInteractive(WorkspaceModel workspace)
            : base(workspace)
        {
            Type type = typeof(T);
            OutPortData.Add(new PortData("", type.Name));
            RegisterAllPorts();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.InnerText = SerializeValue();
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (
                XmlNode subNode in
                    nodeElement.ChildNodes.Cast<XmlNode>()
                               .Where(subNode => subNode.Name.Equals(typeof(T).FullName)))
            {
// ReSharper disable once PossibleNullReferenceException
                Value = DeserializeValue(subNode.InnerText);
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called

            if (context == SaveContext.Undo)
            {
                var xmlDocument = element.OwnerDocument;
                var subNode = xmlDocument.CreateElement(typeof(T).FullName);
                subNode.InnerText = SerializeValue();
                element.AppendChild(subNode);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); // Base implementation must be called

            if (context == SaveContext.Undo)
            {
                foreach (XmlNode subNode in element.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals(typeof(T).FullName)))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Value = DeserializeValue(subNode.InnerText);
                }
            }
        }

        #endregion
    }
}
