using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.Graph.Nodes;

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
        private int dismissedWarnings = 0;
        private bool isInfo = false;
        private bool isEmptyList = false;
        private bool isNull = false;
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
                stateIsFunction = NodeModel.IsCustomFunction;
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
                
                isEmptyList = true;
                
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
                isNull = NodeModel.IsInErrorState;
                return isNull;
            }
            internal set => isNull = value;
        }
        #endregion

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
