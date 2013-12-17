using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Xml;
using DSCoreNodesUI;
using Dynamo.Controls;
using Dynamo.Models;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Sample that contains a slider and produces a number.
    /// </summary>
    [Browsable(false)]
    public class NumberSlider : NodeModel, IWpfNode
    {
        public NumberSlider()
        {
            Value = 50;
        }

        /// <summary>
        /// Builds the custom AST that contains information bound to the UI.
        /// </summary>
        [Browsable(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    new DoubleNode(Value.ToString(CultureInfo.InvariantCulture)))
            };
        }

        /// <summary>
        /// UI is initialized and bindings are setup here.
        /// </summary>
        /// <param name="nodeUI">UI view that we can customize the UI of.</param>
        [Browsable(false)]
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a slider control to the input grid of the control
            var slider = new DynamoSlider(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 150,
                TickPlacement = TickPlacement.None,
                Minimum = -100,
                Maximum = 100
            };

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(slider);

            Grid.SetColumn(slider, 0);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            slider.DataContext = this;
            
            // slider value 
            var sliderBinding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            };
            slider.SetBinding(RangeBase.ValueProperty, sliderBinding);
        }

        private double _val;
        /// <summary>
        /// Current value of the slider, will be used as the value of the Number AST produced by this node.
        /// </summary>
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

        #region Load/Save
        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(double).FullName))
                {
                    double value = Value;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            value = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                    }

                    Value = value;
                }
            }
        }
        #endregion
    }

}
