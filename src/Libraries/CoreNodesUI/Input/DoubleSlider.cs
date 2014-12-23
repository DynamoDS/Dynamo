using System;
using System.Globalization;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using DSCoreNodesUI.Input;

using Dynamo.Models;

namespace Dynamo.Nodes
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

        public DoubleSlider(WorkspaceModel workspace)
            : base(workspace)
        {
            Min = 0;
            Max = 100;
            Step = 0.01;
            Value = 0;
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

        protected override bool ShouldDisplayPreviewCore
        {
            get { return false; }
        }

        protected override bool UpdateValueCore(string name, string value)
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

            return base.UpdateValueCore(name, value);
        }

        #region Load/Save

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);

            XmlElement outEl = xmlDoc.CreateElement("Range");
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("step", Step.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (!subNode.Name.Equals("Range"))
                    continue;

                if (subNode.Attributes == null) continue;

                foreach (XmlAttribute attr in subNode.Attributes)
                {
                    if (attr.Name.Equals("min"))
                        Min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("max"))
                        Max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("step"))
                        Step = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                }
            }
        }

        #endregion

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