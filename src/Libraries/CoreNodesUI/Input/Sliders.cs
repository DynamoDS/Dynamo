using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Double Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces double values.")]
    [Browsable(false)]
    [IsDesignScriptCompatible]
    public class DoubleSlider : DSCoreNodesUI.Double
    {
        public DoubleSlider()
        {
            Value = 50;
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
                if (_min < Value)
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
            //add a slider control to the input grid of the control
            var tbSlider = new DynamoSlider(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 150,
                TickPlacement = TickPlacement.None
            };

            tbSlider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            // input value textbox
            var valtb = new DynamoTextBox
            {
                Width = double.NaN,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var maxtb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tbSlider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tbSlider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tbSlider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            valtb.BindToProperty(
                new Binding("Value") { Mode = BindingMode.TwoWay, Converter = new DoubleDisplay() });

            // slider value 
            var sliderBinding = new Binding("Value") { Mode = BindingMode.TwoWay, Source = this, };
            tbSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);

            // max value
            maxtb.BindToProperty(
                new Binding("Max")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new DoubleDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // max slider value
            var bindingMaxSlider = new Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MaximumProperty, bindingMaxSlider);


            // min value
            mintb.BindToProperty(
                new Binding("Min")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new DoubleDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // min slider value
            var bindingMinSlider = new Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
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
                if (subNode.Name.Equals("Range"))
                {
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
                        }
                    }

                    Min = min;
                    Max = max;
                }
            }
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("min", Min);
                helper.SetAttribute("max", Max);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Min = helper.ReadDouble("min");
                Max = helper.ReadDouble("max");
            }
        }

        #endregion
    }


    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces integer values.")]
    [Browsable(false)]
    [IsDesignScriptCompatible]
    public class IntegerSlider : DSCoreNodesUI.Integer
    {
        public IntegerSlider()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Value = 50;
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
            //base.SetupCustomUIElements(nodeUI);

            //add a slider control to the input grid of the control
            var tbSlider = new DynamoSlider(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 150,
                TickPlacement = TickPlacement.BottomRight,
                TickFrequency = 1,
                IsSnapToTickEnabled = true
            };

            tbSlider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            // input value textbox
            var valtb = new DynamoTextBox
            {
                Width = double.NaN,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var maxtb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tbSlider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tbSlider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tbSlider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            valtb.BindToProperty(
                new Binding("Value") { Mode = BindingMode.TwoWay, Converter = new IntegerDisplay() });

            // slider value 
            var sliderBinding = new Binding("Value") { Mode = BindingMode.TwoWay, Source = this, };
            tbSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);

            // max value
            maxtb.BindToProperty(
                new Binding("Max")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new IntegerDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // max slider value
            var bindingMaxSlider = new Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MaximumProperty, bindingMaxSlider);


            // min value
            mintb.BindToProperty(
                new Binding("Min")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new IntegerDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // min slider value
            var bindingMinSlider = new Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MinimumProperty, bindingMinSlider);
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new IntegerDisplay();
            switch (name)
            {
                case "Value":
                    Value = ((int)converter.ConvertBack(value, typeof(int), null, null));
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
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("Range"))
                {
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
                        }
                    }

                    Min = min;
                    Max = max;
                }
            }
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("min", Min);
                helper.SetAttribute("max", Max);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Min = helper.ReadInteger("min");
                Max = helper.ReadInteger("max");
            }
        }

        #endregion
    }
}
