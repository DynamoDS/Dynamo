using System.Linq;
using Autodesk.DesignScript.Runtime;
using CoreNodes.ChartHelpers;
using CoreNodeModelsWpf.Charts.Controls;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Wpf.Properties;
using Dynamo.ViewModels;
using System.Collections.Generic;
using System;
using CoreNodeModelsWpf.Converters;
using System.ComponentModel;
using System.Collections;
using Dynamo.Graph.Connectors;

namespace CoreNodeModelsWpf.Charts
{
    [IsDesignScriptCompatible]
    [NodeName("Math.CurveMapper")]
    [NodeCategory("Math.Graph.Curve")]
    [NodeDescription("CurveMapperNodeDescription", typeof(CoreNodeModelWpfResources))]
    [NodeSearchTags("CurveMapperSearchTags", typeof(CoreNodeModelWpfResources))]
    //[InPortNames("x-MinLimit", "x-MaxLimit", "y-MinLimit", "y-MaxLimit", "count")]
    //[InPortTypes("double", "double", "double", "double", "double")]
    //[InPortDescriptions(typeof(CoreNodeModelWpfResources),
    //    "CurveMapperXMinLimitDataPortToolTip",
    //    "CurveMapperXMaxLimitDataPortToolTip",
    //    "CurveMapperYMinLimitDataPortToolTip",
    //    "CurveMapperYMaxLimitDataPortToolTip",
    //    "CurveMapperListDataPortToolTip")]
    //[OutPortNames("numbers")]
    //[OutPortTypes("List<double>")]
    //[OutPortDescriptions(typeof(CoreNodeModelWpfResources),
    //    "CurveMapperOutputDataPortToolTip")]
    [AlsoKnownAs("CoreNodeModelsWpf.Charts.CurveMapper")] // move to Math
    public class CurveMapperNodeModel : NodeModel
    {
        #region Properties
        private double minLimitX;
        private double maxLimitX = 1;
        private double minLimitY;
        private double maxLimitY = 1;
        private double pointsCount = 10;
        private readonly IntNode minLimitXDefaultValue = new IntNode(0);
        private readonly IntNode maxLimitXDefaultValue = new IntNode(1);
        private readonly IntNode minLimitYDefaultValue = new IntNode(0);
        private readonly IntNode maxLimitYDefaultValue = new IntNode(1);
        private readonly IntNode pointsCountDefaultValue = new IntNode(10);

        // Should those properties be serialized ???
        [JsonProperty(PropertyName = "MinLimitX")]
        public double MinLimitX
        {
            get => minLimitX;
            set
            {
                if (minLimitX != value)
                {
                    minLimitX = value;
                    this.RaisePropertyChanged(nameof(MinLimitX));
                    OnNodeModified();
                }                
            }
        }
        [JsonProperty(PropertyName = "MaxLimitX")]
        public double MaxLimitX
        {
            get => maxLimitX;
            set
            {
                if (maxLimitX != value)
                {
                    maxLimitX = value;
                    this.RaisePropertyChanged(nameof(MaxLimitX));
                    OnNodeModified();
                }
            }
        }
        [JsonProperty(PropertyName = "MinLimitY")]
        public double MinLimitY
        {
            get => minLimitY;
            set
            {
                if (minLimitY != value)
                {
                    minLimitY = value;
                    this.RaisePropertyChanged(nameof(MinLimitY));
                    OnNodeModified();
                }                
            }
        }
        [JsonProperty(PropertyName = "MaxLimitY")]
        public double MaxLimitY
        {
            get => maxLimitY;
            set
            {
                if (maxLimitY != value)
                {
                    maxLimitY = value;
                    this.RaisePropertyChanged(nameof(MaxLimitY));
                    OnNodeModified();
                }                
            }
        }
        public double MidValueX => (MaxLimitX + MinLimitX) * 0.5;
        public double MidValueY => (MaxLimitY + MinLimitY) * 0.5;
        [JsonProperty(PropertyName = "PointsCount")]
        public double PointsCount
        {
            get => pointsCount;
            set
            {
                if (pointsCount != value)
                {
                    pointsCount = value;
                    this.RaisePropertyChanged(nameof(PointsCount));
                    OnNodeModified();
                }
            }
        }

        /// <summary>
        /// Triggers when port is connected or disconnected
        /// </summary>
        public event EventHandler PortUpdated;

        protected virtual void OnPortUpdated(EventArgs args)
        {
            PortUpdated?.Invoke(this, args);
        }

