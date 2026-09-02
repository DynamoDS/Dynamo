using System;
using System.Globalization;
using CoreNodeModels.Input;

using Dynamo.Core;

namespace CoreNodeModelsWpf
{
    /// <summary>
    /// The SliderViewModel acts as the converter
    /// for numeric sliders. By using a view model
    /// to do the conversion instead of a converter,
    /// we can do conditional conversion based on the 
    /// context in which the conversion happens.
    /// </summary>
    public class SliderViewModel<T> : NotificationObject where T : IComparable<T>
    {
        private SliderBase<T> model;

        // These text setters intentionally discard the value. Bindings are TwoWay so
        // ValidateWithoutUpdate() can run; DynamoTextBox writes the model via
        // UpdateModelValueCommand. Implementing these would double-commit on every edit.
        public string MaxText
        {
            get => SliderBase<T>.ConvertNumberToString(model.Max);
            set => _ = value;
        }

        public string MinText
        {
            get => SliderBase<T>.ConvertNumberToString(model.Min);
            set => _ = value;
        }

        public string StepText
        {
            get => SliderBase<T>.ConvertNumberToString(model.Step);
            set => _ = value;
        }

        public string ValueText
        {
            get => SliderBase<T>.ConvertNumberToString(model.Value);
            set => _ = value;
        }

        public T Max
        {
            get { return model.Max; }
        }

        public T Min
        {
            get { return model.Min; }
        }

        public T Step
        {
            get { return model.Step; }
        }

        public T Value
        {
            get { return model.Value; }
            set
            {
                if (value.CompareTo(model.Max) == 1)
                    model.Max = value;

                if (value.CompareTo(model.Min) == -1)
                    model.Min = value;

                double.TryParse(model.Step.ToString(), out double stepValue);
                var stepValueString = stepValue.ToString(null, CultureInfo.InvariantCulture);
                var decimalPoints = 0;
                if (stepValueString.Contains('.'))
                {
                    decimalPoints = stepValueString.Substring(stepValueString.IndexOf('.') + 1).Length;
                }

                if (value is IFormattable formattableval)
                {
                    var invariantString  = formattableval.ToString(null,CultureInfo.InvariantCulture);
                    var sliderValue = Math.Round(decimal.Parse(invariantString, CultureInfo.InvariantCulture), decimalPoints);
                    model.UpdateValue(new Dynamo.Graph.UpdateValueParams(nameof(Value), sliderValue.ToString(CultureInfo.InvariantCulture)));
                }
                else
                {
                    model.UpdateValue(new Dynamo.Graph.UpdateValueParams(nameof(Value), value.ToString()));
                }

            }
        }

        public SliderViewModel(SliderBase<T> sliderBaseModel)
        {
            model = sliderBaseModel;
            model.PropertyChanged += model_PropertyChanged;
        }

        private void model_PropertyChanged(
            object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

    }
}
