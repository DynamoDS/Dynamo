using System;
using System.Globalization;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName(/*NXLT*/"Double Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription(/*NXLT*/"DoubleSliderDescription", typeof(Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class DoubleSlider : DSCoreNodesUI.Double
    {
        public DoubleSlider(WorkspaceModel workspace) : base(workspace)
        {
            Value = 0;
            Min = 0;
            Max = 100;
        }

        private double _max;
        public double Max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (_max < Value)
                    Value = _max;
                RaisePropertyChanged(/*NXLT*/"Max");
            }
        }

        private double _min;
        public double Min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (_min > Value)
                    Value = _min;
                RaisePropertyChanged(/*NXLT*/"Min");
            }
        }

        #region Load/Save

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);

            XmlElement outEl = xmlDoc.CreateElement("Range");
            outEl.SetAttribute(/*NXLT*/"min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute(/*NXLT*/"max", Max.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (!subNode.Name.Equals(/*NXLT*/"Range"))
                    continue;

                double min = Min;
                double max = Max;

                if (subNode.Attributes != null)
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals(/*NXLT*/"min"))
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals(/*NXLT*/"max"))
                            max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals(/*NXLT*/"value"))
                            Value = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
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
                subNode.SetAttribute(/*NXLT*/"min", Min.ToString(CultureInfo.InvariantCulture));
                subNode.SetAttribute(/*NXLT*/"max", Max.ToString(CultureInfo.InvariantCulture));
                element.AppendChild(subNode);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.

            if (context == SaveContext.Undo)
            {
                foreach (XmlNode subNode in element.ChildNodes)
                {
                    if (!subNode.Name.Equals("Range"))
                        continue;
                    if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                        continue;

                    double min = this.Min;
                    double max = this.Max;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals(/*NXLT*/"min"))
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals(/*NXLT*/"max"))
                            max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                    }

                    this.Min = min;
                    this.Max = max;
                    break;
                }
            }
        }

        #endregion

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new DoubleDisplay();
            switch (name)
            {
                case /*NXLT*/"Min":
                    Min = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case /*NXLT*/"Max":
                    Max = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case /*NXLT*/"Value":
                    Value = ((double)converter.ConvertBack(value, typeof(double), null, null));
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

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }
    }
}