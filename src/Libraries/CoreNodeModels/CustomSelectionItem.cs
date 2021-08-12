using System;
using System.ComponentModel;
using System.Windows.Input;
using Newtonsoft.Json;

namespace CoreNodeModels
{
    /// <summary>
    /// Represents an Enum item
    /// </summary>
    public class CustomSelectionItem
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public CustomSelectionItem()
        {

        }
    }


    public class CustomSelectionItemViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly CustomSelectionItem customSelectionItem;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        [JsonIgnore]
        public Func<CustomSelectionItem, bool> IsUnique { get; set; }

        [JsonIgnore]
        public Action ItemChanged { get; set; }

        [JsonIgnore]
        public Action<CustomSelectionItemViewModel> RemoveRequested { get; set; }

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
        public bool IsValid
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

        [JsonIgnore]
        public ICommand RemoveCommand { get; private set; }

        #endregion

        #region Public logic

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