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

        [Obsolete("Remove method after deprecating IntegerSlider in favor of IntegerSlider64Bit")]
        internal static int ConvertStringToInt(string value)
        {
            int result = 0;
            if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
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

        /// <summary>
        /// Convert a value with a string representation into 64 bit integers.
        /// This is used by IntegerSlider64Bit. The expected range of values are
        /// from -2^63 to 2^63 - 1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static long ConvertStringToInt64(string value)
        {
            long result = 0;
            if (long.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return result;
            //check if the value exceeds the 64 bit maximum / minimum value
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
                result = start == 0 ? long.MaxValue : long.MinValue;
            }
            return result;
        }

        /// <summary>
        /// check if the value is within int64 range
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static bool IsValueInt64(string value)
        {
            try
            {
                var result = Convert.ToInt64(value);
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}