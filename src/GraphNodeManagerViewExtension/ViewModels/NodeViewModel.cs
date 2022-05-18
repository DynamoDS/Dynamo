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
    public class NodeViewModel : NotificationObject
    {
        #region Private Properties
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
        private int infoCount = 0;
        private string infoIcon = string.Empty;
        private ElementState state;
        private ObservableCollection<NodeInfo> nodeInfos = new ObservableCollection<NodeInfo>();
        private string package;
        private Guid nodeGuid;
        #endregion

        #region Public Fields
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
            internal set => name = value;
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
            internal set => statusIsHidden = value;
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
            internal set => stateIsInput = value;
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
            internal set => stateIsInput = value;
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
            internal set => statusIsFrozen = value;
        }

        /// <summary>
        /// IsFrozen
        /// </summary>
        public bool StateIsFunction
        {
            get
            {
                stateIsFunction = IsNodeOutputFunction(NodeModel.CachedValue);
                return stateIsFunction;
            }
            internal set => stateIsFunction = value;
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
            internal set => issuesHasWarning = value;
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
            internal set => issuesHasError = value;
        }


        /// <summary>
        /// Node Is Info Node
        /// </summary>
        public bool IsInfo
        {
            get
            {
                if (NodeModel.State == ElementState.Info)
                {
                    isInfo = true;
                }
                return isInfo;
            }
            internal set => isInfo = value;
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
            internal set => isEmptyList = value;
        }

        /// <summary>
        /// Node returns null
        /// </summary>
        public bool IsNull
        {
            get
            {
                isNull = NodeModel.CachedValue != null && NodeModel.CachedValue.IsNull;
                return isNull;
            }
            internal set => isNull = value;
        }
        /// <summary>
        /// Number of dismissed alerts - Warnings/Errors in a node
        /// </summary>
        public int DismissedAlertsCount
        {
            get
            {
                dismissedAlertsCount = NodeModel.DismissedAlerts.Count;
                return dismissedAlertsCount;
            }
            internal set => dismissedAlertsCount = value;
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
            internal set => isDummyNode = value;
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
            internal set => infoCount = value;
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
        ///  Returns true only if it IsCollection and has no elements inside
        /// </summary>
        /// <param name="mirrorData"></param>
        /// <returns></returns>
        private bool IsNodeEmptyList(MirrorData mirrorData)
        {
            if (mirrorData == null || !mirrorData.IsCollection) return false;

            var list = mirrorData.GetElements();
            return !list.Any();
        }

        /// <summary>
        /// Returns true only if the mirrorData class is not null and its name is set to Function
        /// </summary>
        /// <param name="mirrorData"></param>
        /// <returns></returns>
        private bool IsNodeOutputFunction(MirrorData mirrorData)
        {
            if (mirrorData == null || mirrorData.Class == null) return false;

            if (string.Equals(mirrorData.Class.Name, "Function")) return true;
            return false;
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
            NodeModel.NodeInfos.ForEach(ni => nodeInfos.Add(new NodeInfo() { Message = GetNodeMessage(ni.Message), Index = $"{++i}/{NodeModel.NodeInfos.Count}", State = ni.State, HashCode = ni.GetHashCode() }));
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

        #region Setup and Constructors
        internal NodeModel NodeModel { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        public NodeViewModel(NodeModel node)
        {
            NodeModel = node;
        }
        
        #endregion

    }
}
