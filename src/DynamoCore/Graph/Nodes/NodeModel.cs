using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Migration;
using Dynamo.Scheduler;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Visualization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using String = System.String;
using StringNode = ProtoCore.AST.AssociativeAST.StringNode;

namespace Dynamo.Graph.Nodes
{
    public abstract class NodeModel : ModelBase, IRenderPackageSource<NodeModel>, IDisposable
    {
        #region private members

        private LacingStrategy argumentLacing = LacingStrategy.Auto;
        private bool displayLabels;
        private bool isVisible;
        private bool isSetAsInput = false;
        private bool isSetAsOutput = false;
        private bool canUpdatePeriodically;
        private string name;
        private ElementState state;
        private string toolTipText = "";
        private string description;
        private string persistentWarning = "";
        private bool areInputPortsRegistered;
        private bool areOutputPortsRegistered;

        ///A flag indicating whether the node has been explicitly frozen.
        internal bool isFrozenExplicitly;

        /// <summary>
        /// The cached value of this node. The cachedValue object is protected by the cachedValueMutex
        /// as it may be accessed from multiple threads concurrently.
        ///
        /// However, generally access to the cachedValue property should be protected by usage
        /// of the Scheduler.
        /// </summary>
        private MirrorData cachedValue;
        private readonly object cachedValueMutex = new object();

        // Input and output port related data members.
        private ObservableCollection<PortModel> inPorts = new ObservableCollection<PortModel>();
        private ObservableCollection<PortModel> outPorts = new ObservableCollection<PortModel>();

        #endregion

        #region public members

        private readonly Dictionary<int, Tuple<int, NodeModel>> inputNodes;
        private readonly Dictionary<int, HashSet<Tuple<int, NodeModel>>> outputNodes;

        /// <summary>
        /// The unique name that was created the node by
        /// </summary>
        [JsonIgnore]
        public virtual string CreationName
        {
            get
            {
                return getNameFromNodeNameAttribute();
            }
        }

        /// <summary>
        /// This property queries all the Upstream Nodes  for a given node, ONLY after the graph is loaded.
        /// This property is computed in ComputeUpstreamOnDownstreamNodes function
        /// </summary>
        internal HashSet<NodeModel> UpstreamCache = new HashSet<NodeModel>();

        /// <summary>
        /// The NodeType property provides a name which maps to the
        /// server type for the node. This property should only be
        /// used for serialization.
        /// </summary>
        public virtual string NodeType
        {
            get
            {
                return "ExtensionNode";
            }
        }

        #endregion

        #region events

        //TODO(Steve): Model should not have to worry about UI thread synchronization -- MAGN-5709

        /// <summary>
        ///     Called by nodes for behavior that they want to dispatch on the UI thread
        ///     Triggers event to be received by the UI. If no UI exists, behavior will not be executed.
        /// </summary>
        /// <param name="a"></param>
        public void DispatchOnUIThread(Action a)
        {
            OnDispatchedToUI(this, new UIDispatcherEventArgs(a));
        }

        private void OnDispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            if (DispatchedToUI != null)
                DispatchedToUI(sender, e);
        }

        internal event DispatchedToUIThreadHandler DispatchedToUI;

        /// <summary>
        /// Event triggered when a port is connected.
        /// </summary>
        public event Action<PortModel, ConnectorModel> PortConnected;

        /// <summary>
        /// Event triggered when a port is disconnected.
        /// </summary>
        public event Action<PortModel> PortDisconnected;

        /// <summary>
        /// Event triggered before a node is executed.
        /// Note: This event will only be triggered when profiling is active.
        /// </summary>
        public event Action<NodeModel> NodeExecutionBegin;

        internal void OnNodeExecutionBegin()
        {
            NodeExecutionBegin?.Invoke(this);
        }

        /// <summary>
        /// Event triggered after a node is executed.
        /// Note: This event will only be triggered when profiling is active.
        /// </summary>
        public event Action<NodeModel> NodeExecutionEnd;

        internal void OnNodeExecutionEnd()
        {
            NodeExecutionEnd?.Invoke(this);
        }

        #endregion

        #region public properties
        /// <summary>
        /// Id for this node, must be unique within the graph.
        /// </summary>
        [JsonProperty("Id")]
        [JsonConverter(typeof(IdToGuidConverter))]
        public override Guid GUID
        {
            get
            {
                return base.GUID;
            }
            set
            {
                base.GUID = value;
            }
        }

        /// <summary>
        ///     All of the connectors entering and exiting the NodeModel.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ConnectorModel> AllConnectors
        {
            get
            {
                return inPorts.Concat(outPorts).SelectMany(port => port.Connectors);
            }
        }

        /// <summary>
        ///     Returns whether this node represents a built-in or custom function.
        /// </summary>
        [JsonIgnore]
        public bool IsCustomFunction
        {
            get { return this is Function; }
        }

