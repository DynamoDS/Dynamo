using System.ComponentModel;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Dead simple view model for when you only need to have a different display name and underlying value.
    /// </summary>
    /// <typeparam name="T">The type of your value</typeparam>
    public class SimpleViewModel<T> : ViewModelBase, INotifyPropertyChanged
    {
        /// <summary>
        /// Underlying value.
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// Display name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Create a SimpleViewModel given an underlying value and display name.
        /// </summary>
        /// <param name="value">Underlying value</param>
        /// <param name="name">Display name</param>
        public SimpleViewModel(T value, string name)
        {
            Value = value;
            Name = name;
        }
    }
}
