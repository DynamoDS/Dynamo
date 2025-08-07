using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.Properties;
using Dynamo.Selection;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dynamo.Graph.Annotations
{
    /// <summary>
    /// This class contains methods and properties used for creating groups in Dynamo.
    /// </summary>
    public class AnnotationModel : ModelBase
    {
        private const double DoubleValue = 0.0;
        private const double MinTextHeight = 20.0;
        private const double ExtendSize = 10.0;
        private const double ExtendYHeight = 5.0;
        private const double NoteYAdjustment = 8.0;
        private bool isTextChanging;
        internal bool IsThumbResizing;

        #region Properties

        /// <summary>
        /// Triggers when it needs to get the model to add from Workspace
        /// </summary>
        public event Func<Guid, ModelBase> ModelBaseRequested;

        /// <summary>
        /// Required to calculate the TOP position in a group
        /// </summary>
        public double InitialTop { get; set; }

        /// <summary>
        /// Required to calculate the HEIGHT of a group
        /// </summary>
        public double InitialHeight { get; set; }

        /// <summary>
        /// Returns default background of the group
        /// </summary>
        public string GroupBackground = "#FFC1D676";

        /// <summary>
        /// DeletedModelBases is used to keep track of deleted / ungrouped models. 
        /// During Undo operations this is used to get those models that are deleted from the group
        /// </summary>
        public List<ModelBase> DeletedModelBases { get; set; }

        /// <summary>
        /// Indicates if group properties should be read from xml data
        /// </summary>
        public bool loadFromXML { get; set; }

        private double width;
        /// <summary>
        /// Returns width of the group
        /// </summary>
        public override double Width
        {
            get
            {
                return width;
            }
            set
            {
                if (width == value) return;

                width = value;
                RaisePropertyChanged(nameof(Width));
            }
        }

        private double height;
        /// <summary>
        /// Returns the full height of the group.
        /// That means ModelAreaHeight + TextBlockHeight 
        /// </summary>
        public override double Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height == value) return;

                height = value;
                RaisePropertyChanged(nameof(Height));
            }
        }

        private double modelAreaHeight;
        /// <summary>
        /// Returns the height of the area that all
        /// model belonging to this group is encapsulated in.
        /// </summary>
        public double ModelAreaHeight
        {
            get => modelAreaHeight;
            set
            {
                modelAreaHeight = value;
                RaisePropertyChanged(nameof(ModelAreaHeight));
            }
        }

        private string text;

        /// <summary>
        /// Returns text of the group
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged(nameof(Text));
            }
        }

        private string annotationText;
        /// <summary>
        /// Returns title of the group
        /// </summary>
        public string AnnotationText
        {
            get { return annotationText; }
            set
            {
                annotationText = value;               
                RaisePropertyChanged(nameof(AnnotationText));
            }

        }

        private string annotationDescriptionText;
        /// <summary>
        /// Group description text
        /// </summary>
        public string AnnotationDescriptionText
        {
            get => annotationDescriptionText;
            set
            {
                annotationDescriptionText = value;
                RaisePropertyChanged(nameof(AnnotationDescriptionText));
            }
        }

        private string background;
        /// <summary>
        /// Returns background of the group
        /// </summary>
        public string Background
        {
            get { return background ?? GroupBackground; }
            set
            {
                background = value;
                RaisePropertyChanged(nameof(Background));
            }
        }

        private HashSet<ModelBase> nodes;
        /// <summary>
        /// Returns collection of models (nodes and notes) which the group contains
        /// </summary>
        public IEnumerable<ModelBase> Nodes
        {
            get { return nodes; }
            set
            {
                // Unsubscribe all content in group before
                // overwriting with the new content.
                // If we dont do this we end up with
                // lots of memory leaks that eventually will
                // lead to a stackoverflow exception
                if (nodes != null && nodes.Any())
                {
                    foreach (var model in nodes)
                    {
                        model.PropertyChanged -= model_PropertyChanged;
                        model.Disposed -= model_Disposed;
                    }
                }

                // First separate all pins from the input
                var pinModels = value.OfType<ConnectorPinModel>().ToList();
                var valuesWithoutPins = value.Except(pinModels);

                // Then recalculate which pins belong to the group based on the nodes
                var pinsFromNodes = GetPinsFromNodes(valuesWithoutPins.OfType<NodeModel>());

                // Then filter out pins that have been marked as removed
                pinsFromNodes = pinsFromNodes.Where(p => !removedPins.Contains(p.GUID)).ToArray();  

                // Combine all
                nodes = valuesWithoutPins.Concat(pinModels).Concat(pinsFromNodes).ToHashSet<ModelBase>();

                if (nodes != null && nodes.Any())
                {
                    foreach (var model in nodes)
                    {
                        model.PropertyChanged += model_PropertyChanged;
                        model.Disposed += model_Disposed;
                    }
                }
                UpdateBoundaryFromSelection();
                RaisePropertyChanged(nameof(Nodes));
            }
        }

        /// <summary>
        /// ID of the AnnotationModel, which is unique within the graph.
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
        /// Overriding the Rect from Modelbase
        /// This queries the actual RECT of the group. 
        /// This is required to make the group as ILocatable.
        /// </summary>      
        public override Rect2D Rect
        {
            get { return new Rect2D(this.X, this.Y, this.Width, this.Height); }
        }

        private double textBlockHeight;
        /// <summary>
        /// Returns height of the text area of the group
        /// </summary>
        public double TextBlockHeight
        {
            get { return textBlockHeight; }
            set
            {
                textBlockHeight = value;
                //Increase the Y value by 10. This provides the extra space between
                // a model and textbox. Otherwise there will be some overlap
                Y = InitialTop - ExtendSize - textBlockHeight;
                Height = InitialHeight + textBlockHeight - MinTextHeight;

                isTextChanging = true;
                UpdateBoundaryFromSelection();
                isTextChanging = false;
            }
        }

        private double textMaxWidth;
        /// <summary>
        /// Returns the maxWidth of text area of the group
        /// </summary>
        public double TextMaxWidth
        {
            get { return textMaxWidth; }
            set
            {
                textMaxWidth = value;
            }
        }

        private double fontSize = 36;
        /// <summary>
        /// Returns font size of the text of the group
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                RaisePropertyChanged(nameof(FontSize));
            }
        }

        private Guid groupStyleId;
        /// <summary>
        /// Returns the Groupstyle applied
        /// </summary>
        public Guid GroupStyleId
        {
            get
            {
                return groupStyleId;
            }
            set
            {
                groupStyleId = value;
                RaisePropertyChanged(nameof(GroupStyleId));
            }
        }

        private NodeModel pinnedNode;
        
        /// <summary>
        /// Optional reference to pinned node
        /// This reference will be used in note serialization
        /// as note is deserialized from an annotation model
        /// </summary>
        public NodeModel PinnedNode
        {
            get { return pinnedNode; }
            set
            {
                pinnedNode = value;
                RaisePropertyChanged(nameof(PinnedNode));
            }
        }

        /// <summary>
        /// Stores the GUIDs of connector pins that have been marked as removed from the group.
        /// </summary>
        private HashSet<Guid> removedPins = new HashSet<Guid>();

        private double widthAdjustment;
        /// <summary>
        /// Adjustment margin to be added to the width of the
        /// group. When set the width of the group will always
        /// be set to Width + widthAdjustment
        /// </summary>
        public double WidthAdjustment
        {
            get { return widthAdjustment; }
            set 
            {
                if (value == widthAdjustment) return;
                widthAdjustment = value;

                // update boundary only while manually resizing the group
                if (IsThumbResizing)
                    UpdateBoundaryFromSelection();
            }
        }

        private double heightAdjustment;
        /// <summary>
        /// Adjustment margin to be added to the height of the
        /// group. When set the height of the group will always
        /// be set to Height + heightAdjustment
        /// </summary>
        public double HeightAdjustment
        {
            get { return heightAdjustment; }
            set 
            {
                if (value == heightAdjustment) return;
                heightAdjustment = value;

                // update boundary only while manually resizing the group
                if (IsThumbResizing)
                    UpdateBoundaryFromSelection();
            }
        }

        private double userSetHeight;
        /// <summary>
        /// Indicates the height the user manually set using the resize thumb.
        /// </summary>
        public double UserSetHeight
        {
            get => userSetHeight;
            set
            {
                if (value == userSetHeight) return;
                userSetHeight = value;
            }
        }

        private double userSetWidth;
        /// <summary>
        /// Indicates the height the user manually set using the resize thumb.
        /// Indicates the width the user manually set using the resize thumb.
        /// Not necessarily equal to the actual rendered width.
        /// </summary>  
        public double UserSetWidth
        {
            get => userSetWidth;
            set
            {
                if (value == userSetWidth) return;
                userSetWidth = value;
            }
        }

        private bool isExpanded = true;
        /// <summary>
        /// Returns whether or not the group is expanded.
        /// </summary>
        public bool IsExpanded 
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                UpdateBoundaryFromSelection();
                UpdateErrorAndWarningIconVisibility();
            }
        }

        private ElementState groupState = ElementState.Active;

        /// <summary>
        /// Indicates whether the group contains nodes that are in an info/warning/error state.
        /// This includes the state of any nodes that are in nested groups.
        /// </summary>
        [JsonIgnore]
        public ElementState GroupState
        {
            get => groupState;
            internal set
            {
                groupState = value;
                RaisePropertyChanged(nameof(GroupState));
            }
        }

        /// <summary>
        /// Checks if this group contains any nested groups.
        /// </summary>
        public bool HasNestedGroups => nodes.OfType<AnnotationModel>().Any();

        private bool isVisible = true;
        /// <summary>
        /// Preview visibility of the nodes in a group
        /// </summary>
        [JsonIgnore]
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            internal set
            {
                if (value != isVisible)
                {
                    isVisible = value;
                    RaisePropertyChanged(nameof(IsVisible));
                }
            }
        }

        private bool isFrozen = false;
        /// <summary>
        /// Returns whether or not all nodes in the group are frozen.
        /// </summary>
        [JsonIgnore]
        public bool IsFrozen
        {
            get
            {
                return isFrozen;
            }
            internal set
            {
                if (value != isFrozen)
                {
                    isFrozen = value;
                    RaisePropertyChanged(nameof(IsFrozen));
                }
            }
        }

        private bool isOptionalInPortsCollapsed;
        /// <summary>
        /// Indicates whether optional input ports were manually expanded or collapsed when the graph was last saved.
        /// Used only for serialization.
        /// </summary>
        public bool IsOptionalInPortsCollapsed
        {
            get => isOptionalInPortsCollapsed;
            set
            {
                if (isOptionalInPortsCollapsed == value) return;
                isOptionalInPortsCollapsed = value;
            }
        }

        private bool isUnconnectedOutPortsCollapsed;
        /// <summary>
        /// Indicates whether unconnected output ports were manually expanded or collapsed when the graph was last saved.
        /// Used only for serialization.
        /// </summary>
        public bool IsUnconnectedOutPortsCollapsed
        {
            get => isUnconnectedOutPortsCollapsed;
            set
            {
                if (isUnconnectedOutPortsCollapsed == value) return;
                isUnconnectedOutPortsCollapsed = value;
            }

        }

        private bool hasToggledOptionalInPorts;
        /// <summary>
        /// Indicates whether the user manually toggled the visibility of optional input ports.
        /// If true, this overrides the global preference setting.
        /// </summary>
        public bool HasToggledOptionalInPorts
        {
            get => hasToggledOptionalInPorts;
            set
            {
                if (hasToggledOptionalInPorts == value) return;
                hasToggledOptionalInPorts = value;
            }
        }

        private bool hasToggledUnconnectedOutPorts;
        /// <summary>
        /// Indicates whether the user manually toggled the visibility of unconnected output ports.
        /// If true, this overrides the global preference setting.
        /// </summary>
        public bool HasToggledUnconnectedOutPorts
        {
            get => hasToggledUnconnectedOutPorts;
            set
            {
                if (hasToggledUnconnectedOutPorts == value) return;
                hasToggledUnconnectedOutPorts = value;
            }
        }

        private bool isCollapsedToMinSize;
        /// <summary>
        /// Gets or sets a value indicating whether the group was manually resized while collapsed
        /// </summary>
        public bool IsCollapsedToMinSize
        {
            get => isCollapsedToMinSize;
            set
            {
                if (isCollapsedToMinSize == value) return;
                isCollapsedToMinSize = value;
            }
        }

        private bool suppressBoundaryUpdate;
        /// <summary>
        /// A temporary flag used to suppress boundary updates while internal operations,
        /// such as connector redrawing, are in progress.
        /// Should be set to true only during those operations to avoid redundant or recursive updates.
        /// </summary>
        internal bool SuppressBoundaryUpdate
        {
            get => suppressBoundaryUpdate;
            set
            {
                if (value == suppressBoundaryUpdate) return;
                suppressBoundaryUpdate = value;
            }
        }

        private double minWidthOnCollapsed;
        /// <summary>
        /// Gets or sets the minimum width of the group when it is collapsed. 
        /// This value equals the combined width of the group's proxy input and output ports.
        /// </summary>
        public double MinWidthOnCollapsed
        {
            get => minWidthOnCollapsed;
            set
            {
                if (minWidthOnCollapsed == value) return;
                minWidthOnCollapsed = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationModel"/> class.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="notes">The notes.</param>
        public AnnotationModel(IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes) : this(nodes, notes, new List<AnnotationModel>()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationModel"/> class.
        /// </summary>
        /// <param name="nodes">The nodes that belongs to this group.</param>
        /// <param name="notes">The notes that belongs to this group.</param>
        /// <param name="groups">The groups that belongs to this group</param>
        public AnnotationModel(IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes, IEnumerable<AnnotationModel> groups)
        {
            var nodeModels = nodes as NodeModel[] ?? nodes.ToArray();
            var noteModels = notes as NoteModel[] ?? notes.ToArray();
            var groupModels = groups as AnnotationModel[] ?? groups.ToArray();

            DeletedModelBases = new List<ModelBase>();
            this.Nodes = nodeModels
                .Concat(noteModels.Cast<ModelBase>())
                .Concat(groupModels.Cast<ModelBase>())
                .ToList();

            UpdateBoundaryFromSelection();
            UpdateErrorAndWarningIconVisibility();
        }

        private ConnectorPinModel[] GetPinsFromNodes(IEnumerable<NodeModel> nodeModels)
        {
            if (nodeModels is null ||
                !nodeModels.Any())
            {
                return new List<ConnectorPinModel>().ToArray();
            }

            var connectorPinsToAdd = nodeModels
                .SelectMany(x => x.AllConnectors)
                .Where(x => nodeModels.Contains(x.Start.Owner) && nodeModels.Contains(x.End.Owner))
                .SelectMany(x => x.ConnectorPinModels)
                .Distinct()
                .ToArray();

            return connectorPinsToAdd;
        }

        /// <summary>
        /// Marks a connector pin as removed by adding its GUID to the removedPins set.
        /// </summary>
        internal void MarkPinAsRemoved(ConnectorPinModel pin)
        {
            removedPins.Add(pin.GUID);
        }

        /// <summary>
        /// Clears the set of removed connector pins.
        /// </summary>
        private void ClearRemovedPins()
        {
            removedPins.Clear();
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Skip boundary updates caused by connector redraws if group is collapsed
            if (!IsExpanded && SuppressBoundaryUpdate && e.PropertyName == nameof(Position))
                return;

            switch (e.PropertyName)
            {
                case nameof(Position):
                    UpdateBoundaryFromSelection();
                    break;
                case nameof(Text):
                    UpdateBoundaryFromSelection();
                    break;
                case nameof(ModelBase.Height):
                case nameof(ModelBase.Width):
                    UpdateBoundaryFromSelection();
                    break;
                case nameof(NodeModel.State):
                    UpdateErrorAndWarningIconVisibility();
                    break;
                case nameof(NodeModel.IsFrozen):
                    UpdateGroupFrozenStatus();
                    break;
            }
        }

        /// <summary>
        /// Recalculate the group when a node is disposed
        /// </summary>
        /// <param name="model"></param>
        private void model_Disposed(ModelBase model)
        {
            var modelList = this.Nodes.ToList();
            bool remove = modelList.Remove(model);
            if (remove)
            {
                DeletedModelBases.Add(model);
                Nodes = modelList;
                UpdateBoundaryFromSelection();
            }
        }

        /// <summary>
        /// The method updates the group frozen status. If all nodes and nested groups are frozen, it sets to true, else false.
        /// </summary>
        /// <returns>Frozen state of a group</returns>
        internal void UpdateGroupFrozenStatus()
        {
            // Check for any non-frozen node in the group
            var nonFrozenNodeInGroup = this.Nodes.OfType<NodeModel>().Any(x => !x.IsFrozen);
            if (nonFrozenNodeInGroup)
            {
                this.IsFrozen = false;
                return;
            }

            // Check for any non-frozen nested group in the group
            var nonFrozenGroupInGroup = this.Nodes.OfType<AnnotationModel>().Any(x => !x.IsFrozen);
            if (nonFrozenGroupInGroup)
            {
                this.IsFrozen = false;
                return;
            }

            this.IsFrozen = true;
        }

        /// <summary>
        /// Updates the group boundary based on the nodes / notes selection.
        /// </summary>
        internal void UpdateBoundaryFromSelection()
        {
            var selectedModelsList = nodes.ToList();
            if (!selectedModelsList.Any())
            {
                // No models in the group — set dimensions to zero
                Width = 0;
                Height = 0;
                return;
            }

            // Sort models left to right for consistent calculations
            var groupModels = selectedModelsList.OrderBy(x => x.X).ToList();

            // Determine left boundary (smallest X), shifted left by padding
            double regionX = groupModels.Min(x => x.X) - ExtendSize;

            // Determine top boundary, adjusted for note offset and text block height
            double regionY = groupModels.Min(y => (y as NoteModel) == null ? y.Y : y.Y - NoteYAdjustment)
                             - ExtendSize
                             - (TextBlockHeight == 0.0 ? MinTextHeight : TextBlockHeight);

            // Compute the horizontal span of all models
            double xDistance = groupModels.Max(x => x.X + x.Width) - regionX;

            // Save the actual top-most Y value (before subtracting text block height)
            this.InitialTop = groupModels.Min(y => (y as NoteModel) == null ? y.Y : y.Y - NoteYAdjustment);

            // Track whether position has changed
            bool positionChanged = regionX != X || regionY != Y;
            X = regionX;
            Y = regionY;

            // Use different logic for expanded vs. collapsed state
            if (IsExpanded)
            {
                UpdateExpandedLayout(groupModels, regionX, regionY, xDistance);
            }
            else
            {
                UpdateCollapsedLayout(xDistance);
            }

            // Notify UI if position changed
            if (positionChanged)
                RaisePropertyChanged(nameof(Position));
        }

        /// <summary>
        /// Calculates and sets the group size and bounds when the group is expanded.
        /// Includes full height of contained models and padding.
        /// </summary>
        private void UpdateExpandedLayout(List<ModelBase> groupModels, double regionX, double regionY, double xDistance)
        {
            // Compute total vertical height of models in group
            double yDistance = groupModels.Max(y => (y as NoteModel) == null ? y.Y + y.Height : y.Y + y.Height - NoteYAdjustment)
                               - regionY;

            // Define the full rectangular area of the group (excluding text block)
            var region = new Rect2D
            {
                X = regionX,
                Y = regionY,
                Width = xDistance + ExtendSize + Math.Max(WidthAdjustment, 0),
                Height = yDistance + ExtendSize + ExtendYHeight + HeightAdjustment - TextBlockHeight
            };

            // The actual space used by nodes (without adjustments)
            double groupCalculatedWidth = xDistance + ExtendSize;
            double groupCalculatedHeight = yDistance + ExtendSize + ExtendYHeight - TextBlockHeight;

            if (IsThumbResizing || isTextChanging)
            {
                // While dragging the resize thumb or editing text
                ModelAreaHeight = region.Height;
                Height = ModelAreaHeight + TextBlockHeight;
                Width = Math.Max(region.Width, TextMaxWidth + ExtendSize);
            }
            else
            {
                // HEIGHT logic
                // user has not resized the group
                if (UserSetHeight <= 0) 
                {
                    ModelAreaHeight = groupCalculatedHeight;
                    Height = ModelAreaHeight + TextBlockHeight;
                }
                // some nodes are outside the user set size 
                else if (groupCalculatedHeight >= UserSetHeight)
                {
                    ModelAreaHeight = groupCalculatedHeight;
                    Height = ModelAreaHeight + TextBlockHeight;
                    HeightAdjustment = 0;
                }
                // all nodes are within the user set size
                else
                {
                    HeightAdjustment = Math.Max(0, UserSetHeight - groupCalculatedHeight);
                    ModelAreaHeight = UserSetHeight;
                    Height = UserSetHeight + TextBlockHeight;
                }

                // WIDTH logic
                // user has not resized the group
                if (UserSetWidth <= 0) 
                {
                    Width = Math.Max(region.Width, TextMaxWidth + ExtendSize);
                }
                // some nodes are outside the user set size 
                else if (groupCalculatedWidth >= UserSetWidth) 
                {
                    Width = Math.Max(groupCalculatedWidth, TextMaxWidth + ExtendSize);
                    WidthAdjustment = 0;
                }
                // all nodes are within the user set size
                else
                {
                    WidthAdjustment = Math.Max(0, UserSetWidth - groupCalculatedWidth);
                    Width = UserSetWidth;
                }
            }

            // Only store the first calculated initial height
            if (InitialHeight <= 0.0)
                InitialHeight = region.Height;
        }

        /// <summary>
        /// Calculates and sets the group size when collapsed.
        /// Supports two modes: full-width collapse and minimum-size collapse.
        /// </summary>
        private void UpdateCollapsedLayout(double xDistance)
        {
            // Choose width based on collapse preference
            if (!IsCollapsedToMinSize)
            {
                // Collapse vertically, keep full width
                Width = Math.Max(xDistance + ExtendSize + WidthAdjustment, TextMaxWidth + ExtendSize);
            }
            else
            {
                // Fully minimize the group
                Width = Math.Max(MinWidthOnCollapsed + ExtendSize, TextMaxWidth + ExtendSize);
            }

            Height = TextBlockHeight + ModelAreaHeight;
        }

        /// <summary>
        /// Determines whether this group displays warning or error icons in its header.
        /// </summary>
        private void UpdateErrorAndWarningIconVisibility()
        {
            // No icons are displayed when the group is expanded / not collapsed.
            if (IsExpanded)
            {
                GroupState = ElementState.Active;
                return;
            }

            List<NodeModel> nodes = Nodes
                .OfType<NodeModel>()
                .ToList();

            List<AnnotationModel> groups = Nodes
                .OfType<AnnotationModel>()
                .ToList();
                
            // If anything in this group is in an error state, we display an error icon.
            if (nodes.Any(x => x.State == ElementState.Error) ||
                groups.Any(x => x.GroupState == ElementState.Error))
            {
                GroupState = ElementState.Error;
                return;
            }

            // If anything in this group is in a warning state, we display a warning icon.
            if (nodes.Any(x => x.State == ElementState.Warning) ||
                groups.Any(x => x.GroupState == ElementState.Warning))
            {
                GroupState = ElementState.Warning;
                return;
            }

            GroupState = ElementState.Active;
        }

        /// <summary>
        /// Fired when this group is removed from its parent group
        /// </summary>
        internal event EventHandler RemovedFromGroup;

        private void OnRemovedFromGroup()
        {
            RemovedFromGroup?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fired when this group is added to another group
        /// </summary>
        internal event EventHandler AddedToGroup;

        private void OnAddedToGroup()
        {
            AddedToGroup?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Group the Models based on Height and Width
        /// </summary>
        /// <returns> the width and height of the last model </returns>

        #region Serialization/Deserialization Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case nameof(FontSize):
                    FontSize = Convert.ToDouble(value);
                    break;
                case nameof(GroupStyleId):
                    GroupStyleId = new Guid(value);
                    break;
                case nameof(Background):
                    Background = value;
                    break;  
                case "TextBlockText":
                    AnnotationText = value;
                    break;
                case nameof(AnnotationDescriptionText):
                    AnnotationDescriptionText = value;
                    break;
                case nameof(IsExpanded):
                    IsExpanded = Convert.ToBoolean(value);
                    break;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        protected override
             void SerializeCore(XmlElement element, SaveContext context)
        {            
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", this.GUID);
            helper.SetAttribute("annotationText", this.AnnotationText);
            helper.SetAttribute(nameof(AnnotationDescriptionText), this.AnnotationDescriptionText);
            helper.SetAttribute("left", this.X);
            helper.SetAttribute("top", this.Y);
            helper.SetAttribute("width", this.Width);
            helper.SetAttribute("height", this.Height);
            helper.SetAttribute("fontSize", this.FontSize);
            helper.SetAttribute("groupStyleId", this.GroupStyleId);
            helper.SetAttribute("InitialTop", this.InitialTop);
            helper.SetAttribute("InitialHeight", this.InitialHeight);
            helper.SetAttribute("TextblockHeight", this.TextBlockHeight);
            helper.SetAttribute("backgrouund", (this.Background == null ? "" : this.Background.ToString()));
            helper.SetAttribute(nameof(IsSelected), IsSelected);
            helper.SetAttribute(nameof(IsExpanded), this.IsExpanded);
            helper.SetAttribute(nameof(IsOptionalInPortsCollapsed), this.IsOptionalInPortsCollapsed);
            helper.SetAttribute(nameof(IsUnconnectedOutPortsCollapsed), this.IsUnconnectedOutPortsCollapsed);
            helper.SetAttribute(nameof(HasToggledOptionalInPorts), this.HasToggledOptionalInPorts);
            helper.SetAttribute(nameof(HasToggledUnconnectedOutPorts), this.HasToggledUnconnectedOutPorts);

            //Serialize Selected models
            XmlDocument xmlDoc = element.OwnerDocument;            
            foreach (var guids in this.Nodes.Select(x => x.GUID))
            {
                if (xmlDoc != null)
                {
                    var modelElement = xmlDoc.CreateElement("Models");
                    element.AppendChild(modelElement);
                    XmlElementHelper mhelper = new XmlElementHelper(modelElement);
                    modelElement.SetAttribute("ModelGuid", guids.ToString());
                }
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {         
            XmlElementHelper helper = new XmlElementHelper(element);
            this.GUID = helper.ReadGuid("guid", this.GUID);
            this.annotationText = helper.ReadString("annotationText", Resources.GroupNameDefaultText);
            this.AnnotationDescriptionText = helper.ReadString(nameof(AnnotationDescriptionText), Resources.GroupDefaultText);
            this.X = helper.ReadDouble("left", DoubleValue);
            this.Y = helper.ReadDouble("top", DoubleValue);
            this.width = helper.ReadDouble("width", DoubleValue);
            this.height = helper.ReadDouble("height", DoubleValue);
            this.background = helper.ReadString("backgrouund", "");
            this.fontSize = helper.ReadDouble("fontSize", fontSize);
            this.groupStyleId =  helper.ReadGuid("groupStyleId", GroupStyleId);
            this.textBlockHeight = helper.ReadDouble("TextblockHeight", DoubleValue);
            this.InitialTop = helper.ReadDouble("InitialTop", DoubleValue);
            this.InitialHeight = helper.ReadDouble("InitialHeight", DoubleValue);
            this.IsSelected = helper.ReadBoolean(nameof(IsSelected), false);
            this.IsExpanded = helper.ReadBoolean(nameof(IsExpanded), true);
            this.IsOptionalInPortsCollapsed = helper.ReadBoolean(nameof(IsOptionalInPortsCollapsed), true);
            this.IsUnconnectedOutPortsCollapsed = helper.ReadBoolean(nameof(IsUnconnectedOutPortsCollapsed), true);
            this.HasToggledOptionalInPorts = helper.ReadBoolean(nameof(HasToggledOptionalInPorts), false);
            this.HasToggledUnconnectedOutPorts = helper.ReadBoolean(nameof(HasToggledUnconnectedOutPorts), false);

            if (IsSelected)
                DynamoSelection.Instance.Selection.Add(this);
            else
                DynamoSelection.Instance.Selection.Remove(this);  

            //Deserialize Selected models
            if (element.HasChildNodes)
            {
                var removedModels = new List<ModelBase>();
                var listOfModels = new List<ModelBase>();
                if (Nodes != null)
                {
                    foreach (var childnode in element.ChildNodes)
                    {
                        var mhelper = new XmlElementHelper(childnode as XmlElement);
                        var result = mhelper.ReadGuid("ModelGuid", new Guid());
                        var model = ModelBaseRequested != null
                            ? ModelBaseRequested(result)
                            : Nodes.FirstOrDefault(x => x.GUID == result);

                        if (model != null)
                        {
                            listOfModels.Add(model);
                        }
                    }
                }

                removedModels = Nodes.Except(listOfModels).ToList();
                Nodes = listOfModels;

                foreach (var model in removedModels)
                {
                    UnsubscribeRemovedModel(model);
                }
            }

            //On any Undo Operation, current values are restored to previous values.
            //These properties should be Raised, so that they get the correct value on Undo.
            RaisePropertyChanged(nameof(Background));
            RaisePropertyChanged(nameof(FontSize));           
            RaisePropertyChanged(nameof(GroupStyleId));
            RaisePropertyChanged(nameof(AnnotationText));
            RaisePropertyChanged(nameof(Nodes));
            RaisePropertyChanged(nameof(IsExpanded));
            RaisePropertyChanged(nameof(IsOptionalInPortsCollapsed));
            RaisePropertyChanged(nameof(IsUnconnectedOutPortsCollapsed));
            this.ReportPosition();
        }

        /// <summary>
        /// This is called when a model is added to or deleted from a group.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="checkOverlap"> checkoverlap determines whether the selected model is 
        /// completely inside that group</param>
        internal void AddToTargetAnnotationModel(ModelBase model, bool checkOverlap = false)
        {
            if (model is null) return;
            if ((model as NodeModel)?.IsTransient == true) return;
            var list = this.Nodes.ToList();
            if (model.GUID == this.GUID) return;
            if (list.Where(x => x.GUID == model.GUID).Any()) return;
            if (!CheckModelIsInsideGroup(model, checkOverlap)) return;
            list.Add(model);
            this.Nodes = list;
            if (model is AnnotationModel annotationModel) annotationModel.OnAddedToGroup();
            this.UpdateBoundaryFromSelection();
            UpdateErrorAndWarningIconVisibility();
        }

        private void UnsubscribeRemovedModel(ModelBase model)
        {
            model.PropertyChanged -= model_PropertyChanged;
            model.Disposed -= model_Disposed;
            if (model is AnnotationModel annotationModel)
            {
                annotationModel.OnRemovedFromGroup();
            }
        }

        private bool CheckModelIsInsideGroup(ModelBase model, bool checkOverlap)
        {
            if (!checkOverlap) return true;
            var modelRect = model.Rect;
            if (this.Rect.Contains(modelRect))
            {
                return true;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Overriding the Select behavior
        /// because selecting the  group should select the models
        /// within that group
        /// </summary>
        public override void Select()
        {
            foreach (var models in Nodes)
            {
                if (models is AnnotationModel annotationModel)
                {
                    annotationModel.Select();
                    continue;
                }
                models.IsSelected = true;
            }

            base.Select();
        }

        /// <summary>
        /// Overriding the Deselect behavior
        /// because deselecting the  group should deselect the models
        /// within that group
        /// </summary>
        public override void Deselect()
        {           
            foreach (var model in Nodes)
            {
                model.IsSelected = false;
                // De-select all elements under the deleted group if there is any nested group
                if (model is AnnotationModel)
                {
                    (model as AnnotationModel).Deselect();
                }
            }

            base.Deselect();
        }

        /// <summary>
        /// Checks if the provided modelbase belongs
        /// to this group.
        /// </summary>
        /// <param name="modelBase">modelbase to check if belongs to this group</param>
        /// <returns></returns>
        public bool ContainsModel(ModelBase modelBase)
        {
            if (modelBase is null) return false;

            return nodes.Contains(modelBase);
        }

        /// <summary>
        /// Implementation of Dispose method
        /// </summary>
        public override void Dispose()
        {           
            if (this.Nodes.Any())
            {
                foreach (var model in this.Nodes)
                {
                    model.PropertyChanged -= model_PropertyChanged;
                    model.Disposed -= model_Disposed;
                }
            }
            ClearRemovedPins();
            base.Dispose();
        }     
    }   
}
