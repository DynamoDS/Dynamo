using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using ProtoCore.Mirror;

namespace Dynamo.GraphNodeManager.ViewModels
{
    /// <summary>
    /// An extension class representing the original Dynamo NodeInfo NodeModel class
    /// </summary>
    public class NodeInfo
    {
        public string Message { get; set; }
        public ElementState State { get; set; }
        public int HashCode { get; set; }
        public string Index { get; set; }
        public bool Dismissed { get; set; }
    }
    /// <summary>
    /// The ViewModel class to represent a Dynamo Node 
    /// </summary>
    public class GridNodeViewModel : NotificationObject
    {
        #region Private Fields
        private string name = string.Empty;
        private bool stateIsInput = false;
        private bool stateIsOutput = false;
        private bool stateIsFunction = false;
        private bool statusIsHidden = false;
        private bool statusIsFrozen = false;
        private bool issuesHasWarning = false;
        private bool issuesHasError = false;
        private bool isInfo = false;
        private bool isEmptyList = false;
        private bool isNull = false;
        private int dismissedAlertsCount = 0;
        private bool isDummyNode = false;
        private bool isPartOfPackage;
        private bool outputIsSingleItem;
        private int infoCount = 0;
        private string infoIcon = string.Empty;
        private ElementState state;
        private ObservableCollection<NodeInfo> nodeInfos = new ObservableCollection<NodeInfo>();
        private string package;
        private string originalName = string.Empty;
        private string topLevelItemsNumber = string.Empty;
        private Guid nodeGuid;
        private bool isRenamed = false;
        private int topLevelItemsNumberAsInt = 0;

        public delegate void EventHandler(object sender, EventArgs args);
        public event EventHandler BubbleUpdate = delegate { };

        #endregion

