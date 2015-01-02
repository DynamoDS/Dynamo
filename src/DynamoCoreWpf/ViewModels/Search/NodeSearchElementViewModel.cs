using System.ComponentModel;
using System.Windows.Input;

using Dynamo.Search.SearchElements;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    public class NodeSearchElementViewModel : NotificationObject, ISearchEntryViewModel
    {
        private bool isSelected;

        public NodeSearchElementViewModel(NodeSearchElement element)
        {
            Model = element;
            Model.PropertyChanged += ModelOnPropertyChanged;
            ClickedCommand = new DelegateCommand(Model.ProduceNode);
        }

        public void Dispose()
        {
            Model.PropertyChanged -= ModelOnPropertyChanged;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "Name":
                    RaisePropertyChanged("Name");
                    break;
                case "IsVisibleInSearch":
                    RaisePropertyChanged("Visibility");
                    break;
                case "Description":
                    RaisePropertyChanged("Description");
                    break;
            }
        }

        public NodeSearchElement Model { get; set; }

        public string Name
        {
            get { return Model.Name; }
        }

        public bool Visibility
        {
            get { return Model.IsVisibleInSearch; }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value.Equals(isSelected)) return;
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public string Description
        {
            get { return Model.Description; }
        }

        public ICommand ClickedCommand { get; private set; }
    }

    public class CustomNodeSearchElementViewModel : NodeSearchElementViewModel
    {
        private string path;

        public CustomNodeSearchElementViewModel(CustomNodeSearchElement element) : base(element)
        {
            Model.PropertyChanged += ModelOnPropertyChanged;
            Path = Model.Path;
        }

        public string Path
        {
            get { return path; }
            set
            {
                if (value == path) return;
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Path")
                RaisePropertyChanged("Path");
        }

        public new CustomNodeSearchElement Model
        {
            get { return base.Model as CustomNodeSearchElement; }
            set { base.Model = value; }
        }
    }
}