        private GraphTypes selectedGraphType;
        public GraphTypes SelectedGraphType
        {
            get => selectedGraphType;
            set
            {
                selectedGraphType = value;
                RaisePropertyChanged(nameof(SelectedGraphType));
                OnNodeModified();
            }
        }
        #endregion

        #region Constructors

        public CurveMapperNodeModel()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("x-MinLimit",
                CoreNodeModelWpfResources.CurveMapperXMinLimitDataPortToolTip,
                minLimitXDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("x-MaxLimit",
                CoreNodeModelWpfResources.CurveMapperXMaxLimitDataPortToolTip,
                maxLimitXDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("y-MinLimit",
                CoreNodeModelWpfResources.CurveMapperYMinLimitDataPortToolTip,
                minLimitYDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("y-MaxLimit",
                CoreNodeModelWpfResources.CurveMapperYMaxLimitDataPortToolTip,
                maxLimitYDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("count",
                CoreNodeModelWpfResources.CurveMapperListDataPortToolTip,
                pointsCountDefaultValue)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("numbers",
                CoreNodeModelWpfResources.CurveMapperOutputDataPortToolTip)));

            RegisterAllPorts();

            //PortConnected += CurveMapperNodeModel_PortConnected;
            //PortDisconnected += CurveMapperNodeModel_PortDisconnected;

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            SelectedGraphType = GraphTypes.Empty;
            ArgumentLacing = LacingStrategy.Disabled;
        }
        [JsonConstructor]
        public CurveMapperNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            //PortDisconnected += GraphMapNodeModel_PortDisconnected;
            //PropertyChanged += GraphMapNodeModel_PropertyChanged;

            ArgumentLacing = LacingStrategy.Disabled;
        }

        #endregion

