using System;
using System.Globalization;
using System.Xml;
using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName(/*NXLT*/"Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription(/*NXLT*/"IntegerSliderDescription", typeof(Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider : DSCoreNodesUI.Integer
    {
        public IntegerSlider()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Value = 0;

            ShouldDisplayPreviewCore = false;
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

                RaisePropertyChanged(/*NXLT*/"Max");
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

                RaisePropertyChanged(/*NXLT*/"Min");
            }
        }

        protected override bool UpdateValueCore(string name, string value, UndoRedoRecorder recorder)
        {
            var converter = new IntegerDisplay();
            switch (name)
            {
                case /*NXLT*/"Value":
                    Value = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case /*NXLT*/"Max":
                    Max = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case /*NXLT*/"Min":
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

            return base.UpdateValueCore(name, value, recorder);
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
            base.DeserializeCore(nodeElement, context); // Base implementation must be called.

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (!subNode.Name.Equals(/*NXLT*/"Range"))
                    continue;

                int min = Min;
                int max = Max;

                if (subNode.Attributes != null)
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals(/*NXLT*/"min"))
                            min = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals(/*NXLT*/"max"))
                            max = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals(/*NXLT*/"value"))
                            Value = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }
                }

                Min = min;
                Max = max;
            }
        }

        #endregion
    }
}
