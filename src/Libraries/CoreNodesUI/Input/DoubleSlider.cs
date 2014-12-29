using System;
using System.Globalization;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;

namespace DSCoreNodesUI.Input
{
    [NodeName("Number Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces numeric values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [NodeSearchTags(new[] { "double", "number", "float", "integer", "slider" })]
    public class DoubleSlider : DSCoreNodesUI.Double, ISlider<double>
    {
        private double max;
        private double min;
        private double step;

        public DoubleSlider()
        {
            Min = 0;
            Max = 100;
            Step = 0.01;
            Value = 0;
            ShouldDisplayPreviewCore = false;
        }

        public double Max
        {
            get { return max; }
            set
            {
                max = value; 
                RaisePropertyChanged("Max");
            }
        }

        public double Min
        {
            get { return min; }
            set
            {
                min = value; 
                RaisePropertyChanged("Min");
            }
        }

        public double Step
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
                    Min = SliderViewModel<double>.ConvertStringToDouble(value);
                    if (Min > Max)
                    {
                        Max = Min;
                        Value = Max;
                    }
                    return true; // UpdateValueCore handled.
                case "Max":
                case "MaxText":
                    Max = SliderViewModel<double>.ConvertStringToDouble(value);
                    if (Max < Min)
                    {
                        Min = Max;
                        Value = Min;
                    }
                    return true; // UpdateValueCore handled.
                case "Value":
                case "ValueText":
                    Value = SliderViewModel<double>.ConvertStringToDouble(value);
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
                    Step = SliderViewModel<double>.ConvertStringToDouble(value);
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
                        Min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("max"))
                        Max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("step"))
                        Step = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                }

                break;
            }
        }

        #endregion
    }
}