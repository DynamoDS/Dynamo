using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dynamo.DSEngine;
using Dynamo.Search.Interfaces;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
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

        public string FullName
        {
            get { return Model.FullName; }
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
                var name = GetResourceName(ResourceType.SmallIcon, false);
                BitmapSource icon = GetIcon(name + Configurations.SmallIconPostfix);

                if (icon == null)
                {
                    // Get dis-ambiguous resource name and try again.
                    name = GetResourceName(ResourceType.SmallIcon, true);
                    icon = GetIcon(name + Configurations.SmallIconPostfix);

                    // If there is no icon, use default.
                    if (icon == null)
                        icon = LoadDefaultIcon(ResourceType.SmallIcon);
                }
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
                var name = GetResourceName(ResourceType.LargeIcon, false);
                BitmapSource icon = GetIcon(name + Configurations.LargeIconPostfix);

                if (icon == null)
                {
                    // Get dis-ambiguous resource name and try again.
                    name = GetResourceName(ResourceType.LargeIcon, true);
                    icon = GetIcon(name + Configurations.LargeIconPostfix);

                    // If there is no icon, use default.
                    if (icon == null)
                        icon = LoadDefaultIcon(ResourceType.LargeIcon);
                }
                return icon;
            }
        }

        public ICommand ClickedCommand { get; private set; }

        protected virtual string GetResourceName(
            ResourceType resourceType, bool disambiguate = false)
        {
            switch (resourceType)
            {
                case ResourceType.SmallIcon: return FullName;
                case ResourceType.LargeIcon: return FullName;
            }

            throw new InvalidOperationException("Unhandled resourceType");
        }

        protected BitmapSource GetIcon(string fullNameOfIcon)
        {
            if (string.IsNullOrEmpty(Model.Assembly))
                return null;

            var cust = LibraryCustomizationServices.GetForAssembly(Model.Assembly);
            BitmapSource icon = null;
            if (cust != null)
                icon = cust.LoadIconInternal(fullNameOfIcon);
            return icon;
        }

        protected virtual BitmapSource LoadDefaultIcon(ResourceType resourceType)
        {
            if (resourceType == ResourceType.LargeIcon)
                return null;

            var cust = LibraryCustomizationServices.GetForAssembly(Configurations.DefaultAssembly);
            return cust.LoadIconInternal(Configurations.DefaultIcon);
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
