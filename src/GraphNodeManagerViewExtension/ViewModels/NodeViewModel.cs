using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI.Commands;
using Dynamo.Selection;
using ProtoCore.Mirror;

namespace Dynamo.GraphNodeManager.ViewModels
{
    /// <summary>
    /// The ViewModel class to represent a Dynamo Node 
    /// </summary>
    public class NodeViewModel : NotificationObject
    {
        #region Private Properties
        private string name = String.Empty;
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
