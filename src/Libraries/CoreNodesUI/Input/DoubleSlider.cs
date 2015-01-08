using System;
using System.Globalization;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName(/*NXLT*/"Double Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("DoubleSliderDescription", typeof(Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class DoubleSlider : DSCoreNodesUI.Double
    {
        public DoubleSlider()
        {
            Value = 0;
            Min = 0;
            Max = 100;
            ShouldDisplayPreviewCore = false;
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

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            XmlElement outEl = element.OwnerDocument.CreateElement("Range");
            outEl.SetAttribute(/*NXLT*/"min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute(/*NXLT*/"max", Max.ToString(CultureInfo.InvariantCulture));
            element.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context); //Base implementation must be called.

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

        protected override bool UpdateValueCore(string name, string value, UndoRedoRecorder recorder)
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

            return base.UpdateValueCore(name, value, recorder);
        }
    }
}