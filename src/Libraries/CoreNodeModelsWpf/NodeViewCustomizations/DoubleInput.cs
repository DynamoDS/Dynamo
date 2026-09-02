using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{   
    public class DoubleInputNodeViewCustomization : INodeViewCustomization<DoubleInput>
    {
        public void CustomizeView(DoubleInput nodeModel, NodeView nodeView)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(nodeModel.Value ?? "0.0")
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                MinWidth = 40
            };

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = nodeModel;
            var textToValueBinding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleInputDisplay(),
                ConverterCulture = CultureInfo.InvariantCulture,
                NotifyOnValidationError = false,
                Source = nodeModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            var numericalValidation = new NumericValidationRule();
            numericalValidation.ValidationStep = ValidationStep.RawProposedValue;
            textToValueBinding.ValidationRules.Add(numericalValidation);
            tb.BindToProperty(textToValueBinding);
            Validation.SetErrorTemplate(tb, null);
        }

        public void Dispose()
        {
        }
    }
}
