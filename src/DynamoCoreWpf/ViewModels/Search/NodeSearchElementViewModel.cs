using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using FontAwesome.WPF;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    public class NodeSearchElementViewModel : NotificationObject, ISearchEntryViewModel
    {
        private Dictionary<SearchElementGroup, FontAwesomeIcon> FontAwesomeDict;

        private bool isSelected;
        private SearchViewModel searchViewModel;

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

            LoadFonts();
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

        public string FullName
        {
            get { return Model.FullName; }
        }

        public string Assembly
        {
            get { return Model.Assembly; }
        }

        public string Parameters
        {
            get { return Model.Parameters; }
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

        /// <summary>
        /// Indicates class from which node came, e.g. Point - class, ByCoordinates - node.
        /// </summary>
        public string Class
        {
            get
            {
                return Model.Categories.Count > 1 ? Model.Categories.Last() : String.Empty;
            }
        }

        /// <summary>
        /// Indicates category from which node came, e.g. Geometry - category, ByCoordinates - node.
        /// </summary>
        public string Category
        {
            get
            {
                return Model.Categories.FirstOrDefault();
            }
        }

        /// <summary>
        /// Indicates node's group. It can be create, action or query. 
        /// </summary>
        public SearchElementGroup Group
        {
            get
            {
                return Model.Group;
            }
        }

        /// <summary>
        /// Loads font awesome icons for node's groups, e.g. create - plus icon.
        /// </summary>
        private void LoadFonts()
        {
            FontAwesomeDict = new Dictionary<SearchElementGroup, FontAwesomeIcon>();

            FontAwesomeDict.Add(SearchElementGroup.Create, FontAwesomeIcon.Plus);
            FontAwesomeDict.Add(SearchElementGroup.Action, FontAwesomeIcon.LightningBolt);
            FontAwesomeDict.Add(SearchElementGroup.Query, FontAwesomeIcon.Question);
        }

        /// <summary>
        /// Indicates group icon, e.g. create - plus icon.
        /// </summary>
        public FontAwesomeIcon GroupIconName
        {
            get
            {
                return FontAwesomeDict.ContainsKey(Group) ? FontAwesomeDict[Group] : FontAwesomeIcon.None;
            }
        }

        public bool HasDescription
        {
            get { return (!Model.Description.Equals(Configurations.NoDescriptionAvailable)); }
        }

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

                Analytics.LogPiiInfo("Search-NodeAdded", FullName);
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
