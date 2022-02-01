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
    public class CustomSelectionItemViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly CustomSelectionItem customSelectionItem;

        /// <summary>
        /// Event raised when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        internal Func<CustomSelectionItem, bool> IsUnique { get; set; }
        internal Action ItemChanged { get; set; }
        internal Action<CustomSelectionItemViewModel> RemoveRequested { get; set; }


        /// <summary>
        /// Then name of the menu item
        /// </summary>
        public string Name
        {
            get
            {
                return customSelectionItem.Name;
            }
            set
            {
                customSelectionItem.Name = value;

                OnPropertyChanged("Name");
                OnPropertyChanged("IsValid");

                if (ItemChanged != null)
                    ItemChanged();
            }
        }


        /// <summary>
        /// The value stored in the menu item
        /// </summary>
        public string Value
        {
            get
            {
                return customSelectionItem.Value;
            }
            set
            {
                customSelectionItem.Value = value;
                OnPropertyChanged("Value");

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

        #endregion

        #region IDataErrorInfo

        [JsonIgnore]
        public string Error
        {
            get
            {
                return string.Empty;
            }
        }

        [JsonIgnore]
        public string this[string columnName]
        {
            get
            {
                if (columnName.Equals("Name"))
                {
                    if (string.IsNullOrWhiteSpace(Name))
                        return "Name cannot be empty";
                    else if (IsUnique != null && !IsUnique(customSelectionItem))
                        return "Name must be unique";
                }

                return string.Empty;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command for removing the menu item
        /// </summary>
        [JsonIgnore]
        public ICommand RemoveCommand { get; private set; }

        #endregion

        #region Public logic


        /// <summary>
        /// Construct a new custom dropdown menu item
        /// </summary>
        public CustomSelectionItemViewModel()
        {
            customSelectionItem = new CustomSelectionItem();
            RemoveCommand = new RelayCommand(RemoveCommandExecute);
        }

        public CustomSelectionItemViewModel(CustomSelectionItem customSelectionItem)
        {
            if (customSelectionItem == null)
                throw new ArgumentException("enumItem cannot be null");

            this.customSelectionItem = customSelectionItem;
            RemoveCommand = new RelayCommand(RemoveCommandExecute);
        }

        public void RemoveCommandExecute(object param)
        {
            if (RemoveRequested != null)
                RemoveRequested(this);
        }

        public void Validate()
        {
            OnPropertyChanged("Name");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}