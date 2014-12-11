using System;
using System.Globalization;
using System.Xml;
using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces integer values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider : DSCoreNodesUI.Integer
    {
        public IntegerSlider(WorkspaceModel workspace) : base(workspace)
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Value = 0;
        }

        private int _max;
        public int Max
        {
            get { return _max; }
            set
            {
                _max = value;

                if (_max < Value)
                    Value = _max;

                RaisePropertyChanged("Max");
            }
        }

        private int _min;
        public int Min
        {
            get { return _min; }
            set
            {
                _min = value;

                if (_min > Value)
                    Value = _min;

                RaisePropertyChanged("Min");
            }
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new IntegerDisplay();
            switch (name)
            {
                case "Value":
                    Value = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Max":
                    Max = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Min":
                    Min = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    if (Value >= Max)
                    {
                        this.Max = Value;
                    }
                    if (Value <= Min)
                    {
                        this.Min = Value;
                    }
                    return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }

        #region Load/Save

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);

            XmlElement outEl = xmlDoc.CreateElement("Range");
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (!subNode.Name.Equals("Range"))
                    continue;

                int min = Min;
                int max = Max;

                if (subNode.Attributes != null)
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("min"))
                            min = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("max"))
                            max = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("value"))
                            Value = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }
                }

                Min = min;
                Max = max;
            }
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            if (context == SaveContext.Undo)
            {
                var xmlDocument = element.OwnerDocument;
                XmlElement subNode = xmlDocument.CreateElement("Range");
                subNode.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
                subNode.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
                element.AppendChild(subNode);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); // Base implementation must be called.

            if (context == SaveContext.Undo)
            {
                foreach (XmlNode subNode in element.ChildNodes)
                {
                    if (!subNode.Name.Equals("Range"))
                        continue;
                    if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                        continue;

                    int min = Min;
                    int max = Max;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("min"))
                            min = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("max"))
                            max = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                    }

                    Min = min;
                    Max = max;
                }
            }
        }

        #endregion

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }
    }
}
