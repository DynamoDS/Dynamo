using System.Windows.Input;

using Dynamo.UI.Commands;

using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    /// <summary>
    /// Class that is used to filter nodes in search ui.
    /// If search category is selected, then nodes of this category are shown in search.
    /// </summary>
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

        /// <summary>
        /// Fires, when category button is clicked.
        /// </summary>
        public ICommand ClickCommand { get; private set; }

        /// <summary>
        /// Fires, when "only" textblock is clicked.
        /// </summary>
        public ICommand OnlyClickCommand { get; private set; }

        /// <summary>
        /// Creates search category, it's used in Search UI to filter nodes.
        /// </summary>
        /// <param name="title">name of category, e.g. Core, BuiltIn etc.</param>
        public SearchCategory(string title)
        {
            name = title;
            isSelected = true;

            ClickCommand = new DelegateCommand(ToggleSelect);
            OnlyClickCommand = new DelegateCommand(SelectOnlyThisCategory);
        }

        private void SelectOnlyThisCategory(object obj)
        {
            IsSelected = true;
        }

        private void ToggleSelect(object obj)
        {
            IsSelected = !IsSelected;
        }
    }
}
