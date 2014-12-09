using System;
using System.Globalization;
using System.Xml;

using Dynamo.Controls;
using Dynamo.Models;

using Autodesk.DesignScript.Runtime;

using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Nodes
{
    public enum NumericFormat{Double, Integer}

    /// <summary>
    /// The SliderViewModel acts as the converter
    /// for numeric sliders. By using a view model
    /// to do the conversion instead of a converter,
    /// we can do conditional conversion based on the 
    /// context in which the conversion happens.
    /// </summary>
    public class SliderViewModel: NotificationObject
    {
        private NumericFormat format;
        private SliderBase model;

        public string MaxText
        {
            get { return ConvertToString(format, model.Max); }
        }

        public string MinText
        {
            get{ return ConvertToString(format, model.Min); }
        }

        public string StepText
        {
            get{return ConvertToString(format, model.Step); }
        }

        public string ValueText
        {
            get { return ConvertToString(format, model.Value); }
        }

        public double Max
        {
            get { return model.Max; }
        }

        public double Min
        {
            get { return model.Min; }
        }

        public double Step
        {
            get { return model.Step; }
        }

        public double Value
        {
            get { return model.Value; }
            set
            {
                if (value >= model.Max)
                    model.Max = value;

                if (value <= model.Min)
                    model.Min = value;

                model.Value = value;
            }
        }

        public SliderViewModel(NumericFormat format, SliderBase sliderBaseModel)
        {
            this.format = format;
            model = sliderBaseModel;
            model.PropertyChanged += model_PropertyChanged;
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Max":
                    RaisePropertyChanged("Max");
                    RaisePropertyChanged("MaxText");
                    break;
                case "Min":
                    RaisePropertyChanged("Min");
                    RaisePropertyChanged("MinText");
                    break;
                case "Value":
                    RaisePropertyChanged("Value");
                    RaisePropertyChanged("ValueText");
                    break;
                case "Step":
                    RaisePropertyChanged("Step");
                    RaisePropertyChanged("StepText");
                    break;
            }
        }

        internal static string ConvertToString(NumericFormat format, double value)
        {
            switch (format)
            {
                case NumericFormat.Double:
                    return value.ToString("0.000", CultureInfo.InvariantCulture);
                case NumericFormat.Integer:
                    return value.ToString("0", CultureInfo.InvariantCulture);
            }

            return "0.0";
        }

        internal static double ConvertToDouble(NumericFormat format, string value)
        {
            switch (format)
            {
                case NumericFormat.Double:
                    double d = 0.0;
                    double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
                    return d;

                case NumericFormat.Integer:
                    int i = 0;
                    int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out i);
                    return i;
            }

            return 0.0;
        }
    }

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

        protected SliderBase(WorkspaceModel workspace): base(workspace){}

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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
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

    [NodeName("Number Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces numeric values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [NodeSearchTags(new []{"double","number","float","integer","slider"})]
    public class DoubleSlider : SliderBase
    {
        public DoubleSlider(WorkspaceModel workspace) : base(workspace)
        {
            Min = 0;
            Max = 100;
            Step = 0.01;
            Value = 0;
        }

        /// <summary>
        /// UI is initialized and bindings are setup here.
        /// </summary>
        /// <param name="nodeUI">UI view that we can customize the UI of.</param>
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var slider = new UI.Controls.DynamoSlider(this, nodeUI)
            {
                DataContext = new SliderViewModel(NumericFormat.Double, this)
            };

            nodeUI.inputGrid.Children.Add(slider);
        }

    }

    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces integer values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider : SliderBase
    {
        public IntegerSlider(WorkspaceModel workspace) : base(workspace)
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Step = 1;
            Value = 0;
        }

        /// <summary>
        /// UI is initialized and bindings are setup here.
        /// </summary>
        /// <param name="nodeUI">UI view that we can customize the UI of.</param>
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var slider = new UI.Controls.DynamoSlider(this, nodeUI)
            {
                DataContext = new SliderViewModel(NumericFormat.Integer,this)
            };

            nodeUI.inputGrid.Children.Add(slider);
        }

    }
}
