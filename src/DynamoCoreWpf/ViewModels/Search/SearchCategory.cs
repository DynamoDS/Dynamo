using System.Windows.Input;

using Dynamo.UI.Commands;
using Dynamo.ViewModels;

using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    public class SearchCategory : NotificationObject
    {
        private readonly string name;

        /// <summary>
        /// Name of category
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        private bool isSelected;

        /// <summary>
        /// If category is selected, nodes of this category are shown as search results.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public ICommand ClickCommand { get; private set; }

        public SearchCategory(string title)
        {
            name = title;
            isSelected = true;

            ClickCommand = new DelegateCommand(ToggleSelect);
        }

        private void ToggleSelect(object obj)
        {
            IsSelected = !IsSelected;
        }
    }
}
