using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodes
{
    public abstract class NodeWithUI : NodeModel
    {
        //We can remove this from NodeModel and only use it here.
        public abstract void SetupCustomUIElements(dynNodeView nodeUI);
    }

    public class NumberSlider : NodeWithUI
    {
        public NumberSlider()
        {
            Min = 0.0;
            Max = 100.0;
            Value = 50;
        }

        public AssociativeNode BuildAst()
        {
            return new DoubleNode(Value.ToString());
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a slider control to the input grid of the control
            var tbSlider = new DynamoSlider(this)
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                MinWidth = 150,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.None
            };

            tbSlider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
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
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

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
            valtb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay()
            });

            // slider value 
            var sliderBinding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            };
            tbSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);

            // max value
            maxtb.BindToProperty(new Binding("Max")
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
            mintb.BindToProperty(new Binding("Min")
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

        private double _val;
        public double Value
        {
            get
            {
                return _val;
            }
            set
            {
                _val = value;
                RaisePropertyChanged("Value");
            }
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

        #region Serialize and Deserialize
        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(double).FullName))
                {
                    double value = Value;
                    double min = Min;
                    double max = Max;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            value = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                        else if (attr.Name.Equals("min"))
                        {
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                        else if (attr.Name.Equals("max"))
                        {
                            max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                    }

                    Min = min;
                    Max = max;
                    Value = value;
                }
            }
        }
        #endregion
    }
}
