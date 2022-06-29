﻿using System;
using System.Collections.Generic;
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
        
        public delegate void EventHandler(object sender, EventArgs args);
        public event EventHandler BubbleUpdate = delegate { };

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
        /// IsFrozen
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
                if (NodeModel.State == ElementState.Info)
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
                isNull = NodeModel.CachedValue != null && NodeModel.CachedValue.IsNull;
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
                dismissedAlertsCount = NodeModel.DismissedAlerts.Count;
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

            try
            {
                var list = mirrorData.GetElements();
                return !list.Any();
            }
            catch (Exception)
            {
                return false;
            }
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
                case "Name":
                    RaisePropertyChanged(nameof(Name));
                    break;
                case "IsVisible":
                    RaisePropertyChanged(nameof(StatusIsHidden));
                    break;
                case "IsSetAsInput":
                    RaisePropertyChanged(nameof(StateIsInput));
                    break;
                case "IsSetAsOutput":
                    RaisePropertyChanged(nameof(StateIsOutput));
                    break;
                case "IsInErrorState":
                    RaisePropertyChanged(nameof(IssuesHasError));
                    break;
                case "IsFrozen":
                    RaisePropertyChanged(nameof(StatusIsFrozen));
                    UpdateDownstreamNodes(nodeModel);
                    break;
                case "State":
                    RaisePropertyChanged(nameof(State));
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

    }
}
