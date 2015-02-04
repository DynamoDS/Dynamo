using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    public class NodeSearchElementViewModel : NotificationObject, ISearchEntryViewModel
    {
        private bool isSelected;

        public event RequestBitmapSourceHandler RequestBitmapSource;
        public void OnRequestBitmapSource(IconRequestEventArgs e)
        {
            if (RequestBitmapSource != null)
            {
                RequestBitmapSource(e);
            }
        }

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

        public string FullName
        {
            get { return Model.FullName; }
        }

        public string Assembly
        {
            get { return Model.Assembly; }
        }

        public bool Visibility
        {
            get { return Model.IsVisibleInSearch; }
        }

        public bool IsExpanded
        {
            get { return true; }
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

        public bool HasDescription
        {
            get { return (!string.IsNullOrEmpty(Model.Description)); }
        }

        public NodeCategoryViewModel Category { get; set; }

        public IEnumerable<Tuple<string, string>> InputParameters
        {
            get { return Model.InputParameters; }
        }

        public List<string> OutputParameters
        {
            get { return Model.OutputParameters; }
        }

        protected enum ResourceType
        {
            SmallIcon, LargeIcon
        }

        ///<summary>
        /// Small icon for class and method buttons.
        ///</summary>
        public BitmapSource SmallIcon
        {
            get
            {
                var name = GetResourceName(ResourceType.SmallIcon);
                BitmapSource icon = GetIcon(name + Configurations.SmallIconPostfix);

                // If there is no icon, use default.
                if (icon == null)
                    icon = LoadDefaultIcon(ResourceType.SmallIcon);

                return icon;
            }
        }

        ///<summary>
        /// Large icon for tooltips.
        ///</summary>
        public BitmapSource LargeIcon
        {
            get
            {
                var name = GetResourceName(ResourceType.LargeIcon);
                BitmapSource icon = GetIcon(name + Configurations.LargeIconPostfix);

                // If there is no icon, use default.
                if (icon == null)
                    icon = LoadDefaultIcon(ResourceType.LargeIcon);

                return icon;
            }
        }

        public ICommand ClickedCommand { get; private set; }

        private string GetResourceName(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.SmallIcon:
                case ResourceType.LargeIcon:
                    return Model.IconName;
            }

            throw new InvalidOperationException("Unhandled resourceType");
        }

        protected BitmapSource GetIcon(string fullNameOfIcon)
        {
            if (string.IsNullOrEmpty(Model.Assembly))
                return null;

            var iconRequest = new IconRequestEventArgs(Model.Assembly, fullNameOfIcon);
            OnRequestBitmapSource(iconRequest);

            return iconRequest.Icon;
        }

        protected virtual BitmapSource LoadDefaultIcon(ResourceType resourceType)
        {
            if (resourceType == ResourceType.LargeIcon)
                return null;

            var iconRequest = new IconRequestEventArgs(Configurations.DefaultAssembly,
                Configurations.DefaultIcon);
            OnRequestBitmapSource(iconRequest);

            return iconRequest.Icon;
        }
    }

    public class CustomNodeSearchElementViewModel : NodeSearchElementViewModel
    {
        private string path;

        public CustomNodeSearchElementViewModel(CustomNodeSearchElement element)
            : base(element)
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

        protected override BitmapSource LoadDefaultIcon(ResourceType resourceType)
        {
            string postfix = resourceType == ResourceType.SmallIcon ?
                Configurations.SmallIconPostfix : Configurations.LargeIconPostfix;

            return GetIcon(Configurations.DefaultCustomNodeIcon + postfix);
        }
    }
}
