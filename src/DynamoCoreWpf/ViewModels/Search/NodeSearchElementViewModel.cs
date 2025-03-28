using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using FontAwesome5;
using Prism.Commands;

namespace Dynamo.Wpf.ViewModels
{
    public class NodeSearchElementViewModel : ViewModelBase, ISearchEntryViewModel
    {
        private Dictionary<SearchElementGroup, EFontAwesomeIcon> FontAwesomeDict;

        private bool isSelected;
        private SearchViewModel searchViewModel;
        private int spacingBetweenNodes = 50;
        private int spacingforHigherWidthNodes = 450;

        /// <summary>
        /// Machine Learning related info
        /// </summary>
        public AutoCompletionNodeMachineLearningInfo AutoCompletionNodeMachineLearningInfo { get; set; } = new AutoCompletionNodeMachineLearningInfo();

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
            {
                Clicked += searchViewModel.OnSearchElementClicked;
            }
            ClickedCommand = new DelegateCommand(OnClicked);
            CreateAndConnectCommand = new DelegateCommand<PortModel>(CreateAndConnectToPort, CanCreateAndConnectToPort);

            LoadFonts();
        }

        private void ModelOnVisibilityChanged()
        {           
            RaisePropertyChanged("Visibility");
        }

        public override void Dispose()
        {
            Model.VisibilityChanged -= ModelOnVisibilityChanged;
            if (searchViewModel != null)
            {
                if (RequestBitmapSource != null)
                {
                    RequestBitmapSource -= searchViewModel.SearchViewModelRequestBitmapSource;
                }
                Clicked -= searchViewModel.OnSearchElementClicked;
                searchViewModel = null;
            }
            base.Dispose();
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
            FontAwesomeDict = new Dictionary<SearchElementGroup, EFontAwesomeIcon>();

            FontAwesomeDict.Add(SearchElementGroup.Create, EFontAwesomeIcon.Solid_Plus);
            FontAwesomeDict.Add(SearchElementGroup.Action, EFontAwesomeIcon.Solid_Bolt);
            FontAwesomeDict.Add(SearchElementGroup.Query, EFontAwesomeIcon.Solid_Question);
        }