        /// <summary>
        ///     Returns whether the node is to be included in visualizations.
        /// </summary>
        [JsonIgnore]
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }

            private set // Private setter, see "ArgumentLacing" for details.
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        /// <summary>
        /// Input nodes are used in Customizer and Presets. Input nodes can be numbers, number sliders,
        /// strings, bool, code blocks and custom nodes, which don't specify path. This property
        /// is true for nodes that are potential inputs for Customizers and Presets.
        /// </summary>
        [JsonIgnore]
        public virtual bool IsInputNode
        {
            get
            {

                return !inPorts.Any() && !IsCustomFunction;
            }
        }

        /// <summary>
        /// This property is user-controllable via a checkbox and is set to true when a user wishes to include
        /// this node in a Customizer as an interactive control.
        /// </summary>
        [JsonIgnore]
        public bool IsSetAsInput
        {
            get
            {
                if (!IsInputNode)
                    return false;

                return isSetAsInput;
            }

            set
            {
                if (isSetAsInput != value)
                {
                    isSetAsInput = value;
                    RaisePropertyChanged(nameof(IsSetAsInput));
                }
            }
        }

        /// <summary>
        /// Output nodes are used by applications that consume graphs outside of Dynamo such as Optioneering, Optimization, 
        /// and Dynamo Player. Output nodes can be any node that returns a single output or a dictionary. Code block nodes and
        /// Custom nodes are specifically excluded at this time even though they can return a single output for sake of clarity. 
        /// </summary>
        [JsonIgnore]
        public virtual bool IsOutputNode
        {
            get
            {
                return !IsCustomFunction;
            }
        }

        /// <summary>
        /// This property is user-controllable via a checkbox and is set to true when a user wishes to include
        /// this node in the OutputData block of the Dyn file.
        /// </summary>
        [JsonIgnore]
        public bool IsSetAsOutput
        {
            get
            {
                if (!IsOutputNode)
                    return false;

                return isSetAsOutput;
            }

            set
            {
                if (isSetAsOutput != value)
                {
                    isSetAsOutput = value;
                    RaisePropertyChanged(nameof(IsSetAsOutput));
                }
            }
        }

        /// <summary>
        ///     The Node's state, which determines the coloring of the Node in the canvas.
        /// </summary>
        [JsonIgnore]
        public ElementState State
        {
            get { return state; }
            set
            {
                if (value != ElementState.Error && value != ElementState.AstBuildBroken)
                    ClearTooltipText();

                // Check before settings and raising
                // a notification.
                if (state == value) return;

                state = value;
                RaisePropertyChanged("State");
            }
        }

        /// <summary>
        ///   If the state of node is Error or AstBuildBroken
        /// </summary>
        [JsonIgnore]
        public bool IsInErrorState
        {
            get
            {
                return state == ElementState.AstBuildBroken || state == ElementState.Error;
            }
        }

        /// <summary>
        ///     Indicates if node preview is pinned
        /// </summary>
        [JsonIgnore]
        public bool PreviewPinned { get; internal set; }

        /// <summary>
        ///     Text that is displayed as this Node's tooltip.
        /// </summary>
        [JsonIgnore]
        public string ToolTipText
        {
            get { return toolTipText; }
            set
            {
                toolTipText = value;
                RaisePropertyChanged("ToolTipText");
            }
        }

        /// <summary>
        ///  The name that is displayed in the UI for this NodeModel.
        /// </summary>
        [JsonIgnore()]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        ///     Collection of PortModels representing all Input ports.
        /// </summary>
        [JsonProperty("Inputs")]
        public ObservableCollection<PortModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

        /// <summary>
        ///     Collection of PortModels representing all Output ports.
        /// </summary>
        [JsonProperty("Outputs")]
        public ObservableCollection<PortModel> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
                RaisePropertyChanged("OutPorts");
            }
        }

        [JsonIgnore]
        public IDictionary<int, Tuple<int, NodeModel>> InputNodes
        {
            get { return inputNodes; }
        }

        [JsonIgnore]
        public IDictionary<int, HashSet<Tuple<int, NodeModel>>> OutputNodes
        {
            get { return outputNodes; }
        }

        /// <summary>
        ///     Control how arguments lists of various sizes are laced.
        /// </summary>
        [JsonProperty("Replication"), JsonConverter(typeof(StringEnumConverter))]
        public LacingStrategy ArgumentLacing
        {
            get
            {
                return argumentLacing;
            }

            // The property setter is marked as private/protected because it
            // should not be set from an external component directly. The ability
            // to directly set the property value causes a NodeModel to be altered
            // without careful consideration of undo/redo recording. If changing
            // this property value should be undo-able, then the caller should use
            // "DynamoModel.UpdateModelValueCommand" to set the property value.
            // The command ensures changes to the NodeModel is recorded for undo.
            //
            // In some cases being able to set the property value directly is
            // desirable, for example, some unit test scenarios require the given
            // NodeModel property to be of certain value. In such cases the
            // easiest workaround is to use "NodeModel.UpdateValue" method:
            //
            //      someNode.UpdateValue("ArgumentLacing", "CrossProduct");
            //
            protected set
            {
                if (argumentLacing != value)
                {
                    argumentLacing = value;
                    RaisePropertyChanged("ArgumentLacing");

                    // Mark node for update
                    OnNodeModified();
                }
            }
        }

        private string ShortenName
        {
            get
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), false);

                if (!string.IsNullOrEmpty(CreationName))
                {
                    // Obtain the node's default name from its creation name.
                    // e.g. For creation name DSCore.Math.Max@double,double - the name "Max" is obtained and appended to the final link.
                    int indexAfter = (CreationName.LastIndexOf('@') == -1) ? CreationName.Length : CreationName.LastIndexOf('@');
                    string s = CreationName.Substring(0, indexAfter);

                    int indexBefore = s.LastIndexOf(Configurations.CategoryDelimiterString);
                    int firstChar = (indexBefore == -1) ? 0 : indexBefore + 1;
                    return s.Substring(firstChar, s.Length - CreationName.Substring(0, firstChar).Length);
                }

                if (!type.IsAbstract && (attribs.Length > 0))
                {
                    var attrib = attribs[0] as NodeNameAttribute;
                    if (attrib != null)
                    {
                        string name = attrib.Name;
                        if (!string.IsNullOrEmpty(name)) return name;
                    }
                }
                return "";
            }
        }

        /// <summary>
        ///     Category property
        /// </summary>
        /// <value>
        ///     If the node has a category, return it.  Other wise return empty string.
        /// </value>
        [JsonIgnore]
        public string Category
        {
            get
            {
                category = category ?? GetCategoryStringFromAttributes();
                return category;
            }
            set
            {
                category = value;
                RaisePropertyChanged("Category");
            }
        }

        private string category;

        private string GetCategoryStringFromAttributes()
        {
            Type type = GetType();
            object[] attribs = type.GetCustomAttributes(typeof(NodeCategoryAttribute), false);
            if (!type.IsAbstract && (attribs.Length > 0) && (attribs[0] is NodeCategoryAttribute))
            {
                string category = ((NodeCategoryAttribute)attribs[0]).ElementCategory;
                if (category != null) return category;
            }

            if (type.Namespace != "Dynamo.Graph.Nodes" || type.IsAbstract || attribs.Length <= 0
                || !type.IsSubclassOf(typeof(NodeModel)))
                return "";

            var elCatAttrib = attribs[0] as NodeCategoryAttribute;
            return elCatAttrib.ElementCategory;
        }

        /// <summary>
        ///     Dictionary Link property
        /// </summary>
        /// <value>
        ///     If the node has a name and a category, convert them into a link going to the node's help page on
        ///     Dynamo Dictionary, and return the link.
        ///     Otherwise, return the Dynamo Dictionary home page.
        /// </value>
        [JsonIgnore]
        public string DictionaryLink
        {
            get
            {
                dictionaryLink = dictionaryLink ?? Configurations.DynamoDictionary;
                return dictionaryLink;
            }
            set
            {
                dictionaryLink = value;
            }
        }

        private string dictionaryLink;

        internal string ConstructDictionaryLinkFromLibrary(LibraryServices libraryServices)
        {
            string finalLink = Configurations.DynamoDictionary + "#/";
            if (IsCustomFunction)
            {
                return ""; // If it is not a core or Revit function, do not display the dictionary link
            }
            if (category == null || category == "")
            {
                return Configurations.DynamoDictionary; // if there is no category, return the link to home page
            }

            int i = category.LastIndexOf(Configurations.CategoryDelimiterString);
            switch (category.Substring(i + 1))
            {
                case Configurations.CategoryGroupAction:
                    finalLink += ObtainURL(category.Substring(0, i));
                    finalLink += "Action/";
                    break;
                case Configurations.CategoryGroupCreate:
                    finalLink += ObtainURL(category.Substring(0, i));
                    finalLink += "Create/";
                    break;
                case Configurations.CategoryGroupQuery:
                    finalLink += ObtainURL(category.Substring(0, i));
                    finalLink += "Query/";
                    break;
                default:
                    finalLink += ObtainURL(category);
                    finalLink += "Action/";
                    break;
            }
            finalLink += this.ShortenName;

            // Check if the method has overloads
            IEnumerable<FunctionDescriptor> descriptors = libraryServices.GetAllFunctionDescriptors(CreationName.Split('@')[0]);
            if (descriptors != null && descriptors.Skip(1).Any())
            {
                // If there are overloads
                string parameters = "(";
                IEnumerable<Tuple<string, string>> inputParameters = null;

                foreach (FunctionDescriptor fd in descriptors)
                {
                    if (fd.MangledName == CreationName) // Find the function descriptor among the overloads and obtain their parameter names
                    {
                        inputParameters = fd.InputParameters;
                        break;
                    }
                }
                // Convert the parameters into a valid Dictionary URL format, e.g. (x_double-y_double-z_double)
                if (inputParameters != null)
                {
                    int parameterCount = inputParameters.Count();
                    for (int k = 0; k < parameterCount - 1; k++)
                    {
                        parameters += inputParameters.ElementAt(k).Item1 + "_" + inputParameters.ElementAt(k).Item2 + "-";
                    }
                    // Append the last parameter without the dash and with the close bracket
                    var lastInputParam = inputParameters.ElementAt(parameterCount - 1);
                    parameters += lastInputParam.Item1 + "_" + lastInputParam.Item2 + ")";
                    finalLink += parameters;
                }
            }
            return finalLink;
        }

        /// <summary>
        /// This method converts the character '.' in the node's category to '/', and append
        /// another '/' at the end, to be used as a URL.
        /// e.g. Core.Input.Action is converted to Core/Input/Action/
        /// </summary>
        private string ObtainURL(string category)
        {
            string result = "";
            for (int i = 0; i < category.Length; i++)
            {
                if (category[i] == '.')
                {
                    result += '/';
                }
                else
                {
                    result += category[i];
                }
            }
            result += '/';
            return result;
        }

        /// <summary>
        /// The value of this node after the most recent computation
        ///
        /// As this property could be modified by the virtual machine, it's dangerous
        /// to access this value without using the active Scheduler. Use the Scheduler to
        /// remove the possibility of race conditions.
        /// </summary>
        [JsonIgnore]
        public MirrorData CachedValue
        {
            get
            {
                lock (cachedValueMutex)
                {
                    return cachedValue;
                }
            }
            private set
            {
                lock (cachedValueMutex)
                {
                    cachedValue = value;
                }

                RaisePropertyChanged("CachedValue");
            }
        }

        /// <summary>
        /// This flag is used to determine if a node was involved in a recent execution.
        /// The primary purpose of this flag is to determine if the node's render packages
        /// should be returned to client browser when it requests for them. This is mainly
        /// to avoid returning redundant data that has not changed during an execution.
        /// </summary>
        internal bool WasInvolvedInExecution { get; set; }

        /// <summary>
        /// This flag indicates if render packages of a NodeModel has been updated
        /// since the last execution. UpdateRenderPackageAsyncTask will always be
        /// generated for a NodeModel that took part in the evaluation, if this flag
        /// is false.
        /// </summary>
        internal bool WasRenderPackageUpdatedAfterExecution { get; set; }

        /// <summary>
        ///     Search tags for this Node.
        /// </summary>
        [JsonIgnore]
        public List<string> Tags
        {
            get
            {
                return
                    GetType()
                        .GetCustomAttributes(typeof(NodeSearchTagsAttribute), true)
                        .Cast<NodeSearchTagsAttribute>()
                        .SelectMany(x => x.Tags)
                        .ToList();
            }
        }

        [JsonConverter(typeof(DescriptionConverter))]
        /// <summary>
        ///     Description of this Node.
        /// </summary>
        public string Description
        {
            get
            {
                description = description ?? GetDescriptionStringFromAttributes();
                return description;
            }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }

        [JsonIgnore]
        public bool CanUpdatePeriodically
        {
            get { return canUpdatePeriodically; }
            set
            {
                canUpdatePeriodically = value;
                RaisePropertyChanged("CanUpdatePeriodically");
            }
        }

        /// <summary>
        ///     ProtoAST Identifier for result of the node before any output unpacking has taken place.
        ///     If there is only one output for the node, this is equivalent to GetAstIdentifierForOutputIndex(0).
        /// </summary>
        [JsonIgnore]
        public IdentifierNode AstIdentifierForPreview
        {
            get { return AstFactory.BuildIdentifier(AstIdentifierBase); }
        }

        /// <summary>
        ///     If this node is allowed to be converted to AST node in nodes to code conversion.
        /// </summary>
        [JsonIgnore]
        public virtual bool IsConvertible
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Return a variable whose value will be displayed in preview window.
        ///     Derived nodes may overwrite this function to display default value
        ///     of this node. E.g., code block node may want to display the value
        ///     of the left hand side variable of last statement.
        /// </summary>
        [JsonIgnore]
        public virtual string AstIdentifierBase
        {
            get
            {
                return AstBuilder.StringConstants.VarPrefix + AstIdentifierGuid;
            }
        }

        /// <summary>
        ///     A unique ID that will be appended to all identifiers of this node.
        /// </summary>
        [JsonIgnore]
        public string AstIdentifierGuid
        {
            get
            {
                return GUID.ToString("N");
            }
        }

        /// <summary>
        ///     Enable or disable label display. Default is false.
        /// </summary>
        [JsonIgnore]
        public bool DisplayLabels
        {
            get { return displayLabels; }
            set
            {
                if (displayLabels == value)
                    return;

                displayLabels = value;
                RaisePropertyChanged("DisplayLabels");
            }
        }

        /// <summary>
        ///     Is this node being applied partially, resulting in a partial function?
        /// </summary>
        [JsonIgnore]
        public bool IsPartiallyApplied //TODO(Steve): Move to Graph level -- MAGN-5710
        {
            get { return !inPorts.All(p => p.IsConnected); }
        }

        /// <summary>
        ///     Returns the description from type information
        /// </summary>
        /// <returns>The value or "No description provided"</returns>
        public string GetDescriptionStringFromAttributes()
        {
            Type t = GetType();
            object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true);
            return rtAttribs.Length > 0
                ? ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription
                : Properties.Resources.NoDescriptionAvailable;
        }

        /// <summary>
        ///     Fetches the ProtoAST Identifier for a given output port.
        /// </summary>
        /// <param name="outputIndex">Index of the output port.</param>
        /// <returns>Identifier corresponding to the given output port.</returns>
        public virtual IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (outputIndex < 0 || outputIndex > OutPorts.Count)
                throw new ArgumentOutOfRangeException("outputIndex", @"Index must correspond to an OutPortData index.");

            if (OutPorts.Count <= 1)
                return AstIdentifierForPreview;
            else
            {
                string id = AstIdentifierBase + "_out" + outputIndex;
                return AstFactory.BuildIdentifier(id);
            }
        }

        /// <summary>
        ///      The possible type of output at specified port. This
        ///      type information is not necessary to be accurate.
        /// </summary>
        /// <returns></returns>
        public virtual ProtoCore.Type GetTypeHintForOutput(int index)
        {
            return ProtoCore.TypeSystem.BuildPrimitiveTypeObject(ProtoCore.PrimitiveType.Var);
        }

        /// <summary>
        /// A flag indicating whether the node is frozen.
        /// When a node is frozen, the node, and all nodes downstream will not participate in execution.
        /// This will return true if any upstream node is frozen or if the node was explicitly frozen.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this node is frozen; otherwise, <c>false</c>.
        /// </value>
        [JsonIgnore]
        public bool IsFrozen
        {
            get
            {
                return IsAnyUpstreamFrozen() || isFrozenExplicitly;
            }
            set
            {
                bool oldValue = isFrozenExplicitly;
                isFrozenExplicitly = value;
                //If the node is Unfreezed then Mark all the downstream nodes as
                // modified. This is essential recompiling the AST.
                if (!value)
                {
                    if (oldValue)
                    {
                        MarkDownStreamNodesAsModified(this);
                        OnNodeModified();
                        RaisePropertyChanged("IsFrozen");
                    }
                }
                //If the node is frozen, then do not execute the graph immediately.
                // delete the node and its downstream nodes from AST.
                else
                {
                    ComputeUpstreamOnDownstreamNodes();
                    OnUpdateASTCollection();
                }
            }
        }

        /// <summary>
        /// The default behavior for ModelBase objects is to not serialize the X and Y
        /// properties. This overload allows the serialization of the X property
        /// for NodeModel.
        /// </summary>
        /// <returns>True.</returns>
        public override bool ShouldSerializeX()
        {
            return false;
        }

        /// <summary>
        /// The default behavior for ModelBase objects is to not serialize the X and Y
        /// properties. This overload allows the serialization of the Y property
        /// for NodeModel.
        /// </summary>
        /// <returns>True</returns>
        public override bool ShouldSerializeY()
        {
            return false;
        }

        [JsonIgnore]
        public virtual NodeInputData InputData
        {
           get { return null; }
        }

        [JsonIgnore]
        public virtual NodeOutputData OutputData
        {
            get
            {
                // Determine if the output type can be determined at this time
                // Current enum supports String, Integer, Float, Boolean, and unknown
                // When CachedValue is null, type is set to unknown
                // When Concrete type is dictionary or other type not expressed in enum, type is set to unknown
                object returnObj = CachedValue?.Data?? new object();
                var returnType = NodeOutputData.getNodeOutputTypeFromType(returnObj.GetType());
                var returnValue = String.Empty;

                // IntialValue is returned when the Type enum does not equal unknown
                if(returnType != NodeOutputTypes.unknownOutput)
                {
                    var formattableReturnObj = returnObj as IFormattable;
                    returnValue = formattableReturnObj != null ? formattableReturnObj.ToString(null, CultureInfo.InvariantCulture) : returnObj.ToString();
                }

                
                return new NodeOutputData()
                {
                    Id = this.GUID,
                    Name = this.Name,
                    Type = returnType,
                    Description = this.Description,
                    InitialValue = returnValue
                };
            }
        }

        #endregion

        #region freeze execution
        /// <summary>
        /// Determines whether any of the upstream node is frozen.
        /// </summary>
        /// <returns></returns>
        internal bool IsAnyUpstreamFrozen()
        {
            return UpstreamCache.Any(x => x.isFrozenExplicitly);
        }

        /// <summary>
        /// For a given node, this function computes all the upstream nodes
        /// by gathering the cached upstream nodes on this node's immediate parents.
        /// </summary>
        internal void ComputeUpstreamCache()
        {
            this.UpstreamCache = new HashSet<NodeModel>();
            var inpNodes = this.InputNodes.Values;

            foreach (var inputnode in inpNodes.Where(x => x != null))
            {
                this.UpstreamCache.Add(inputnode.Item2);
                foreach (var upstreamNode in inputnode.Item2.UpstreamCache)
                {
                    this.UpstreamCache.Add(upstreamNode);
                }
            }
        }

        /// <summary>
        /// For a given node, this function computes all the upstream nodes
        /// by gathering the cached upstream nodes on this node's immediate parents.
        /// If a node has any downstream nodes, then for all those downstream nodes, upstream
        /// nodes will be computed. Essentially this method propogates the UpstreamCache down.
        /// Also this function gets called only after the workspace is added.
        /// </summary>
        internal void ComputeUpstreamOnDownstreamNodes()
        {
            //first compute upstream nodes for this node
            ComputeUpstreamCache();

            //then for downstream nodes
            //gather downstream nodes and bail if we see an already visited node
            HashSet<NodeModel> downStreamNodes = new HashSet<NodeModel>();
            this.GetDownstreamNodes(this, downStreamNodes);

            foreach (var downstreamNode in AstBuilder.TopologicalSort(downStreamNodes))
            {
                downstreamNode.UpstreamCache = new HashSet<NodeModel>();
                var currentinpNodes = downstreamNode.InputNodes.Values;
                foreach (var inputnode in currentinpNodes.Where(x => x != null))
                {
                    downstreamNode.UpstreamCache.Add(inputnode.Item2);
                    foreach (var upstreamNode in inputnode.Item2.UpstreamCache)
                    {
                        downstreamNode.UpstreamCache.Add(upstreamNode);
                    }
                }
            }

            RaisePropertyChanged("IsFrozen");
        }

        private void MarkDownStreamNodesAsModified(NodeModel node)
        {
            HashSet<NodeModel> gathered = new HashSet<NodeModel>();
            GetDownstreamNodes(node, gathered);
            foreach (var iNode in gathered)
            {
                iNode.executionHint = ExecutionHints.Modified;
            }
        }

        /// <summary>
        /// Returns the downstream nodes for the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="gathered">The gathered.</param>
        internal void GetDownstreamNodes(NodeModel node, HashSet<NodeModel> gathered)
        {
            if (gathered.Contains(node)) // Considered this node before, bail.pu
                return;

            gathered.Add(node);

            var sets = node.OutputNodes.Values;
            var outputNodes = sets.SelectMany(set => set.Select(t => t.Item2));
            foreach (var outputNode in outputNodes)
            {
                // Recursively get all downstream nodes.
                GetDownstreamNodes(outputNode, gathered);
            }
        }
        #endregion

        /// <summary>
        /// Protected constructor used during deserialization.
        /// </summary>
        /// <param name="inPorts"></param>
        /// <param name="outPorts"></param>
        protected NodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
        {
            inputNodes = new Dictionary<int, Tuple<int, NodeModel>>();
            outputNodes = new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();

            // Initialize the port events
            // Note: It is important that this occurs before the ports are added next
            InPorts.CollectionChanged += PortsCollectionChanged;
            OutPorts.CollectionChanged += PortsCollectionChanged;

            // Set the ports from the deserialized data
            InPorts.AddRange(inPorts);
            OutPorts.AddRange(outPorts);

            IsVisible = true;
            ShouldDisplayPreviewCore = true;
            executionHint = ExecutionHints.Modified;

            PropertyChanged += delegate (object sender, PropertyChangedEventArgs args)
            {
                switch (args.PropertyName)
                {
                    case ("OverrideName"):
                        RaisePropertyChanged("Name");
                        break;
                }
            };

            //Fetch the element name from the custom attribute.
            SetNameFromNodeNameAttribute();

            IsSelected = false;
            SetNodeStateBasedOnConnectionAndDefaults();
            ArgumentLacing = LacingStrategy.Disabled;

            RaisesModificationEvents = true;
        }

        protected NodeModel()
        {
            inputNodes = new Dictionary<int, Tuple<int, NodeModel>>();
            outputNodes = new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();

            IsVisible = true;
            ShouldDisplayPreviewCore = true;
            executionHint = ExecutionHints.Modified;

            PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch (args.PropertyName)
                {
                    case ("OverrideName"):
                        RaisePropertyChanged("Name");
                        break;
                }
            };

            //Fetch the element name from the custom attribute.
            SetNameFromNodeNameAttribute();

            IsSelected = false;
            State = ElementState.Dead;
            ArgumentLacing = LacingStrategy.Disabled;

            RaisesModificationEvents = true;

            InPorts.CollectionChanged += PortsCollectionChanged;
            OutPorts.CollectionChanged += PortsCollectionChanged;
        }

        private void PortsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    ConfigureSnapEdges(sender == InPorts ? InPorts : OutPorts);
                    foreach(PortModel p in e.NewItems)
                    {
                        p.Connectors.CollectionChanged += (coll, args) =>
                        {
                            // Call the collection changed handler, replacing
                            // the 'sender' with the port, which is required
                            // for the disconnect operations.
                            ConnectorsCollectionChanged(p, args);
                        };
                        p.PropertyChanged += OnPortPropertyChanged;
                        SetNodeStateBasedOnConnectionAndDefaults();
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach(PortModel p in e.OldItems)
                    {
                        p.PropertyChanged -= OnPortPropertyChanged;

                        p.DestroyConnectors();

                        SetNodeStateBasedOnConnectionAndDefaults();
                    }
                    break;
            }
        }

        private void ConnectorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var p = (PortModel)sender;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach(ConnectorModel c in e.NewItems)
                    {
                        OnPortConnected(p, c);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (ConnectorModel c in e.OldItems)
                    {
                        OnPortDisconnected(p, c);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (ConnectorModel c in e.OldItems)
                    {
                        OnPortDisconnected(p, c);
                    }
                    break;
            }
            SetNodeStateBasedOnConnectionAndDefaults();
        }

        private string getNameFromNodeNameAttribute()
        {
            Type type = GetType();
            object[] attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), false);
            if (type.Namespace == "Dynamo.Graph.Nodes" && !type.IsAbstract && attribs.Length > 0
                && type.IsSubclassOf(typeof(NodeModel)))
            {
                var elCatAttrib = attribs[0] as NodeNameAttribute;
                return elCatAttrib.Name;
            }
            return "";
        }

        /// <summary>
        /// Returns the most recent value of this node stored in an EngineController that has evaluated it.
        /// </summary>
        /// <param name="outPortIndex"></param>
        /// <param name="engine"></param>
        /// <returns></returns>
        public MirrorData GetValue(int outPortIndex, EngineController engine)
        {
            return engine.GetMirror(GetAstIdentifierForOutputIndex(outPortIndex).Value).GetData();
        }

        /// <summary>
        ///     Sets the name of this node from the attributes on the class definining it.
        /// </summary>
        public void SetNameFromNodeNameAttribute()
        {
            var elNameAttrib = GetType().GetCustomAttributes<NodeNameAttribute>(false).FirstOrDefault();
            if (elNameAttrib != null)
                Name = elNameAttrib.Name;

        }

        #region Modification Reporting

        /// <summary>
        ///     Indicate if the node should respond to NodeModified event. It
        ///     always should be true, unless is temporarily set to false to
        ///     avoid flood of Modified event.
        /// </summary>
        [JsonIgnore]
        public bool RaisesModificationEvents { get; set; }

        /// <summary>
        ///     Event fired when the node's DesignScript AST should be recompiled
        /// </summary>
        public event Action<NodeModel> Modified;
        public virtual void OnNodeModified(bool forceExecute = false)
        {
            if (!RaisesModificationEvents || IsFrozen)
                return;

            MarkNodeAsModified(forceExecute);
            var handler = Modified;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Event fired when the node's DesignScript AST should be updated.
        /// This event deletes the frozen nodes from AST collection.
        /// </summary>
        public event Action<NodeModel> UpdateASTCollection;
        public virtual void OnUpdateASTCollection()
        {
            var handler = UpdateASTCollection;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Called when a node is requesting that the workspace's node modified events be
        /// silenced. This is particularly critical for code block nodes, whose modification can
        /// mutate the workspace.
        ///
        /// As opposed to RaisesModificationEvents, this modifies the entire parent workspace
        /// </summary>
        internal event Action<NodeModel, bool> RequestSilenceNodeModifiedEvents;

        internal void OnRequestSilenceModifiedEvents(bool silence)
        {
            if (RequestSilenceNodeModifiedEvents != null)
            {
                RequestSilenceNodeModifiedEvents(this, silence);
            }
        }

        #endregion

        #region ProtoAST Compilation

        /// <summary>
        /// Override this to declare the outputs for each of this Node's output ports.
        /// </summary>
        /// <param name="inputAstNodes">Ast for inputs indexed by input port index.</param>
        /// <returns>Sequence of AssociativeNodes representing this Node's code output.</returns>
        public virtual IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return
                OutPorts.Enumerate()
                           .Select(
                               output => AstFactory.BuildAssignment(
                                   GetAstIdentifierForOutputIndex(output.Index),
                                   new NullNode()));
        }

        /// <summary>
        /// Wraps the publically overrideable `BuildOutputAst` method so that it works with Preview.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <param name="context">Compilation context</param>
        internal virtual IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            OnBuilt();

            IEnumerable<AssociativeNode> result = null;

            try
            {
                result = BuildOutputAst(inputAstNodes);
            }
            catch (Exception e)
            {
                // If any exception from BuildOutputAst(), we emit
                // a function call "var_guid = %nodeAstFailed(full.node.name)"
                // for this node, set the state of node to AstBuildBroken and
                // disply the corresponding error message.
                //
                // The return value of function %nodeAstFailed() is always
                // null.
                var errorMsg = Properties.Resources.NodeProblemEncountered;
                var fullMsg = errorMsg + "\n\n" + e.Message;
                this.NotifyAstBuildBroken(fullMsg);

                var fullName = this.GetType().ToString();
                var astNodeFullName = AstFactory.BuildStringNode(fullName);
                var arguments = new List<AssociativeNode> { astNodeFullName };
                var func = AstFactory.BuildFunctionCall(Constants.kNodeAstFailed, arguments);

                return new[]
                {
                    AstFactory.BuildAssignment(AstIdentifierForPreview, func)
                };
            }

            if (OutPorts.Count == 1)
            {
                var firstOuputIdent = GetAstIdentifierForOutputIndex(0);
                if (!AstIdentifierForPreview.Equals(firstOuputIdent))
                {
                    result = result.Concat(
                    new[]
                    {
                        AstFactory.BuildAssignment(AstIdentifierForPreview, firstOuputIdent)
                    });
                }
                return result;
            }

            // Create AST for Dictionary initialization:
            // var_ast_identifier = {"outport1" : var_ast_identifier_out1, ..., "outportn" : var_ast_identifier_outn};
            var kvps = OutPorts.Select((outNode, index) =>
                new KeyValuePair<StringNode, IdentifierNode>
                    (new StringNode {Value = outNode.Name}, GetAstIdentifierForOutputIndex(index)));

            var dict = new DictionaryExpressionBuilder();
            foreach (var kvp in kvps)
            {
                dict.AddKey(kvp.Key);
                dict.AddValue(kvp.Value);
            }
            return result.Concat(new[]
            {
                AstFactory.BuildAssignment(
                    this.AstIdentifierForPreview, dict.ToFunctionCall())
            });
        }

        /// <summary>
        ///     Callback for when this NodeModel has been compiled.
        /// </summary>
        protected virtual void OnBuilt()
        {
        }

        /// <summary>
        /// Apppend replication guide to the input parameter based on lacing
        /// strategy.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public void UseLevelAndReplicationGuide(List<AssociativeNode> inputs)
        {
            if (inputs == null || !inputs.Any())
                return;

            for (int i = 0; i < inputs.Count; i++)
            {
                if (InPorts[i].UseLevels)
                {
                    inputs[i] = AstFactory.AddAtLevel(inputs[i], -InPorts[i].Level, InPorts[i].KeepListStructure);
                }
            }

            switch (ArgumentLacing)
            {
                case LacingStrategy.Auto:
                    for (int i = 0; i < inputs.Count(); ++i)
                    {
                        if (InPorts[i].UseLevels)
                            inputs[i] = AstFactory.AddReplicationGuide(inputs[i], new List<int> { 1 }, false);
                    }
                    break;

                case LacingStrategy.Shortest:
                    for (int i = 0; i < inputs.Count(); ++i)
                    {
                        inputs[i] = AstFactory.AddReplicationGuide(inputs[i], new List<int> { 1 }, false);
                    }
                    break;


                case LacingStrategy.Longest:

                    for (int i = 0; i < inputs.Count(); ++i)
                    {
                        inputs[i] = AstFactory.AddReplicationGuide(inputs[i], new List<int> { 1 }, true);
                    }
                    break;

                case LacingStrategy.CrossProduct:

                    int guide = 1;
                    for (int i = 0; i < inputs.Count(); ++i)
                    {
                        inputs[i] = AstFactory.AddReplicationGuide(inputs[i], new List<int> { guide }, false);
                        guide++;
                    }
                    break;
            }
        }
        #endregion

        #region Input and Output Connections

        /// <summary>
        ///     Event fired when a new ConnectorModel has been attached to one of this node's inputs.
        /// </summary>
        public event Action<ConnectorModel> ConnectorAdded;
        protected virtual void OnConnectorAdded(ConnectorModel obj)
        {
            var handler = ConnectorAdded;
            if (handler != null) handler(obj);
        }

        /// <summary>
        /// If node is connected to some other node(other than Output) then it is not a 'top' node
        /// </summary>
        [JsonIgnore]
        public bool IsTopMostNode
        {
            get
            {
                if (OutPorts.Count < 1)
                    return false;

                foreach (var port in OutPorts.Where(port => port.Connectors.Count != 0))
                {
                    return port.Connectors.Any(connector => connector.End.Owner is Output);
                }

                return true;
            }
        }

        internal void ConnectInput(int inputData, int outputData, NodeModel node)
        {
            inputNodes[inputData] = Tuple.Create(outputData, node);
        }

        internal void ConnectOutput(int portData, int inputData, NodeModel nodeLogic)
        {
            if (!outputNodes.ContainsKey(portData))
                outputNodes[portData] = new HashSet<Tuple<int, NodeModel>>();
            outputNodes[portData].Add(Tuple.Create(inputData, nodeLogic));
        }

        internal void DisconnectInput(int data)
        {
            inputNodes[data] = null;
        }

        /// <summary>
        ///     Attempts to get the input for a certain port.
        /// </summary>
        /// <param name="data">PortData to look for an input for.</param>
        /// <param name="input">If an input is found, it will be assigned.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool TryGetInput(int data, out Tuple<int, NodeModel> input)
        {
            return inputNodes.TryGetValue(data, out input) && input != null;
        }

        /// <summary>
        ///     Attempts to get the output for a certain port.
        /// </summary>
        /// <param name="output">Index to look for an output for.</param>
        /// <param name="newOutputs">If an output is found, it will be assigned.</param>
        /// <returns>True if there is an output, false otherwise.</returns>
        public bool TryGetOutput(int output, out HashSet<Tuple<int, NodeModel>> newOutputs)
        {
            return outputNodes.TryGetValue(output, out newOutputs);
        }

        internal void DisconnectOutput(int portData, int inPortData, NodeModel nodeModel)
        {
            HashSet<Tuple<int, NodeModel>> output;
            if (outputNodes.TryGetValue(portData, out output))
                output.RemoveWhere(x => x.Item2 == nodeModel && x.Item1 == inPortData);
        }

        #endregion

        #region UI Framework

        private void ClearTooltipText()
        {
            ToolTipText = "";
        }

        private void ClearPersistentWarning()
        {
            persistentWarning = String.Empty;
        }

        /// <summary>
        /// Clears the errors/warnings that are generated when running the graph,
        /// the State will be set to ElementState.Dead.
        /// </summary>
        public virtual void ClearErrorsAndWarnings()
        {
            State = ElementState.Dead;
            ClearPersistentWarning();

            SetNodeStateBasedOnConnectionAndDefaults();
            ClearTooltipText();
        }

        public void SelectNeighbors()
        {
            IEnumerable<ConnectorModel> outConnectors = outPorts.SelectMany(x => x.Connectors);
            IEnumerable<ConnectorModel> inConnectors = inPorts.SelectMany(x => x.Connectors);

            foreach (var c in outConnectors.Where(c => !DynamoSelection.Instance.Selection.Contains(c.End.Owner)))
                DynamoSelection.Instance.Selection.Add(c.End.Owner);

            foreach (var c in inConnectors.Where(c => !DynamoSelection.Instance.Selection.Contains(c.Start.Owner)))
                DynamoSelection.Instance.Selection.Add(c.Start.Owner);
        }

        #region Node State

        /// <summary>
        /// Sets the <seealso cref="ElementState"/> for the node based on
        /// the port's default value status and connectivity.
        /// </summary>
        protected virtual void SetNodeStateBasedOnConnectionAndDefaults()
        {
            //Debug.WriteLine(string.Format("Validating Connections: Node type: {0}, {1} inputs, {2} outputs", this.GetType(), this.InPorts.Count(), this.OutPorts.Count()));

            if (State == ElementState.PersistentWarning) return;

            if (!string.IsNullOrEmpty(persistentWarning))
            {
                State = ElementState.PersistentWarning;
                return;
            }

            // if there are inputs without connections
            // mark as dead; otherwise, if the original state is dead,
            // update it as active.
            if (inPorts.Any(x => !x.IsConnected && !(x.UsingDefaultValue && x.DefaultValue != null)))
            {
                if (State == ElementState.Active) State = ElementState.Dead;
            }
            else
            {
                if (State == ElementState.Dead) State = ElementState.Active;
            }
        }

        public void Error(string p)
        {
            State = ElementState.Error;
            ToolTipText = p;
        }

        /// <summary>
        /// Set a warning on a node.
        /// </summary>
        /// <param name="p">The warning text.</param>
        /// <param name="isPersistent">Is the warning persistent? If true, the warning will not be
        /// cleared when the node is next evaluated and any additional warning messages will be concatenated
        /// to the persistent error message. If false, the warning will be cleared on the next evaluation.</param>
        public void Warning(string p, bool isPersistent = false)
        {
            if (isPersistent)
            {
                State = ElementState.PersistentWarning;
                if (!string.Equals(persistentWarning, p))
                {
                    persistentWarning += p;
                }
                ToolTipText = persistentWarning;
            }
            else
            {
                State = ElementState.Warning;
                ToolTipText = string.IsNullOrEmpty(persistentWarning) ? p : string.Format("{0}\n{1}", persistentWarning, p);
                ClearPersistentWarning();
            }
        }

        /// <summary>
        /// Change the state of node to ElementState.AstBuildBroken and display
        /// "p" in tooltip window.
        /// </summary>
        /// <param name="p"></param>
        public void NotifyAstBuildBroken(string p)
        {
            State = ElementState.AstBuildBroken;
            ToolTipText = p;
        }

        #endregion

        #region Port Management

        internal int GetPortModelIndex(PortModel portModel)
        {
            if (portModel.PortType == PortType.Input)
                return InPorts.IndexOf(portModel);
            else
                return OutPorts.IndexOf(portModel);
        }

        /// <summary>
        /// If a "PortModel.LineIndex" property isn't "-1", then it is a PortModel
        /// meant to match up with a line in code block node. A code block node may
        /// contain empty lines in it, resulting in one PortModel being spaced out
        /// from another one. In such cases, the vertical position of PortModel is
        /// dependent of its "LineIndex".
        ///
        /// If a "PortModel.LineIndex" property is "-1", then it is a regular
        /// PortModel. Regular PortModel stacks up on one another with equal spacing,
        /// so their positions are based solely on "PortModel.Index".
        /// </summary>
        /// <param name="portModel">The portModel whose vertical offset is to be computed.</param>
        /// <returns>Returns the offset of the given port from the top of the ports</returns>
        //TODO(Steve): This kind of UI calculation should probably live on the VM. -- MAGN-5711
        internal double GetPortVerticalOffset(PortModel portModel)
        {
            double verticalOffset = 2.9;
            int index = portModel.LineIndex == -1 ? portModel.Index : portModel.LineIndex;

            //If the port was not found, then it should have just been deleted. Return from function
            if (index == -1)
                return verticalOffset;

            double portHeight = portModel.Height;
            return verticalOffset + index * portModel.Height;
        }

        /// <summary>
        ///     Reads inputs list and adds ports for each input.
        /// </summary>
        [Obsolete("RegisterInputPorts is deprecated, please use the InPortNamesAttribute, InPortDescriptionsAttribute, and InPortTypesAttribute instead.")]
        public void RegisterInputPorts(IEnumerable<PortData> portDatas)
        {
            int count = 0;
            foreach (PortData pd in portDatas)
            {
                var port = AddPort(PortType.Input, pd, count);
                count++;
            }

            if (inPorts.Count > count)
            {
                foreach (PortModel inport in inPorts.Skip(count))
                {
                    inport.DestroyConnectors();
                }

                for (int i = inPorts.Count - 1; i >= count; i--)
                    inPorts.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Reads outputs list and adds ports for each output
        /// </summary>
        [Obsolete("RegisterOutputPorts is deprecated, please use the OutPortNamesAttribute, OutPortDescriptionsAttribute, and OutPortTypesAttribute instead.")]
        public void RegisterOutputPorts(IEnumerable<PortData> portDatas)
        {
            int count = 0;
            foreach (PortData pd in portDatas)
            {
                var port = AddPort(PortType.Output, pd, count);
                count++;
            }

            if (outPorts.Count > count)
            {
                foreach (PortModel outport in outPorts.Skip(count))
                    outport.DestroyConnectors();

                for (int i = outPorts.Count - 1; i >= count; i--)
                    outPorts.RemoveAt(i);
            }
        }

        /// <summary>
        /// Tries to load ports names and descriptions from attributes.
        /// </summary>
        /// <param name="portType">Input or Output port type</param>
        private IEnumerable<PortData> GetPortDataFromAttributes(PortType portType)
        {
            var type = GetType();
            List<string> names = null;
            List<string> descriptions = null;

            switch (portType)
            {
                case PortType.Input:
                    {
                        names = type.GetCustomAttributes<InPortNamesAttribute>(false)
                                .SelectMany(x => x.PortNames)
                                .ToList();
                        descriptions = type.GetCustomAttributes<InPortDescriptionsAttribute>(false)
                            .SelectMany(x => x.PortDescriptions)
                            .ToList();
                        break;
                    }
                case PortType.Output:
                    {
                        names = type.GetCustomAttributes<OutPortNamesAttribute>(false)
                                .SelectMany(x => x.PortNames)
                                .ToList();
                        descriptions = type.GetCustomAttributes<OutPortDescriptionsAttribute>(false)
                            .SelectMany(x => x.PortDescriptions)
                            .ToList();
                        break;
                    }
            }

            if (names == null)
            {
                return new List<PortData>();
            }

            if (names.Count != descriptions.Count)
            {
                Log(String.Concat(
                        Name,
                        ": ",
                        Properties.Resources.PortsNameDescriptionDoNotEqualWarningMessage));

                // Take the same number of descriptions as number of names.
                descriptions = new List<string>(descriptions.Take(names.Count));
            }

            var ports = new List<PortData>();
            for (int i = 0; i < names.Count; i++)
            {
                string descr = i < descriptions.Count ? descriptions[i] : String.Empty;
                var pd = new PortData(names[i], descr);
                ports.Add(pd);
            }

            return ports;
        }

        /// <summary>
        /// Configures the snap edges.
        /// This class was made protected during refactoring for serialization. When
        /// RegisterInputPorts and RegisterOutputPorts are finally removed, this method
        /// should be called in a collection changed event handler on InPorts and OutPorts.
        /// </summary>
        /// <param name="ports">The ports.</param>
        protected static void ConfigureSnapEdges(IList<PortModel> ports)
        {
            switch (ports.Count)
            {
                case 0:
                    break;
                case 1:
                    ports[0].extensionEdges = SnapExtensionEdges.Top | SnapExtensionEdges.Bottom;
                    break;
                case 2:
                    ports[0].extensionEdges = SnapExtensionEdges.Top;
                    ports[1].extensionEdges = SnapExtensionEdges.Bottom;
                    break;
                default:
                    ports[0].extensionEdges = SnapExtensionEdges.Top;
                    ports[ports.Count - 1].extensionEdges = SnapExtensionEdges.Bottom;
                    var query =
                        ports.Where(
                            port => !port.extensionEdges.HasFlag(SnapExtensionEdges.Top | SnapExtensionEdges.Bottom)
                                && !port.extensionEdges.HasFlag(SnapExtensionEdges.Top)
                                && !port.extensionEdges.HasFlag(SnapExtensionEdges.Bottom));
                    foreach (var port in query)
                        port.extensionEdges = SnapExtensionEdges.None;
                    break;
            }
        }

        /// <summary>
        ///     Updates UI so that all ports reflect current state of node ports.
        /// </summary>
        public void RegisterAllPorts()
        {
            RaisesModificationEvents = false;

            var inportDatas = GetPortDataFromAttributes(PortType.Input);
            if (inportDatas.Any())
            {
                RegisterInputPorts(inportDatas);
            }

            var outPortDatas = GetPortDataFromAttributes(PortType.Output);
            if (outPortDatas.Any())
            {
                RegisterOutputPorts(outPortDatas);
            }

            RaisesModificationEvents = true;
            areInputPortsRegistered = true;
        }

        /// <summary>
        ///     Add a port to this node. If the port already exists, return that port.
        /// </summary>
        /// <param name="portType"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private PortModel AddPort(PortType portType, PortData data, int index)
        {
            PortModel p;
            switch (portType)
            {
                case PortType.Input:
                    if (inPorts.Count > index)
                    {
                        p = inPorts[index];
                    }
                    else
                    {
                        p = new PortModel(portType, this, data);
                        p.PropertyChanged += OnPortPropertyChanged;
                        InPorts.Add(p);
                    }

                    return p;

                case PortType.Output:
                    if (outPorts.Count > index)
                    {
                        p = outPorts[index];
                    }
                    else
                    {
                        p = new PortModel(portType, this, data);
                        OutPorts.Add(p);
                    }

                    return p;
            }

            return null;
        }

        private void OnPortPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "Level":
                case "UseLevels":
                case "KeepListStructure":
                    OnNodeModified();
                    break;
                case "UsingDefaultValue":
                    SetNodeStateBasedOnConnectionAndDefaults();
                    OnNodeModified();
                    break;
                default:
                    break;
            }
        }

        private void OnPortConnected(PortModel port, ConnectorModel connector)
        {
          
            if (port.PortType != PortType.Input) return;

            var data = InPorts.IndexOf(port);
            var startPort = connector.Start;
            var outData = startPort.Owner.OutPorts.IndexOf(startPort);
            ConnectInput(data, outData, startPort.Owner);
            startPort.Owner.ConnectOutput(outData, data, this);

            var handler = PortConnected;
            if (null != handler) handler(port, connector);

            OnConnectorAdded(connector);

            OnNodeModified();
        }

        private void OnPortDisconnected(PortModel port, ConnectorModel connector)
        {
            var handler = PortDisconnected;
            if (null != handler) handler(port);

            if (port.PortType != PortType.Input) return;

            var data = InPorts.IndexOf(port);
            var startPort = connector.Start;
            DisconnectInput(data);
            startPort.Owner.DisconnectOutput(startPort.Owner.OutPorts.IndexOf(startPort), data, this);

            OnNodeModified();
        }

        #endregion

        #endregion

        #region Code Serialization

        /// <summary>
        ///     Creates a Scheme representation of this dynNode and all connected dynNodes.
        /// </summary>
        /// <returns>S-Expression</returns>
        public virtual string PrintExpression()
        {
            string nick = Name.Replace(' ', '_');

            if (!InPorts.Any(p=>p.IsConnected))
                return nick;

            string s = "";

            if (InPorts.All(p=>p.IsConnected))
            {
                s += "(" + nick;
                foreach (int data in Enumerable.Range(0, InPorts.Count))
                {
                    Tuple<int, NodeModel> input;
                    TryGetInput(data, out input);
                    s += " " + input.Item2.PrintExpression();
                }
                s += ")";
            }
            else
            {
                s += "(lambda (" + string.Join(" ", InPorts.Where((_, i) => !InPorts[i].IsConnected).Select(x => x.Name))
                     + ") (" + nick;
                foreach (int data in Enumerable.Range(0, InPorts.Count))
                {
                    s += " ";
                    Tuple<int, NodeModel> input;
                    if (TryGetInput(data, out input))
                        s += input.Item2.PrintExpression();
                    else
                        s += InPorts[data].Name;
                }
                s += "))";
            }

            return s;
        }

        #endregion

        #region ISelectable Interface

        public override void Deselect()
        {
            IsSelected = false;
        }

        #endregion

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case "Name":
                    Name = value;
                    return true;

                case "Position":
                    // Here we expect a string that represents an array of double values which are separated by ";"
                    // For example "12.5;14.56"
                    var pos = value.Split(';');
                    double xPos, yPos;
                    if (pos.Length == 2 && double.TryParse(pos[0], out xPos)
                        && double.TryParse(pos[1], out yPos))
                    {
                        X = xPos;
                        Y = yPos;
                        ReportPosition();
                    }

                    return true;

                case "UsingDefaultValue":
                    if (string.IsNullOrWhiteSpace(value))
                        return true;

                    // Here we expect a string that represents an array of Boolean values which are separated by ";"
                    var arr = value.Split(';');
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var useDef = !bool.Parse(arr[i]);
                        // do not set true, if default value is disabled
                        if (!useDef || InPorts[i].DefaultValue != null)
                        {
                            InPorts[i].UsingDefaultValue = useDef;
                        }
                    }
                    return true;

                case "ArgumentLacing":
                    LacingStrategy strategy;
                    if (!Enum.TryParse(value, out strategy))
                        strategy = LacingStrategy.Disabled;
                    ArgumentLacing = strategy;
                    return true;

                case "IsVisible":
                    bool newVisibilityValue;
                    if (bool.TryParse(value, out newVisibilityValue))
                        IsVisible = newVisibilityValue;
                    return true;

                case "IsFrozen":
                    bool newIsFrozen;
                    if (bool.TryParse(value, out newIsFrozen))
                    {
                        IsFrozen = newIsFrozen;
                    }
                    return true;

                case "PreviewPinned":
                    bool newIsPinned;
                    if (bool.TryParse(value, out newIsPinned))
                    {
                        PreviewPinned = newIsPinned;
                    }
                    return true;

                case "UseLevels":
                    var parts = value.Split(new[] { ':' });
                    if (parts != null && parts.Count() == 2)
                    {
                        int portIndex;
                        bool useLevels;
                        if (int.TryParse(parts[0], out portIndex) &&
                            bool.TryParse(parts[1], out useLevels))
                        {
                            inPorts[portIndex].UseLevels = useLevels;
                        }
                    }
                    return true;

                case "KeepListStructure":
                    var keepListStructureInfos = value.Split(new[] { ':' });
                    if (keepListStructureInfos != null && keepListStructureInfos.Count() == 2)
                    {
                        int portIndex;
                        bool keepListStructure;
                        if (int.TryParse(keepListStructureInfos[0], out portIndex) &&
                            bool.TryParse(keepListStructureInfos[1], out keepListStructure))
                        {
                            inPorts[portIndex].KeepListStructure = keepListStructure;
                            if (keepListStructure)
                            {
                                // Only allow one input port to keep list structure
                                for (int i = 0; i < inPorts.Count; i++)
                                {
                                    if (portIndex != i && inPorts[i].KeepListStructure)
                                    {
                                        inPorts[i].KeepListStructure = false;
                                    }
                                }
                            }
                        }
                    }
                    return true;

                case "ChangeLevel":
                    var changeLevelInfos = value.Split(new[] { ':' });
                    if (changeLevelInfos != null && changeLevelInfos.Count() == 2)
                    {
                        int portIndex;
                        int level;
                        if (int.TryParse(changeLevelInfos[0], out portIndex) &&
                            int.TryParse(changeLevelInfos[1], out level))
                        {
                            inPorts[portIndex].Level = level;
                        }
                    }
                    return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        #endregion

        #region Serialization/Deserialization Methods

        /// <summary>
        /// The OnDeserializedMethod allows us to set this Node ports' Owner
        /// property after deserialization is complete. This allows us to
        /// avoid having to serialize the Owner property on the PortModel.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach(var p in OutPorts)
            {
                p.Owner = this;
                p.PortType = PortType.Output;
            }

            foreach(var p in InPorts)
            {
                p.Owner = this;
                p.PortType = PortType.Input;
            }
        }

        /// <summary>
        ///     Called when the node's Workspace has been saved.
        /// </summary>
        protected internal virtual void OnSave() { }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);

            if (context != SaveContext.Copy)
                helper.SetAttribute("guid", GUID);

            // Set the type attribute
            helper.SetAttribute("type", GetType());
            helper.SetAttribute("nickname", Name);
            helper.SetAttribute("x", X);
            helper.SetAttribute("y", Y);
            helper.SetAttribute("isVisible", IsVisible);
            helper.SetAttribute("lacing", ArgumentLacing.ToString());
            helper.SetAttribute("isSelectedInput", IsSetAsInput.ToString());
            helper.SetAttribute("isSelectedOutput", IsSetAsOutput.ToString());
            helper.SetAttribute("IsFrozen", isFrozenExplicitly);
            helper.SetAttribute("isPinned", PreviewPinned);

            var portIndexTuples = inPorts.Select((port, index) => new { port, index });

            //write port information
            foreach (var t in portIndexTuples)
            {
                XmlElement portInfo = element.OwnerDocument.CreateElement("PortInfo");
                portInfo.SetAttribute("index", t.index.ToString(CultureInfo.InvariantCulture));
                portInfo.SetAttribute("default", t.port.UsingDefaultValue.ToString());

                if (t.port.UseLevels)
                {
                    portInfo.SetAttribute("useLevels", t.port.UseLevels.ToString());
                    portInfo.SetAttribute("level", t.port.Level.ToString());
                    portInfo.SetAttribute("shouldKeepListStructure", t.port.KeepListStructure.ToString());
                }
                element.AppendChild(portInfo);
            }

            // Fix: MAGN-159 (nodes are not editable after undo/redo).
            if (context == SaveContext.Undo)
            {
                //helper.SetAttribute("interactionEnabled", interactionEnabled);
                helper.SetAttribute("nodeState", state.ToString());
            }

            if (context == SaveContext.File)
                OnSave();
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            var helper = new XmlElementHelper(nodeElement);

            if (context != SaveContext.Copy)
                GUID = helper.ReadGuid("guid", GUID);

            // Resolve node nick name.
            string name = helper.ReadString("nickname", string.Empty);
            if (!string.IsNullOrEmpty(name))
                this.name = name;
            else
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), true);
                var attrib = attribs[0] as NodeNameAttribute;
                if (null != attrib)
                    this.name = attrib.Name;
            }

            X = helper.ReadDouble("x", 0.0);
            Y = helper.ReadDouble("y", 0.0);
            isVisible = helper.ReadBoolean("isVisible", true);
            argumentLacing = helper.ReadEnum("lacing", LacingStrategy.Disabled);
            IsSetAsInput = helper.ReadBoolean("isSelectedInput", false);
            IsSetAsOutput = helper.ReadBoolean("isSelectedOutput", false);
            isFrozenExplicitly = helper.ReadBoolean("IsFrozen", false);
            PreviewPinned = helper.ReadBoolean("isPinned", false);

            var portInfoProcessed = new HashSet<int>();

            //read port information
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "PortInfo")
                {
                    int index = int.Parse(subNode.Attributes["index"].Value);
                    if (index < InPorts.Count)
                    {
                        portInfoProcessed.Add(index);

                        var attrValue = subNode.Attributes["default"];
                        if (attrValue != null)
                        {
                            bool def = false;
                            bool.TryParse(subNode.Attributes["default"].Value, out def);
                            inPorts[index].UsingDefaultValue = def;
                        }

                        attrValue = subNode.Attributes["useLevels"];
                        bool useLevels = false;
                        if (attrValue != null)
                        {
                            bool.TryParse(attrValue.Value, out useLevels);
                        }
                        inPorts[index].UseLevels = useLevels;

                        attrValue = subNode.Attributes["shouldKeepListStructure"];
                        bool shouldKeepListStructure = false;
                        if (attrValue != null)
                        {
                            bool.TryParse(attrValue.Value, out shouldKeepListStructure);
                        }
                        inPorts[index].KeepListStructure = shouldKeepListStructure;

                        attrValue = subNode.Attributes["level"];
                        if (attrValue != null)
                        {
                            int level = 1;
                            int.TryParse(attrValue.Value, out level);
                            InPorts[index].Level = level;
                        }
                    }
                }
            }

            //set defaults
            foreach (
                var port in
                    inPorts.Select((x, i) => new { x, i }).Where(x => !portInfoProcessed.Contains(x.i)))
                port.x.UsingDefaultValue = false;

            if (context == SaveContext.Undo)
            {
                // Fix: MAGN-159 (nodes are not editable after undo/redo).
                //interactionEnabled = helper.ReadBoolean("interactionEnabled", true);
                state = helper.ReadEnum("nodeState", ElementState.Active);

                // We only notify property changes in an undo/redo operation. Normal
                // operations like file loading or copy-paste have the models created
                // in different ways and their views will always be up-to-date with
                // respect to their models.
                RaisePropertyChanged("InteractionEnabled");
                RaisePropertyChanged("State");
                RaisePropertyChanged("Name");
                RaisePropertyChanged("ArgumentLacing");
                RaisePropertyChanged("IsVisible");
                 
                //we need to modify the downstream nodes manually in case the
                //undo is for toggling freeze. This is ONLY modifying the execution hint.
                // this does not run the graph.
                RaisePropertyChanged("IsFrozen");
                MarkDownStreamNodesAsModified(this);

                // Notify listeners that the position of the node has changed,
                // then all connected connectors will also redraw themselves.
                ReportPosition();

            }
        }

        #endregion

        #region Dirty Management
        //TODO: Refactor Property into Automatic with private(?) setter
        //TODO: Add RequestRecalc() method to replace setter --steve

        /// <summary>
        /// Execution scenarios for a Node to be re-executed
        /// </summary>
        [Flags]
        protected enum ExecutionHints
        {
            None = 0,
            Modified = 1,       // Marks as modified, but execution is optional if AST is unchanged.
            ForceExecute = 3    // Marks as modified, force execution even if AST is unchanged.
        }

        private ExecutionHints executionHint;

        [JsonIgnore]
        public bool IsModified
        {
            get { return GetExecutionHintsCore().HasFlag(ExecutionHints.Modified); }
        }

        [JsonIgnore]
        public bool NeedsForceExecution
        {
            get { return GetExecutionHintsCore().HasFlag(ExecutionHints.ForceExecute); }
        }

        public void MarkNodeAsModified(bool forceExecute = false)
        {
            executionHint = ExecutionHints.Modified;

            if (forceExecute)
                executionHint |= ExecutionHints.ForceExecute;
        }

        public void ClearDirtyFlag()
        {
            executionHint = ExecutionHints.None;
        }

        protected virtual ExecutionHints GetExecutionHintsCore()
        {
            return executionHint;
        }
        #endregion

        #region Visualization Related Methods

        /// <summary>
        /// Call this method to update the cached MirrorData for this NodeModel.
        /// Note this method should be called from scheduler thread.
        /// </summary>
        ///
        internal void RequestValueUpdate(EngineController engine)
        {
            // A NodeModel should have its cachedMirrorData reset when it is
            // requested to update its value. When the QueryMirrorDataAsyncTask
            // returns, it will update cachedMirrorData with the latest value.
            //
            lock (cachedValueMutex)
            {
                cachedValue = null;
            }

            // Do not have an identifier for preview right now. For an example,
            // this can be happening at the beginning of a code block node creation.
            var variableName = AstIdentifierForPreview.Value;
            if (string.IsNullOrEmpty(variableName))
                return;

            var runtimeMirror = engine.GetMirror(variableName);
            if (runtimeMirror != null)
            {
                CachedValue = runtimeMirror.GetData();
            }
        }

        /// <summary>
        /// Call this method to asynchronously regenerate render package for
        /// this node. This method accesses core properties of a NodeModel and
        /// therefore is typically called on the main/UI thread.
        /// </summary>
        /// <param name="scheduler">An IScheduler on which the task will be scheduled.</param>
        /// <param name="engine">The EngineController which will be used to get MirrorData for the node.</param>
        /// <param name="factory">An IRenderPackageFactory which will be used to generate IRenderPackage objects.</param>
        /// <param name="forceUpdate">Normally, render packages are only generated when the node's IsUpdated parameter is true.
        /// By setting forceUpdate to true, the render packages will be updated.</param>
        /// <returns>Flag which indicates if geometry update has been scheduled</returns>
        public virtual bool RequestVisualUpdateAsync(IScheduler scheduler,
            EngineController engine, IRenderPackageFactory factory, bool forceUpdate = false)
        {
            var initParams = new UpdateRenderPackageParams()
            {
                Node = this,
                RenderPackageFactory = factory,
                EngineController = engine,
                DrawableIdMap = GetDrawableIdMap(),
                PreviewIdentifierName = AstIdentifierForPreview.Name,
                ForceUpdate = forceUpdate
            };

            var task = new UpdateRenderPackageAsyncTask(scheduler);
            if (!task.Initialize(initParams)) return false;

            task.Completed += OnRenderPackageUpdateCompleted;
            scheduler.ScheduleForExecution(task);
            return true;
        }

        /// <summary>
        /// This event handler is invoked when UpdateRenderPackageAsyncTask is
        /// completed, at which point the render packages (specific to this node)
        /// become available.
        /// </summary>
        /// <param name="asyncTask">The instance of UpdateRenderPackageAsyncTask
        /// that was responsible of generating the render packages.</param>
        ///
        private void OnRenderPackageUpdateCompleted(AsyncTask asyncTask)
        {
            var task = asyncTask as UpdateRenderPackageAsyncTask;
            var packages = new RenderPackageCache();

            if (!task.RenderPackages.IsEmpty())
            {
                packages.Add(task.RenderPackages);
                packages.Add(OnRequestRenderPackages());
            }

            OnRenderPackagesUpdated(packages);
        }

        /// <summary>
        ///
        /// </summary>
        public event Func<RenderPackageCache> RequestRenderPackages;

        /// <summary>
        /// This event handler is invoked when the render packages (specific to this node)
        /// become available and in addition the node requests for associated render packages
        /// if any for example, packages used for associated node manipulators
        /// </summary>
        private RenderPackageCache OnRequestRenderPackages()
        {
            if (RequestRenderPackages != null)
            {
                return RequestRenderPackages();
            }

            return new RenderPackageCache();
        }

        /// <summary>
        /// Returns a map of output port GUIDs and drawable Ids as registered 
        /// with visualization manager for all the output ports of the given node.
        /// </summary>
        /// <returns>List of Drawable Ids</returns>
        private IEnumerable<KeyValuePair<Guid, string>> GetDrawableIdMap()
        {
            var idMap = new Dictionary<Guid, string>();
            for (int index = 0; index < OutPorts.Count; ++index)
            {
                string id = GetDrawableId(index);
                if (!string.IsNullOrEmpty(id))
                {
                    Guid originId = OutPorts[index].GUID;
                    idMap[originId] = id;
                }
            }

            return idMap;
        }

        /// <summary>
        /// Returns the drawable Id as registered with visualization manager for
        /// the given output port on the given node.
        /// </summary>
        /// <param name="outPortIndex">Output port index</param>
        /// <returns>Drawable Id</returns>
        private string GetDrawableId(int outPortIndex)
        {
            var output = GetAstIdentifierForOutputIndex(outPortIndex);
            return output == null ? null : output.Value;
        }

        #endregion

        #region Node Migration Helper Methods

        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string name, string funcName)
        {
            return MigrateToDsFunction(data, "", name, funcName);
        }

        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string assembly, string name, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateFunctionNodeFrom(xmlNode);
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", name);
            element.SetAttribute("function", funcName);

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }

        protected static NodeMigrationData MigrateToDsVarArgFunction(
            NodeMigrationData data, string assembly, string name, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateVarArgFunctionNodeFrom(xmlNode);
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", name);
            element.SetAttribute("function", funcName);

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }

        [NodeMigration(version: "1.4.0.0")]
        public static NodeMigrationData MigrateShortestLacingToAutoLacing(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            XmlElement node = data.MigratedNodes.ElementAt(0);
            MigrationManager.ReplaceAttributeValue(node, "lacing", "Shortest", "Auto");
            migrationData.AppendNode(node);
            return migrationData;
        }

        #endregion

        [JsonIgnore]
        public bool ShouldDisplayPreview
        {
            get
            {
                return ShouldDisplayPreviewCore;
            }
        }

        protected bool ShouldDisplayPreviewCore { get; set; }

        public event Action<NodeModel, RenderPackageCache> RenderPackagesUpdated;

        private void OnRenderPackagesUpdated(RenderPackageCache packages)
        {
            if (RenderPackagesUpdated != null)
            {
                RenderPackagesUpdated(this, packages);
            }
        }
    }

    /// <summary>
    /// Represents nodes states.
    /// </summary>
    public enum ElementState
    {
        Dead,
        Active,
        Warning,
        PersistentWarning,
        Error,
        AstBuildBroken
    };

    /// <summary>
    /// Defines Lacing strategy for nodes.
    /// Learn more about lacing here: http://dynamoprimer.com/06_Designing-with-Lists/6-1_whats-a-list.html
    /// </summary>
    public enum LacingStrategy
    {
        Disabled,
        First,
        Shortest,
        Longest,
        CrossProduct,
        Auto
    };

    /// <summary>
    /// Defines Enum for Mouse events.
    /// Used in port snapping.
    /// </summary>
    public enum PortEventType
    {
        MouseEnter,
        MouseLeave,
        MouseLeftButtonDown
    };

    /// <summary>
    /// Returns one of the possible values(none, top, bottom) where a port can be snapped.
    /// </summary>
    [Flags]
    public enum SnapExtensionEdges
    {
        None,
        Top = 0x1,
        Bottom = 0x2
    }

    internal delegate void DispatchedToUIThreadHandler(object sender, UIDispatcherEventArgs e);

    /// <summary>
    /// This class represents the UIDIspatcher thread event arguments.
    /// </summary>
    public class UIDispatcherEventArgs : EventArgs
    {
        /// <summary>
        /// Creates UIDispatcherEventArgs.
        /// </summary>
        /// <param name="a">action to call on UI thread</param>
        public UIDispatcherEventArgs(Action a)
        {
            ActionToDispatch = a;
        }

        /// <summary>
        /// Action to call on UI thread.
        /// </summary>
        public Action ActionToDispatch { get; set; }
    }
}