        #region DataBridge
        /// <summary>
        /// Register the data bridge callback.
        /// </summary>
        protected override void OnBuilt()
        {
            base.OnBuilt();
            VMDataBridge.DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }
        private void DataBridgeCallback(object data)
        {
            // Grab input data which always returned as an ArrayList
            var inputs = data as ArrayList;

            var minValueX = double.TryParse(inputs[0]?.ToString(), out var minX) ? minX : MinLimitX;
            var maxValueX = double.TryParse(inputs[1]?.ToString(), out var maxX) ? maxX : MaxLimitX;
            var minValueY = double.TryParse(inputs[2]?.ToString(), out var minY) ? minY : MinLimitY;
            var maxValueY = double.TryParse(inputs[3]?.ToString(), out var maxY) ? maxY : MaxLimitY;
            //var listValue = int.Parse(inputs[4].ToString());

            if (!InPorts[0].IsConnected && !InPorts[1].IsConnected &&
                !InPorts[2].IsConnected && !InPorts[3].IsConnected)
            {
                return;
            }

            // Check port connectivity
            if (InPorts[0].IsConnected) MinLimitX = minValueX;
            if (InPorts[1].IsConnected) MaxLimitX = maxValueX;
            if (InPorts[2].IsConnected) MinLimitY = minValueY;
            if (InPorts[3].IsConnected) MaxLimitY = maxValueY;

            // Notify property changes to update UI
            RaisePropertyChanged(nameof(MinLimitX));
            RaisePropertyChanged(nameof(MaxLimitX));
            RaisePropertyChanged(nameof(MinLimitY));
            RaisePropertyChanged(nameof(MaxLimitY));


            // Trigger additional UI updates if necessary
            RaisePropertyChanged("DataUpdated");
        }

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected || !InPorts[1].IsConnected)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()),
                };
            }

            var minLimitX = inputAstNodes[0]; // x-MinLimit
            var maxLimitX = inputAstNodes[1]; // x-MaxLimit
            var minLimitY = inputAstNodes[2]; // y-MinLimit
            var maxLimitY = inputAstNodes[3]; // y-MaxLimit

            // Function call for the computational logic of CurveMapper
            var functionCall = AstFactory.BuildFunctionCall(
                new Func<double, double, double, double, List<double>>(CurveMapperFunctions.GenerateCurve),
                new List<AssociativeNode>
                {
                    minLimitX,
                    maxLimitX,
                    minLimitY,
                    maxLimitY
                }
            );

            // Add the DataBridge call to trigger the callback
            var dataBridgeCall = AstFactory.BuildAssignment(
                AstFactory.BuildIdentifier(AstIdentifierBase + "_dataBridge"),
                VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes))
            );

            // Return both the function call and the DataBridge call
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall),
                dataBridgeCall
            };
        }

        #endregion

        #region Events

        private void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnNodeModified(); // This will ensure the node is re-executed
        }

        #endregion
    }

    public class CurveMapperNodeView : INodeViewCustomization<CurveMapperNodeModel>
    {
        private CurveMapperControl curveMapperControl;
        private NodeView view;
        private CurveMapperNodeModel model;

        /// <summary>
        /// At run-time, this method is called during the node 
        /// creation. Add custom UI element to the node view.
        /// </summary>
        /// <param name="model">The NodeModel representing the node's core logic.</param>
        /// <param name="nodeView">The NodeView representing the node in the graph.</param>
        public void CustomizeView(CurveMapperNodeModel model, NodeView nodeView)
        {
            this.model = model;
            this.view = nodeView;

            curveMapperControl = new CurveMapperControl(model);
            nodeView.inputGrid.Children.Add(curveMapperControl);

            //// Call this method to set the initial state of ports
            //UpdatePortDefaultValueMarkers();
            // Subscribe to port update events
            model.PortUpdated += (s, e) => UpdatePortDefaultValueMarkers();
            // Subscribe to port connection events
            foreach (var port in model.InPorts)
            {
                port.Connectors.CollectionChanged += (s, e) => UpdatePortDefaultValueMarkers();
            }

            model.PortUpdated += ModelOnPortUpdated;
        }

        // I think this runs for all ports... can it run only for the port that has changed?
        private void UpdatePortDefaultValueMarkers()
        {
            if (view == null || view.ViewModel == null || view.ViewModel.InPorts.Count == 0)
                return;

            var inPorts = view.ViewModel.InPorts;

            // If the inPort is really connected, turn off the UsingDefaultValue indicator
            // For some reason if inPort is registered with default value IsConnected returns true
            // even if the inPort is not really connected. See Range node for example
            if (model == null || model.InPorts.Count == 0) return;

            for (int i = 0; i < model.InPorts.Count; i++)
            {
                var c1 = model.InPorts[i].Connectors.Any();
                if (model.InPorts[i].Connectors.Any())
                {
                    inPorts[i].UsingDefaultValue = false;
                }
            }

        }

        private void ModelOnPortUpdated(object sender, EventArgs e)
        {
            UpdateDefaultInPortValues();
        }

        // Needed only if the inPorts have default value??
        // Does this work correctly? 
        private void UpdateDefaultInPortValues()
        {
            if (!this.view.ViewModel.InPorts.Any()) return;
            var inPorts = this.view.ViewModel.InPorts;

            // Only apply default values if all ports are disconnected
            if (!model.IsInErrorState &&
                    model.State != ElementState.Active &&
                    !inPorts[0].IsConnected &&
                    !inPorts[1].IsConnected &&
                    !inPorts[2].IsConnected &&
                    !inPorts[3].IsConnected &&
                    !inPorts[4].IsConnected)
            {
                ((InPortViewModel)inPorts[0]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[1]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[2]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[3]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[5]).PortDefaultValueMarkerVisible = true;
            }
            else
            {
                ((InPortViewModel)inPorts[0]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[1]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[2]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[3]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[4]).PortDefaultValueMarkerVisible = false;
            }

            var allPortsConnected = inPorts[0].IsConnected && inPorts[1].IsConnected && inPorts[2].IsConnected && inPorts[3].IsConnected && model.State != ElementState.Warning;
            var noPortsConnected = !inPorts[0].IsConnected && !inPorts[1].IsConnected && !inPorts[2].IsConnected && !inPorts[3].IsConnected;

            // The color input uses default values if it's not connected
            if (!inPorts[4].IsConnected && (allPortsConnected || noPortsConnected))
            {
                ((InPortViewModel)inPorts[4]).PortDefaultValueMarkerVisible = true;
            }
            else
            {
                ((InPortViewModel)inPorts[4]).PortDefaultValueMarkerVisible = false;
            }
        }

        public void Dispose()
        {
            model.PortUpdated -= ModelOnPortUpdated;
        }
    }

    #region GraphTypes

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum GraphTypes
    {
        [Description("Select type")]
        Empty = 0,
        [Description("Linear Curve")]
        Linear = 1,
        [Description("Bezier Curve")]
        Bezier = 2,
        [Description("Sine Wave")]
        SineWave = 3,
        [Description("Cosine Wave")]
        CosineWave = 4,
        [Description("Tangent Wave")]
        TangentWave = 5,
        [Description("Gaussian Wave")]
        GaussianWave = 6,
        [Description("Parabolic Curve")]
        Parabola = 7,
        [Description("Perlin Noise")]
        PerlinNoise = 8
    }

    #endregion
}