        #region Public Properties
        /// <summary>
        /// Node Name
        /// </summary>
        public string Name
        {
            get
            {
                name = NodeModel?.Name;
                return name;
            }
            internal set
            {
                if (name == value) return;
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
        /// <summary>
        /// IsVisible
        /// </summary>
        public bool StatusIsHidden
        {
            get
            {
                statusIsHidden = !NodeModel.IsVisible;
                return statusIsHidden;
            }
            internal set
            {
                if (statusIsHidden == value) return;
                statusIsHidden = value;
                RaisePropertyChanged(nameof(StatusIsHidden));
            }
        }
        /// <summary>
        /// IsInputNode
        /// </summary>
        public bool StateIsInput
        {
            get
            {
                stateIsInput = NodeModel.IsSetAsInput;
                return stateIsInput;
            }
            internal set
            {
                if (stateIsInput == value) return;
                stateIsInput = value;
                RaisePropertyChanged(nameof(StateIsInput));
            }
        }
        /// <summary>
        /// IsOutputNode
        /// </summary>
        public bool StateIsOutput
        {
            get
            {
                stateIsOutput = NodeModel.IsSetAsOutput;
                return stateIsOutput;
            }
            internal set
            {
                if (stateIsOutput == value) return;
                stateIsOutput = value;
                RaisePropertyChanged(nameof(StateIsOutput));
            }
        }

        /// <summary>
        /// IsFrozen
        /// </summary>
        public bool StatusIsFrozen
        {
            get
            {
                statusIsFrozen = NodeModel.IsFrozen;
                return statusIsFrozen;
            }
            internal set
            {
                if (statusIsFrozen == value) return;
                statusIsFrozen = value;
                RaisePropertyChanged(nameof(StatusIsFrozen));
            }
        }

        /// <summary>
        /// IsFunction
        /// </summary>
        public bool StateIsFunction
        {
            get
            {
                stateIsFunction = IsNodeOutputFunction(NodeModel.CachedValue);
                return stateIsFunction;
            }
            internal set
            {
                if (stateIsFunction == value) return;
                stateIsFunction = value;
                RaisePropertyChanged(nameof(StateIsFunction));
            }
        }

        /// <summary>
        /// Returns True if the output of the node is a single item
        /// </summary>
        public bool IsOutputSingleItem
        {
            get
            {
                return IsNodeOutputSingeItem(NodeModel.CachedValue);
            }
            internal set
            {
                if (outputIsSingleItem == value) return;
                outputIsSingleItem = value;
                RaisePropertyChanged(nameof(IsOutputSingleItem));
            }
        }

        /// <summary>
        /// Returns the number of top-level items in list
        /// </summary>
        public string TopLevelItemsNumber
        {
            get
            {
                return GetTopLevelItemsNumber(NodeModel.CachedValue);
            }
            internal set
            {
                if (topLevelItemsNumber == value) return;
                topLevelItemsNumber = value;
                RaisePropertyChanged(nameof(TopLevelItemsNumber));
            }
        }

        /// <summary>
        /// Returns the number of top-level items in list
        /// </summary>
        public int TopLevelItemsNumberAsInt
        {
            get
            {
                return GetTopLevelItemsNumberAsInt(NodeModel.CachedValue);
            }
            internal set
            {
                if (topLevelItemsNumberAsInt == value) return;
                topLevelItemsNumberAsInt = value;
                RaisePropertyChanged(nameof(TopLevelItemsNumberAsInt));
            }
        }

        /// <summary>
        /// Node Has Warnings 
        /// </summary>
        public bool IssuesHasWarning
        {
            get
            {
                if (NodeModel.State == ElementState.Warning || NodeModel.State == ElementState.PersistentWarning)
                {
                    issuesHasWarning = true;
                }
                return issuesHasWarning;
            }
            internal set
            {
                if (issuesHasWarning == value) return;
                issuesHasWarning = value;
                RaisePropertyChanged(nameof(IssuesHasWarning));
            }
        }

        /// <summary>
        /// Node Has Errors 
        /// </summary>
        public bool IssuesHasError
        {
            get
            {
                issuesHasError = NodeModel.IsInErrorState;
                return issuesHasError;
            }
            internal set
            {
                if (issuesHasError == value) return;
                issuesHasError = value;
                RaisePropertyChanged(nameof(IssuesHasError));
            }
        }


        /// <summary>
        /// Node Is Info Node
        /// </summary>
        public bool IsInfo
        {
            get
            {
                if (NodeModel.State == ElementState.Info || NodeModel.State == ElementState.PersistentInfo)
                {
                    isInfo = true;
                }
                return isInfo;
            }
            internal set
            {
                if (isInfo == value) return;
                isInfo = value;
                RaisePropertyChanged(nameof(IsInfo));
            }
        }

        /// <summary>
        /// Node returns an empty list
        /// </summary>
        public bool IsEmptyList
        {
            get
            {
                isEmptyList = IsNodeEmptyList(NodeModel.CachedValue);

                return isEmptyList;
            }
            internal set
            {
                if (isEmptyList == value) return;
                isEmptyList = value;
                RaisePropertyChanged(nameof(IsEmptyList));
            }
        }

        /// <summary>
        /// Node returns null
        /// </summary>
        public bool IsNull
        {
            get
            {
                isNull = IsNodeNull(NodeModel.CachedValue);
                return isNull;
            }
            internal set
            {
                if (isNull == value) return;
                isNull = value;
                RaisePropertyChanged(nameof(IsNull));
            }
        }

        /// <summary>
        /// Number of dismissed alerts - Warnings/Errors in a node
        /// </summary>
        public int DismissedAlertsCount
        {
            get
            {
                dismissedAlertsCount = NodeModel.DismissedAlertsCount;
                return dismissedAlertsCount;
            }
            internal set
            {
                if (dismissedAlertsCount == value) return;
                dismissedAlertsCount = value;
                RaisePropertyChanged(nameof(DismissedAlertsCount));
            }
        }
        /// <summary>
        /// If the node is broken or unreferenced
        /// </summary>
        public bool IsDummyNode
        {
            get
            {
                isDummyNode = (NodeModel as DummyNode) != null;
                return isDummyNode;
            }
            internal set
            {
                if (isDummyNode == value) return;
                isDummyNode = value;
                RaisePropertyChanged(nameof(IsDummyNode));
            }
        }
        /// <summary>
        /// Test The Number of Info/Warning/Error messages 
        /// </summary>
        /// <returns></returns>
        public int InfoCount
        {
            get
            {
                infoCount = NodeModel.NodeInfos.Count;
                return infoCount;
            }
            internal set
            {
                if (infoCount == value) return;
                infoCount = value;
                RaisePropertyChanged(nameof(InfoCount));
            }
        }
        /// <summary>
        /// Checks if the Node has been Renamed after its creation
        /// </summary>
        public bool IsRenamed
        {
            get
            {
                isRenamed = NodeModel.GetOriginalName() != NodeModel.Name;
                return isRenamed;
            }
            internal set
            {
                if (isRenamed == value) return;
                isRenamed = value;
                RaisePropertyChanged(nameof(IsRenamed));
            }
        }
        /// <summary>
        /// The original name of the node
        /// </summary>
        public string OriginalName
        {
            get
            {
                string originalName = NodeModel.GetOriginalName();
                return originalName;
            }
            internal set
            {
                if (originalName == value) return;
                originalName = value;
                RaisePropertyChanged(nameof(OriginalName));
            }
        }
        /// <summary>
        /// The correct icon for the Info Bubble
        /// </summary>
        public string InfoIcon
        {
            get
            {
                switch (NodeModel.State)
                {
                    case ElementState.Info:
                    case ElementState.PersistentInfo:
                        infoIcon = "/GraphNodeManagerViewExtension;component/Images/Info.png";
                        break;
                    case ElementState.Warning:
                    case ElementState.PersistentWarning:
                        infoIcon = "/GraphNodeManagerViewExtension;component/Images/Alert.png";
                        break;
                    case ElementState.Error:
                        infoIcon = "/GraphNodeManagerViewExtension;component/Images/Error.png";
                        break;
                    default:
                        infoIcon = string.Empty;
                        break;
                }
                return infoIcon;
            }
            internal set => infoIcon = value;
        }
        /// <summary>
        /// The state of the Node
        /// </summary>
        public ElementState State
        {
            get
            {
                state = NodeModel.State;
                return state;
            }
            internal set => state = value;

        }
        /// <summary>
        /// Contains Node Package information
        /// </summary>
        public string Package
        {
            get
            {
                package = NodeModel.Category;
                return package;
            }
            internal set => package = value;
        }
        /// <summary>
        /// The GUID of the Node
        /// </summary>
        public Guid NodeGuid
        {
            get
            {
                nodeGuid = NodeModel.GUID;
                return nodeGuid;
            }
            internal set => nodeGuid = value;
        }
        #endregion

        /// <summary>
        ///  Returns true only if the node contains ANY (nested) empty lists 
        /// </summary>
        /// <param name="mirrorData"></param>
        /// <returns></returns>
        private bool IsNodeEmptyList(MirrorData mirrorData)
        {
            if (mirrorData == null) return false;
            if (mirrorData.IsCollection)
            {
                try
                {
                    var list = mirrorData.GetElements();
                    if (!list.ToList().Any()) return true;

                    foreach (var nested in list)
                    {
                        if (IsNodeEmptyList(nested))
                            return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the output of the node is a single item
        /// </summary>
        public bool IsNodeOutputSingeItem(MirrorData mirrorData)
        {

            if (mirrorData == null) return false;
            if (!mirrorData.IsCollection)
            {
                try
                {
                    var output = mirrorData.Data;

                    if (output is object) return true;
                    return false;
                }
                catch (Exception)
                { return false; }
            }
            return false;
        }

        /// <summary>
        /// Returns the number of list items in the top level of the node output
        /// </summary>
        public string GetTopLevelItemsNumber(MirrorData mirrorData)
        {
            if (mirrorData == null) return string.Empty;
            if (mirrorData.IsCollection)
            {
                try
                {
                    var list = mirrorData.GetElements();
                    if (list.ToList().Count() == 1) return "[1]";
                    else if (list.ToList().Count() == 2) return "[2]";
                    else if (list.ToList().Count() == 3) return "[3]";
                    else if (list.ToList().Count() == 4) return "[4]";
                    else if (list.ToList().Count() == 5) return "[5]";
                    else if (list.ToList().Count() == 6) return "[6]";
                    else if (list.ToList().Count() == 7) return "[7]";
                    else if (list.ToList().Count() == 8) return "[8]";
                    else if (list.ToList().Count() == 9) return "[9]";
                    else if (list.ToList().Count() > 9) return "[9+]";
                    else return string.Empty;

                }
                catch (Exception) { return string.Empty; }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the number of list items in the top level of the node output as integer.
        /// </summary>
        public int GetTopLevelItemsNumberAsInt(MirrorData mirrorData)
        {
            if (mirrorData == null) return 0;
            if (mirrorData.IsCollection)
            {
                try
                {
                    return mirrorData.GetElements().ToList().Count();
                }
                catch (Exception) { return 0; }
            }
            return 0;
        }

        /// <summary>
        /// Returns true if the Node contains ANY (nested) null values
        /// </summary>
        /// <param name="mirrorData"></param>
        /// <returns></returns>
        private bool IsNodeNull(MirrorData mirrorData)
        {
            if (mirrorData == null) return false;
            if (mirrorData.IsCollection)
            {
                try
                {
                    var list = mirrorData.GetElements();
                    foreach (var nested in list)
                    {
                        if (IsNodeNull(nested))
                            return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            if (mirrorData.IsNull) return true;
            return false;
        }

        /// <summary>
        /// Returns true only if the mirrorData class is not null and its name is set to Function
        /// </summary>
        /// <param name="mirrorData"></param>
        /// <returns></returns>
        private bool IsNodeOutputFunction(MirrorData mirrorData)
        {
            if (mirrorData == null || mirrorData.Class == null) return false;

            try
            {
                if (mirrorData.IsFunction) return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The actual Info/Warning/Error text message
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<NodeInfo> NodeInfos
        {
            get
            {
                nodeInfos.Clear();
                PopulateNodeInfos();
                return nodeInfos;
            }
            internal set => nodeInfos = value;
        }

        /// <summary>
        /// Populate NodeInfos for this Node
        /// </summary>
        private void PopulateNodeInfos()
        {
            int i = 0;
            NodeModel.NodeInfos.ForEach(ni => nodeInfos.Add(new NodeInfo() { Message = GetNodeMessage(ni.Message), Index = $"{++i}/{NodeModel.NodeInfos.Count}", State = ni.State, HashCode = ni.GetHashCode(), Dismissed = IsNodeMessageDismissed(ni.Message) } ));
        }

        /// <summary>
        /// If the message is included in the DismissedAlerts, add the '(dismissed)' prefix to the info message
        /// TODO It is prone to errors, as multiple infos can have the same message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetNodeMessage(string message)
        {
            return NodeModel.DismissedAlerts.Contains(message) ? $"{message} (dismissed)" : message;
        }

        private bool IsNodeMessageDismissed(string message)
        {
            return NodeModel.DismissedAlerts.Contains(message);
        }

        #region Setup and Constructors
        internal NodeModel NodeModel { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        public GridNodeViewModel(NodeModel node)
        {
            NodeModel = node;

            NodeModel.Modified += NodeModel_Modified;
            NodeModel.PropertyChanged += NodeModel_PropertyChanged;
        }

        private void NodeModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            EvaluateNode(sender as NodeModel, e.PropertyName);
        }

        private void NodeModel_Modified(NodeModel nodeModel)
        {
            EvaluateNode(nodeModel);
        }

        /// <summary>
        /// In case a Node has been modified in any way, update the affected property 
        /// </summary>
        /// <param name="nodeModel"></param>
        /// <param name="propertyName"></param>
        private void EvaluateNode(NodeModel nodeModel, string propertyName = "Modified")
        {
            switch (propertyName)
            {
                case nameof(nodeModel.Name):
                    RaisePropertyChanged(nameof(Name));
                    RaisePropertyChanged(nameof(IsRenamed));
                    break;
                case nameof(nodeModel.IsVisible):
                    RaisePropertyChanged(nameof(StatusIsHidden));
                    break;
                case nameof(nodeModel.IsSetAsInput):
                    RaisePropertyChanged(nameof(StateIsInput));
                    break;
                case nameof(nodeModel.IsSetAsOutput):
                    RaisePropertyChanged(nameof(StateIsOutput));
                    break;
                case nameof(nodeModel.IsInErrorState):
                    RaisePropertyChanged(nameof(IssuesHasError));
                    break;
                case nameof(nodeModel.IsFrozen):
                    RaisePropertyChanged(nameof(StatusIsFrozen));
                    UpdateDownstreamNodes(nodeModel);
                    break;
                case nameof(nodeModel.State):
                    RaisePropertyChanged(nameof(State));
                    break;
                case nameof(nodeModel.DismissedAlertsCount):
                    RaisePropertyChanged(nameof(DismissedAlertsCount));
                    RaisePropertyChanged(nameof(NodeInfos));
                    break;
            }
        }

        /// <summary>
        /// In case an Input Node is frozen, only the input node triggers RaisePropertyChange
        /// All the downstream Nodes won't RaisePropertyChange for IsFrozen (although they will report correctly that they are)
        /// We need to manually update the view in this case
        /// </summary>
        /// <param name="nodeModel"></param>
        private void UpdateDownstreamNodes(NodeModel nodeModel)
        {
            if (nodeModel.OutputNodes.Count > 0)
            {
                BubbleUpdate(nodeModel, new EventArgs());
            }
        }

        /// <summary>
        /// Detach from all event handlers
        /// </summary>
        public void Dispose()
        {
            NodeModel.Modified -= NodeModel_Modified;
            NodeModel.PropertyChanged -= NodeModel_PropertyChanged;
        }

        #endregion

        #region Sort

        /// <summary>
        /// Provides sort keys for the Type, State, Issues, and Output columns, calculated based on each column's specific attributes. 
        /// Each SortKey uses a combination of count (number of relevant attributes) and priority (relative importance).
        /// </summary>
        private string CalculateSortKey((bool condition, int priority)[] conditions, int priorityFormatLength)
        {
            int count = conditions.Count(c => c.condition);
            int priority = conditions.Where(c => c.condition).Sum(c => c.priority);
            return $"{count:D1}-{priority.ToString($"D{priorityFormatLength}")}";
        }

        public string TypeSortKey => CalculateSortKey(
            new (bool, int)[]
            {
                (StateIsFunction, 100),
                (StateIsInput, 10),
                (StateIsOutput, 1)
            }, 3);

        public string StateSortKey => CalculateSortKey(
            new (bool, int)[]
            {
                (StatusIsFrozen, 100),
                (StatusIsHidden, 10),
                (IsInfo, 1)
            }, 3);

        public string IssueSortKey => CalculateSortKey(
            new (bool, int)[]
            {
                (IssuesHasError, 10000),
                (IsDummyNode, 1000),
                (IssuesHasWarning, 100),
                (DismissedAlertsCount > 0, DismissedAlertsCount)
            }, 5);

        public string OutputSortKey => CalculateSortKey(
            new (bool, int)[]
            {
                (IsOutputSingleItem || TopLevelItemsNumberAsInt > 0, 100 * TopLevelItemsNumberAsInt),
                (IsEmptyList, 10),
                (IsNull, 1)
            }, 3);

        #endregion
    }
}
