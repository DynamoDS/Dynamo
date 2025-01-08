using Autodesk.DesignScript.Runtime;
using CoreNodeModelsWpf.Charts.Controls;
using CoreNodeModelsWpf.Converters;
using CoreNodes.ChartHelpers;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Controls.SubControls;
using Dynamo.Wpf.Properties;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoreNodeModelsWpf.Charts
{
    [IsDesignScriptCompatible]
    [NodeName("CurveMapper")]
    [NodeCategory("Math.Graph")]
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
        #region Input Properties
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

        [JsonIgnore]
        internal EngineController EngineController { get; set; }

        // Should those properties be serialized ???
        //[JsonProperty(PropertyName = "MinLimitX")]
        public double MinLimitX
        {
            get => minLimitX;
            set
            {
                if (minLimitX != value)
                {
                    minLimitX = value;
                    this.RaisePropertyChanged(nameof(MinLimitX));
                    this.RaisePropertyChanged(nameof(MidValueX));
                    OnNodeModified();
                }                
            }
        }
        //[JsonProperty(PropertyName = "MaxLimitX")]
        public double MaxLimitX
        {
            get => maxLimitX;
            set
            {
                if (maxLimitX != value)
                {
                    maxLimitX = value;
                    this.RaisePropertyChanged(nameof(MaxLimitX));
                    this.RaisePropertyChanged(nameof(MidValueX));
                    OnNodeModified();
                }
            }
        }
        //[JsonProperty(PropertyName = "MinLimitY")]
        public double MinLimitY
        {
            get => minLimitY;
            set
            {
                if (minLimitY != value)
                {
                    minLimitY = value;
                    this.RaisePropertyChanged(nameof(MinLimitY));
                    this.RaisePropertyChanged(nameof(MidValueY));
                    OnNodeModified();
                }                
            }
        }
        //[JsonProperty(PropertyName = "MaxLimitY")]
        public double MaxLimitY
        {
            get => maxLimitY;
            set
            {
                if (maxLimitY != value)
                {
                    maxLimitY = value;
                    this.RaisePropertyChanged(nameof(MaxLimitY));
                    this.RaisePropertyChanged(nameof(MidValueY));
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

        /// <summary>
        /// Triggers when port is connected or disconnected
        /// </summary>
        public event EventHandler PortUpdated;

        protected virtual void OnPortUpdated(EventArgs args)
        {
            PortUpdated?.Invoke(this, args);
        }

        #region Linear Points

        //[JsonConverter(typeof(StringToPointThumbConverter))]
        [JsonIgnore]
        public CurveMapperControlPoint PointLinearStart { get; set; }
        //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint PointLinearEnd { get; set; }
        [JsonIgnore]
        public CurveLinear LinearCurve { get; set; }
        public CurveMapperControl CurveMapperControl { get; set; }

        #endregion

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

            // Initialize the CurveMapperControl and other elements -> now created in CustomizeView
            //CurveMapperControl = new CurveMapperControl(this);
            //PointLinearStart = new CurveMapperControlPoint(new System.Windows.Point(10, 230), 240, 240);
            //PointLinearEnd = new CurveMapperControlPoint(new System.Windows.Point(230, 10), 240, 240);
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