        /// <summary>
        /// Indicates group icon, e.g. create - plus icon.
        /// </summary>
        public EFontAwesomeIcon GroupIconName
        {
            get
            {
                return FontAwesomeDict.ContainsKey(Group) ? FontAwesomeDict[Group] : EFontAwesomeIcon.None;
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

        /// <summary>
        /// Create the search element as node and connect to target port
        /// </summary>
        public ICommand CreateAndConnectCommand { get; }

        /// <summary>
        /// Create new node for search element, connect to port and place using graph auto layout.
        /// </summary>
        /// <param name="parameter">Port model to connect to</param>
        protected virtual void CreateAndConnectToPort(object parameter)
        {
            var portModel = parameter as PortModel;

            if (portModel is null) return;

            var dynamoViewModel = searchViewModel.dynamoViewModel;

            // Initialize a new undo action group before calling 
            // node CreateAndConnect and AutoLayout commands.
            IDisposable undoRecorderGroup = dynamoViewModel.CurrentSpace.UndoRecorder.BeginActionGroup();


            var initialNode = portModel.Owner;
            var initialNodeVm = dynamoViewModel.CurrentSpaceViewModel.Nodes.FirstOrDefault(x => x.Id == initialNode.GUID);
            var id = Guid.NewGuid();

            var adjustedX = initialNodeVm.X;
            var adjustedY = initialNodeVm.Y;

            var createAsDownStreamNode = portModel.PortType == PortType.Output;

            IEnumerable<NodeModel> misplacedNodes;

            // Placing the new node based on which port it is connecting to.
            if (createAsDownStreamNode)
            {
                // Placing the new node to the right of initial node
                adjustedX += initialNode.Width + spacingBetweenNodes;

                // Create a new node based on node creation name and connection ports
                dynamoViewModel.ExecuteCommand(new DynamoModel.CreateAndConnectNodeCommand(id, initialNode.GUID,
                    Model.CreationName, portModel.Index, Model.AutoCompletionNodeElementInfo.PortToConnect, adjustedX, adjustedY, createAsDownStreamNode, false, true));

                //Collect all output nodes as we need to perform Auto layout on only the output nodes
                misplacedNodes = initialNode.OutputNodes.Values.Where(x => x != null).SelectMany(y => y.Select(z => z.Item2));                
            }
            else
            {
                // Placing the new node to the left of initial node
                adjustedX -= initialNode.Width + spacingBetweenNodes;

                // If the new node is a slider input node, adjust the position on X-axis to compensate for higher width of the slider node.
                if (Model.CreationName.Contains("Slider")) 
                {
                    adjustedX -= spacingforHigherWidthNodes;
                }

                // Create a new node based on node creation name and connection ports
                dynamoViewModel.ExecuteCommand(new DynamoModel.CreateAndConnectNodeCommand(id, initialNode.GUID,
                      Model.CreationName, 0, portModel.Index, adjustedX, adjustedY, createAsDownStreamNode, false, true));

                //Collect all input nodes as we need to perform Auto layout on only the input nodes
                misplacedNodes = initialNode.InputNodes.Values.Where(x => x != null).Select(y => y.Item2);

                //Add the node that is about to be replaced in the selection in order to be considered while AutoLayout
                NodeModel prevNode = portModel?.Connectors?.FirstOrDefault()?.Start?.Owner; //upstream we only have one node to check
                if (prevNode != null)
                {
                    misplacedNodes.Append(prevNode);
                }
            }

            Action ensureUndoGroup = () => undoRecorderGroup?.Dispose();

            NodeAutoCompleteUtilities.PostAutoLayoutNodes(dynamoViewModel.CurrentSpace, initialNode, misplacedNodes, false, true, ensureUndoGroup);
        }

        protected virtual bool CanCreateAndConnectToPort(object parameter)
        {
            // Do not auto connect code block node since default code block node do not have output port
            if (Model.CreationName.Contains("Code Block")) return false;
            
            // Avoid triggering auto complete for an already connected input.
            PortModel portModel = parameter as PortModel;
            return portModel?.PortType != PortType.Input || portModel?.Connectors?.FirstOrDefault()?.Start?.Owner is null;
        }

        public ICommand ClickedCommand { get; private set; }

        public event Action<NodeModel, Point> Clicked;
        protected virtual void OnClicked()
        {
            if (Clicked != null)
            {
                // Try to create the node based on the search element from in-Canvas search
                // The node creation can fail if the node constructor dependencies are not found or other reasons.
                // This is a best effort to create the node and log the error both in console and toast notification if it fails.
                try
                {
                    var newNode = Model.CreateNode();

                    // check to make sure a custom node cannot be added to its own workspace.
                    var dynamoViewModel = searchViewModel.dynamoViewModel;
                    var homeworkspace = dynamoViewModel.HomeSpaceViewModel;

                    if (newNode.IsCustomFunction && dynamoViewModel.CurrentSpace is CustomNodeWorkspaceModel customNodeWorkspaceModel)
                    {
                        var nodeGuid = Guid.Parse(newNode.CreationName);

                        if (nodeGuid.Equals(customNodeWorkspaceModel.CustomNodeId))
                        {
                            if (!customNodeWorkspaceModel.Nodes.Any(n => n.GetOriginalName().Contains("ScopeIf")))
                            {
                                dynamoViewModel.MainGuideManager.CreateRealTimeInfoWindow(Properties.Resources.CannotAddNodeToWorkspace);
                                return;
                            }
                            else
                            {
                                homeworkspace.RunSettingsViewModel.Model.RunType = RunType.Manual;
                            }
                        }
                    }

                    Clicked(newNode, Position);
                }
                catch (Exception ex)
                {
                    searchViewModel.dynamoViewModel.Model.Logger.Log("Failed to create node from search element: " + Model.Name + "\n" + ex.Message);
                    searchViewModel.dynamoViewModel.MainGuideManager.CreateRealTimeInfoWindow(Wpf.Properties.Resources.NodeInCanvasSearchCreationError + Model.Name, true);
                }
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
