using System.Globalization;

using Dynamo.Core;

namespace DSCoreNodesUI.Input
{
    public enum NumericFormat{Double, Integer}

    /// <summary>
    /// The SliderViewModel acts as the converter
    /// for numeric sliders. By using a view model
    /// to do the conversion instead of a converter,
    /// we can do conditional conversion based on the 
    /// context in which the conversion happens.
    /// </summary>
    public class SliderViewModel : NotificationObject
    {
        private NumericFormat format;
        private SliderBase model;

        public string MaxText
        {
            get { return ConvertToString(format, model.Max); }
        }

        public string MinText
        {
            get { return ConvertToString(format, model.Min); }
        }

        public string StepText
        {
            get { return ConvertToString(format, model.Step); }
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
}
