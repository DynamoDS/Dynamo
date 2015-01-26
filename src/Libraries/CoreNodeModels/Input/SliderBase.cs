using System;
using System.Globalization;

namespace DSCoreNodesUI.Input
{
    public abstract class SliderBase<T> : BasicInteractive<T> where T : IComparable<T>
    {
        private T max;
        private T min;
        private T step;

        public T Max
        {
            get { return max; }
            set
            {
                max = value;

                if (max.CompareTo(min) < 0)
                {
                    Min = max;
                    Value = Min;
                }
                if (max.CompareTo(Value) < 0)
                {
                    Value = max;
                }

                RaisePropertyChanged("Max");
            }
        }

        public T Min
        {
            get { return min; }
            set
            {
                min = value;

                if (min.CompareTo(Max) > 0)
                {
                    Max = min;
                    Value = Max;
                }
                if (min.CompareTo(Value) > 0)
                {
                    Value = min;
                }

                RaisePropertyChanged("Min");
            }
        }

        public T Step
        {
            get { return step; }
            set
            {
                step = value;

                if (Value.CompareTo(Max) > 0 ||
                    Value.CompareTo(Max) == 0)
                {
                    this.Max = Value;
                }
                if (Value.CompareTo(Min) < 0 ||
                    Value.CompareTo(Min) == 0)
                {
                    this.Min = Value;
                }

                RaisePropertyChanged("Step");
            }
        }

        protected SliderBase()
        {
            base.PropertyChanged += SliderBase_PropertyChanged;
        }

        void SliderBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Value") return;

            if (Value.CompareTo(Min) < 0) Min = Value;
            if (Value.CompareTo(Max) > 0) Max = Value;
        }
        
        public static string ConvertNumberToString(T value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        internal static double ConvertStringToDouble(string value)
        {
            double result = 0.0;
            System.Double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
            return result;
        }

        internal static int ConvertStringToInt(string value)
        {
            int result = 0;
            System.Int32.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
            return result;
        }
    }
}