﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Dynamo.Models;
using Dynamo.Search;
using System.Windows;

namespace Dynamo.Wpf.ViewModels
{
    public class NodeSearchElementViewModel : NotificationObject, ISearchEntryViewModel
    {
        private bool isSelected;
        private SearchViewModel searchViewModel;

        public bool IsTopResult
        {
            get;
            set;
        }

        public event RequestBitmapSourceHandler RequestBitmapSource;
        public void OnRequestBitmapSource(IconRequestEventArgs e)
        {
            if (RequestBitmapSource != null)
            {
                RequestBitmapSource(e);
            }
        }

        public ElementTypes ElementType
        {
            get { return Model.ElementType; }
        }

        public NodeSearchElementViewModel(NodeSearchElement element, SearchViewModel svm)
        {
            Model = element;
            searchViewModel = svm;

            Model.VisibilityChanged += ModelOnVisibilityChanged;
            if (searchViewModel != null)
                Clicked += searchViewModel.OnSearchElementClicked;
            ClickedCommand = new DelegateCommand(OnClicked);            
        }

        /// <summary>
        /// Creates a copy of NodeSearchElementViewModel.
        /// </summary>
        public NodeSearchElementViewModel(NodeSearchElementViewModel copyElement)
        {
            if (copyElement == null)
                throw new ArgumentNullException();

            Model = copyElement.Model;
            Clicked = copyElement.Clicked;
            RequestBitmapSource = copyElement.RequestBitmapSource;
            ClickedCommand = copyElement.ClickedCommand;
        }

        private void ModelOnVisibilityChanged()
        {           
            RaisePropertyChanged("Visibility");
        }

        public void Dispose()
        {
            if (searchViewModel != null)
                Clicked -= searchViewModel.OnSearchElementClicked;
        }

        public NodeSearchElement Model { get; set; }

        public string Name
        {
            get { return Model.Name; }
        }

        public string UserFriendlyName
        {
            get { return Model.UserFriendlyName; }
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
            get { return (!Model.Description.Equals(Dynamo.UI.Configurations.NoDescriptionAvailable)); }
        }

        public NodeCategoryViewModel Category { get; set; }

        public IEnumerable<Tuple<string, string>> InputParameters
        {
            get { return Model.InputParameters; }
        }

        public IEnumerable<string> OutputParameters
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
        public ImageSource SmallIcon
        {
            get
            {
                var name = GetResourceName(ResourceType.SmallIcon);
                ImageSource icon = GetIcon(name + Configurations.SmallIconPostfix);

                // If there is no icon, use default.
                if (icon == null)
                    icon = LoadDefaultIcon(ResourceType.SmallIcon);

                return icon;
            }
        }

        ///<summary>
        /// Large icon for tooltips.
        ///</summary>
        public ImageSource LargeIcon
        {
            get
            {
                var name = GetResourceName(ResourceType.LargeIcon);
                ImageSource icon = GetIcon(name + Configurations.LargeIconPostfix);

                // If there is no icon, use default.
                if (icon == null)
                    icon = LoadDefaultIcon(ResourceType.LargeIcon);

                return icon;
            }
        }

        internal Point Position { get; set; }

        public ICommand ClickedCommand { get; private set; }

        public event Action<NodeModel, Point> Clicked;
        protected virtual void OnClicked()
        {
            if (Clicked != null)
            {
                var nodeModel = Model.CreateNode();
                Clicked(nodeModel, Position);

                Dynamo.Services.InstrumentationLogger.LogPiiInfo("Search-NodeAdded", FullName);
            }
        }

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

        protected virtual ImageSource GetIcon(string fullNameOfIcon)
        {
            if (string.IsNullOrEmpty(Model.Assembly))
                return null;

            var iconRequest = new IconRequestEventArgs(Model.Assembly, fullNameOfIcon);
            OnRequestBitmapSource(iconRequest);

            return iconRequest.Icon;
        }

        protected virtual ImageSource LoadDefaultIcon(ResourceType resourceType)
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

        public CustomNodeSearchElementViewModel(CustomNodeSearchElement element, SearchViewModel svm)
            : base(element, svm)
        {
            Path = Model.Path;
        }

        public CustomNodeSearchElementViewModel(CustomNodeSearchElementViewModel copyElement)
            : base(copyElement)
        {
            Path = copyElement.Path;
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

        public new CustomNodeSearchElement Model
        {
            get { return base.Model as CustomNodeSearchElement; }
            set { base.Model = value; }
        }

        protected override ImageSource LoadDefaultIcon(ResourceType resourceType)
        {
            string postfix = resourceType == ResourceType.SmallIcon ?
                Configurations.SmallIconPostfix : Configurations.LargeIconPostfix;

            return GetIcon(Configurations.DefaultCustomNodeIcon + postfix);
        }

        protected override ImageSource GetIcon(string fullNameOfIcon)
        {
            IconRequestEventArgs iconRequest;

            // If there is no path, that means it's just created node.
            // Use DefaultAssembly to load icon.
            if (String.IsNullOrEmpty(Path))
            {
                iconRequest = new IconRequestEventArgs(Configurations.DefaultAssembly, fullNameOfIcon);
                OnRequestBitmapSource(iconRequest);
                return iconRequest.Icon;
            }

            string customizationPath = System.IO.Path.GetDirectoryName(Path);
            customizationPath = System.IO.Directory.GetParent(customizationPath).FullName;
            customizationPath = System.IO.Path.Combine(customizationPath, "bin", "Package.dll");

            iconRequest = new IconRequestEventArgs(customizationPath, fullNameOfIcon, false);
            OnRequestBitmapSource(iconRequest);

            if (iconRequest.Icon != null)
                return iconRequest.Icon;

            // If there is no icon inside of customization assembly,
            // try to find it in dynamo assembly.
            iconRequest = new IconRequestEventArgs(Configurations.DefaultAssembly, fullNameOfIcon);
            OnRequestBitmapSource(iconRequest);

            return iconRequest.Icon;
        }
    }
}
