using System;

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

        public string MaxText
        {
            get { return SliderBase<T>.ConvertNumberToString(model.Max); }
        }

        public string MinText
        {
            get { return SliderBase<T>.ConvertNumberToString(model.Min); }
        }

        public string StepText
        {
            get { return SliderBase<T>.ConvertNumberToString(model.Step); }
        }

        public string ValueText
        {
            get { return SliderBase<T>.ConvertNumberToString(model.Value); }
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

                model.Value = value;
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