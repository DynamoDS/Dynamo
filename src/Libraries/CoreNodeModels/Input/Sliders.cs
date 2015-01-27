using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using DSCoreNodesUI;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.UI;
using Autodesk.DesignScript.Runtime;
using Brush = System.Drawing.Brush;

namespace Dynamo.Nodes
{
    [NodeName("Double Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces double values.")]
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
                if (_min > Value)
                    Value = _min;
                RaisePropertyChanged("Min");
            }
        }

        /// <summary>
        /// UI is initialized and bindings are setup here.
        /// </summary>
        /// <param name="nodeUI">UI view that we can customize the UI of.</param>
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            BuildSliderUI(nodeUI, this, Value, SerializeValue(), 
                            new DoubleSliderSettingsControl(){ DataContext = this }, new DoubleDisplay());
        }

        internal static void BuildSliderUI(dynNodeView nodeUI, NodeModel nodeModel,
            double value, string serializedValue, UIElement sliderSettingsControl, 
            IValueConverter numberDisplayConverter  )
        {
            //add a slider control to the input grid of the control
            var tbSlider = new DynamoSlider(nodeModel)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = Configurations.PortHeightInPixels,
                MinWidth = 150,
                TickPlacement = TickPlacement.None,
                Value = value
            };

            tbSlider.PreviewMouseUp += delegate
            {
                nodeUI.ViewModel.DynamoViewModel.ReturnFocusToSearch();
            };

            // build grid for input and expander
            var textBoxExpanderGrid = new Grid()
            {
                MinWidth = 150
            };

            textBoxExpanderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            textBoxExpanderGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(29)
            });

            // input value textbox
            var valtb = new DynamoTextBox(serializedValue);

            Grid.SetColumn(valtb, 0);
            textBoxExpanderGrid.Children.Add(valtb);

            var exp = new Expander();
            exp.Padding = new Thickness(4, 0, 0, 0);
            Grid.SetColumn(exp, 1);
            textBoxExpanderGrid.Children.Add(exp);

            var sliderSp = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            sliderSp.Children.Add(textBoxExpanderGrid);

            // min max control
            var minMaxControl = sliderSettingsControl;
            minMaxControl.Visibility = Visibility.Collapsed;
            sliderSp.Children.Add(minMaxControl);

            // expander modifies visibility of min/max
            exp.Expanded += (sender, args) =>
            {
                minMaxControl.Visibility = Visibility.Visible;
            };

            exp.Collapsed += (sender, args) =>
            {
                minMaxControl.Visibility = Visibility.Collapsed;
            };

            nodeUI.inputGrid.Children.Add(tbSlider);
            nodeUI.PresentationGrid.Children.Add(sliderSp);
            nodeUI.PresentationGrid.Visibility = Visibility.Visible;

            tbSlider.DataContext = nodeModel;
            valtb.DataContext = nodeModel;

            // value input
            valtb.BindToProperty(
                new Binding("Value") { Mode = BindingMode.TwoWay, Converter = numberDisplayConverter });

            // slider value 
            var sliderBinding = new Binding("Value") { Mode = BindingMode.TwoWay, Source = nodeModel };
            tbSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);

            // max slider value
            var bindingMaxSlider = new Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = nodeModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MaximumProperty, bindingMaxSlider);

            // min slider value
            var bindingMinSlider = new Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = nodeModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MinimumProperty, bindingMinSlider);
        }

        #region Load/Save

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
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

                double min = Min;
                double max = Max;

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
                        if (attr.Name.Equals("min"))
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("max"))
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
                case "Min":
                    Min = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case "Max":
                    Max = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case "Value":
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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }

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

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            DoubleSlider.BuildSliderUI(nodeUI, this, Value, SerializeValue(),
                new IntegerSliderSettingsControl()
                {
                    DataContext = this
                }, new IntegerDisplay());
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new IntegerDisplay();
            switch (name)
            {
                case "Value":
                    Value = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    if (Value >= Max)
                    {
                        this.Max = Value;
                    }
                    if (Value <= Min)
                    {
                        this.Min = Value;
                    }
                    return true; // UpdateValueCore handled.
                case "Max":
                    Max = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Min":
                    Min = ((int)converter.ConvertBack(value, typeof(int), null, null));
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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }
}
