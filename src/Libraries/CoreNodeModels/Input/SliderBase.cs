using System;
using System.Collections.Generic;
using System.Globalization;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace CoreNodeModels.Input
{
    public abstract class SliderBase<T> : BasicInteractive<T> where T : IComparable<T>
    {
        private T max;
        private T min;
        private T step;

        [JsonProperty("MaximumValue")]
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

        [JsonProperty("MinimumValue")]
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

        [JsonProperty("StepValue")]
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

        protected SliderBase(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            base.PropertyChanged += SliderBase_PropertyChanged;
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
            System.Double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            return result;
        }

        internal static int ConvertStringToInt(string value)
        {
            int result = 0;
            if (System.Int32.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return result;
            //check if the value exceeds the 32 bit maximum / minimum value
            if (value.Length > 1)
            {
                var start = value[0] == '-' ? 1 : 0;
                for (var i = start; i < value.Length; i++)
                {
                    if (!char.IsDigit(value[i]))
                    {
                        return 0;
                    }

                }
                result = start == 0 ? int.MaxValue : int.MinValue;
            }
            return result;
        }
    }
}