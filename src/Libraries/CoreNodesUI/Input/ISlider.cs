using System.ComponentModel;

namespace DSCoreNodesUI.Input
{
    public interface ISlider<T>:INotifyPropertyChanged
    {
        T Max { get; set; }
        T Min { get; set; }
        T Step { get; set; }
        T Value { get; set; }
    }
}
