using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class is used for storing the expanders status of the Preferences window
    /// </summary>
    public class ExpanderSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isExpanded;
        public ExpanderSettings(string name, bool isExpanded)
        {
            Name = name;
            IsExpanded = isExpanded;
        }
        public string Name { get; set; }
        public string Tab { get; set; }
        public bool IsExpanded 
        {
            get
            {
                return isExpanded;
            }
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
    }
}
