using System;
using System.ComponentModel;
using System.Windows.Input;
using Newtonsoft.Json;

namespace CoreNodeModels
{
    /// <summary>
    /// The name and value of an item in the custom dropdown menu
    /// </summary>
    public class CustomSelectionItem
    {
        /// <summary>
        /// The name of the dropdown item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value stored in the dropdown menu item
        /// </summary>
        public string Value { get; set; }
    }


    /// <summary>
    /// Represents an item and in the custom dropdown menu
    /// </summary>
    public class CustomSelectionItemViewModel : INotifyPropertyChanged
    {
        private readonly CustomSelectionItem customSelectionItem;

        internal Func<CustomSelectionItem, bool> IsUnique { get; set; }
        internal Action ItemChanged { get; set; }
        internal Action<CustomSelectionItemViewModel> RemoveRequested { get; set; }

        /// <summary>
        /// Event raised when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Command for removing the menu item
        /// </summary>
        [JsonIgnore]
        public ICommand RemoveCommand { get; private set; }

        /// <summary>
        /// Then name of the menu item
        /// </summary>
        public string Name
        {
            get => customSelectionItem.Name;
            set
            {
                customSelectionItem.Name = value;

                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(IsValid));

                if (ItemChanged != null)
                    ItemChanged();
            }
        }


        /// <summary>
        /// The value stored in the menu item
        /// </summary>
        public string Value
        {
            get => customSelectionItem.Value;
            set
            {
                customSelectionItem.Value = value;
                OnPropertyChanged(nameof(Value));

                if (ItemChanged != null)
                    ItemChanged();
            }
        }

        [JsonIgnore]
        internal bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return false;

                if (IsUnique != null && !IsUnique(customSelectionItem))
                    return false;

                return true;
            }
        }


        /// <summary>
        /// Construct a new custom dropdown menu item with a given name and value
        /// </summary>
        /// <param name="customSelectionItem"></param>
        /// <exception cref="ArgumentException"></exception>
        public CustomSelectionItemViewModel(CustomSelectionItem customSelectionItem)
        {
            if (customSelectionItem == null)
                throw new ArgumentException("enumItem cannot be null");

            this.customSelectionItem = customSelectionItem;
            RemoveCommand = new RemoveMenuItemCommand(RemoveMenuItem);
        }

        private void RemoveMenuItem(object param)
        {
            if (RemoveRequested != null)
                RemoveRequested(this);
        }

        internal void Validate()
        {
            OnPropertyChanged(nameof(Name));
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        class RemoveMenuItemCommand : ICommand
        {
            private readonly Action<object> execute;
            public event EventHandler CanExecuteChanged;

            public RemoveMenuItemCommand(Action<object> execute)
            {
                this.execute = execute;
            }

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => execute(parameter);
        }
    }
}