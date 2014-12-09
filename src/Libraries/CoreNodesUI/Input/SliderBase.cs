using System;
using System.Globalization;
using System.Xml;

using Dynamo.Models;

namespace DSCoreNodesUI.Input
{
    /// <summary>
    /// SliderBase contains logic for range and 
    /// step values for slider classes.
    /// </summary>
    public abstract class SliderBase : DSCoreNodesUI.Double
    {
        private double _max;
        public double Max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (_max <= Value)
                    Value = _max;
                RaisePropertyChanged("Max");
            }
        }

        private double _min;
        public double Min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (_min >= Value)
                    Value = _min;
                RaisePropertyChanged("Min");
            }
        }

        private double _step;
        public double Step
        {
            get { return _step; }
            set
            {
                _step = value;
                RaisePropertyChanged("Step");
            }
        }

        protected override bool ShouldDisplayPreviewCore
        {
            get { return false; }
        }

        protected SliderBase(WorkspaceModel workspace) : base(workspace){}

        protected override bool UpdateValueCore(string name, string value)
        {
            switch (name)
            {
                case "Min":
                case "MinText":
                    Min = SliderViewModel.ConvertToDouble(NumericFormat.Double, value);
                    return true; // UpdateValueCore handled.
                case "Max":
                case "MaxText":
                    Max = SliderViewModel.ConvertToDouble(NumericFormat.Double, value);
                    return true; // UpdateValueCore handled.
                case "Value":
                case "ValueText":
                    Value = SliderViewModel.ConvertToDouble(NumericFormat.Double, value);
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
                    Step = SliderViewModel.ConvertToDouble(NumericFormat.Double, value);
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

                double min = Min;
                double max = Max;
                double step = Step;

                if (subNode.Attributes != null)
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("min"))
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("max"))
                            max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("value"))
                            Value = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("step"))
                            step = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                    }
                }

                Min = min;
                Max = max;
                Step = step;
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
                subNode.SetAttribute("step", Step.ToString(CultureInfo.InvariantCulture));
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

                    double min = Min;
                    double max = Max;
                    double step = Step;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("min"))
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("max"))
                            max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("step"))
                            step = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                    }

                    Min = min;
                    Max = max;
                    Step = step;

                    break;
                }
            }
        }

        #endregion
    }

}
