namespace DSCoreNodesUI.Input
{
    public abstract class SliderBase<T> : BasicInteractive<T>
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
                RaisePropertyChanged("Max");
            }
        }

        public T Min
        {
            get { return min; }
            set
            {
                min = value;
                RaisePropertyChanged("Min");
            }
        }

        public T Step
        {
            get { return step; }
            set
            {
                step = value;
                RaisePropertyChanged("Step");
            }
        }
    }
}
