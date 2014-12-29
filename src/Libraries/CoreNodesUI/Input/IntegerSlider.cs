using System;
using System.Globalization;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;

namespace DSCoreNodesUI.Input
{
    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces integer values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider : DSCoreNodesUI.Integer, ISlider<int>
    {
        private int max;
        private int min;
        private int step;

        public IntegerSlider()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Step = 1;
            Value = 0;
            ShouldDisplayPreviewCore = false;
        }

        public int Max
        {
            get { return max; }
            set
            {
                max = value;
                RaisePropertyChanged("Max");
            }
        }

        public int Min
        {
            get { return min; }
            set
            {
                min = value;
                RaisePropertyChanged("Min");
            }
        }

        public int Step
        {
            get { return step; }
            set
            {
                step = value;
                RaisePropertyChanged("Step");
            }
        }

        protected override bool UpdateValueCore(string name, string value, UndoRedoRecorder recorder)
        {
            switch (name)
            {
                case "Min":
                case "MinText":
                    Min = SliderViewModel<int>.ConvertStringToInt(value);
                    if (Min > Max)
                    {
                        Max = Min;
                        Value = Max;
                    }
                    return true; // UpdateValueCore handled.
                case "Max":
                case "MaxText":
                    Max = SliderViewModel<int>.ConvertStringToInt(value);
                    if (Max < Min)
                    {
                        Min = Max;
                        Value = Min;
                    }
                    return true; // UpdateValueCore handled.
                case "Value":
                case "ValueText":
                    Value = SliderViewModel<int>.ConvertStringToInt(value);
                    if (Value >= Max)
                    {
                        this.Max = Value;
                    }
                    if (Value <= Min)
                    {
                        this.Min = Value;
                    }
                    return true; // UpdateValueCore handled.
                case "Step":
                case "StepText":
                    Step = SliderViewModel<int>.ConvertStringToInt(value);
                    return true;
            }

            return base.UpdateValueCore(name, value, recorder);
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            if (context != SaveContext.Undo) return;

            var xmlDocument = element.OwnerDocument;
            XmlElement subNode = xmlDocument.CreateElement("Range");
            subNode.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            subNode.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            subNode.SetAttribute("step", Step.ToString(CultureInfo.InvariantCulture));
            element.AppendChild(subNode);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.

            if (context != SaveContext.Undo) return;

            foreach (XmlNode subNode in element.ChildNodes)
            {
                if (!subNode.Name.Equals("Range"))
                    continue;
                if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                    continue;

                foreach (XmlAttribute attr in subNode.Attributes)
                {
                    if (attr.Name.Equals("min"))
                        Min = Convert.ToInt16(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("max"))
                        Max = Convert.ToInt16(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("step"))
                        Step = Convert.ToInt16(attr.Value, CultureInfo.InvariantCulture);
                }

                break;
            }
        }

        #endregion
    }
}
