using System;

namespace DSCoreNodesUI.Input
{
    public abstract class SliderBase<T> : BasicInteractive<T> where T:IComparable<T>
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
    }
}
