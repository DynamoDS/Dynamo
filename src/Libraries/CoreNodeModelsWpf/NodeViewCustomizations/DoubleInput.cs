using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CoreNodeModels.Input;
using CoreNodeModels.Properties;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    internal class NumericValidationRule : ValidationRule
    {
        //if the string can be parsed to a common numeric type return true
        internal bool validateInput(string value)
        {
            double doubleVal;
            long longVal;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleVal)
                || long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out longVal))
            {
                return true;
            }
            return false;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {

            if (!validateInput(value as string))
            {
                return new ValidationResult(false, Resources.NumberNodeInputMustBeNumeric);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
   
    public class DoubleInputNodeViewCustomization : INodeViewCustomization<DoubleInput>
    {
        public void CustomizeView(DoubleInput nodeModel, NodeView nodeView)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(nodeModel.Value ?? "0.0")
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))  
            };

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = nodeModel;
            var textToValueBinding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleInputDisplay(),
                NotifyOnValidationError = false,
                Source = nodeModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            var numericalValidation = new NumericValidationRule();
            numericalValidation.ValidationStep = ValidationStep.ConvertedProposedValue;
            textToValueBinding.ValidationRules.Add(numericalValidation);
            tb.BindToProperty(textToValueBinding);
            Validation.SetErrorTemplate(tb, null);
        }

        public void Dispose()
        {
        }
    }
}