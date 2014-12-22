using System;
using System.Globalization;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using DSCoreNodesUI.Input;

using Dynamo.Models;

namespace Dynamo.Nodes
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

        public IntegerSlider(WorkspaceModel workspace)
            : base(workspace)
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Step = 1;
            Value = 0;
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
                        Min = Convert.ToInt16(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("max"))
                        Max = Convert.ToInt16(attr.Value, CultureInfo.InvariantCulture);
                    else if (attr.Name.Equals("step"))
                        Step = Convert.ToInt16(attr.Value, CultureInfo.InvariantCulture);
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
