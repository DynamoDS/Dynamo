using Autodesk.DesignScript.Runtime;
using CoreNodeModelsWpf.Charts.Controls;
using CoreNodeModelsWpf.Converters;
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
    [NodeName("Curve Mapper")]
    [NodeCategory("Math.Graph.Create")]
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

        [JsonIgnore]
        internal EngineController EngineController { get; set; }

        #region Input Properties
        private double minLimitX;
        private double maxLimitX = 1;
        private double minLimitY;
        private double maxLimitY = 1;
        private int pointsCount = 10;
        private List<double> outputValuesY;
        private List<double> outputValuesX;
        private readonly IntNode minLimitXDefaultValue = new IntNode(0);
        private readonly IntNode maxLimitXDefaultValue = new IntNode(1);
        private readonly IntNode minLimitYDefaultValue = new IntNode(0);
        private readonly IntNode maxLimitYDefaultValue = new IntNode(1);
        private readonly IntNode pointsCountDefaultValue = new IntNode(10);
        private GraphTypes selectedGraphType;
        
        [JsonIgnore]  // Should those properties be serialized ???
        public double MinLimitX
        {
            get => minLimitX;
            set
            {
                if (minLimitX != value)
                {
                    minLimitX = value;
                    GenerateOutputValues();
                    this.RaisePropertyChanged(nameof(MinLimitX));
                    this.RaisePropertyChanged(nameof(MidValueX));
                    OnNodeModified();
                }                
            }
        }
        [JsonIgnore]
        public double MaxLimitX
        {
            get => maxLimitX;
            set
            {
                if (maxLimitX != value)
                {
                    maxLimitX = value;
                    GenerateOutputValues();
                    this.RaisePropertyChanged(nameof(MaxLimitX));
                    this.RaisePropertyChanged(nameof(MidValueX));
                    OnNodeModified();
                }
            }
        }
        [JsonIgnore]
        public double MinLimitY
        {
            get => minLimitY;
            set
            {
                if (minLimitY != value)
                {
                    minLimitY = value;
                    GenerateOutputValues();
                    this.RaisePropertyChanged(nameof(MinLimitY));
                    this.RaisePropertyChanged(nameof(MidValueY));
                    OnNodeModified();
                }                
            }
        }
        [JsonIgnore]
        public double MaxLimitY
        {
            get => maxLimitY;
            set
            {
                if (maxLimitY != value)
                {
                    maxLimitY = value;
                    GenerateOutputValues();
                    this.RaisePropertyChanged(nameof(MaxLimitY));
                    this.RaisePropertyChanged(nameof(MidValueY));
                    OnNodeModified();
                }                
            }
        }
        [JsonIgnore]
        public double MidValueX => (MaxLimitX + MinLimitX) * 0.5;
        [JsonIgnore]
        public double MidValueY => (MaxLimitY + MinLimitY) * 0.5;
        [JsonIgnore]
        public int PointsCount
        {
            get => pointsCount;
            set
            {
                if (pointsCount != value)
                {
                    pointsCount = value;
                    GenerateOutputValues();
                    this.RaisePropertyChanged(nameof(PointsCount));
                    OnNodeModified();
                }
            }
        }
        
        [JsonIgnore] // [JsonConverter(typeof(StringEnumConverter))]
        public GraphTypes SelectedGraphType
        {
            get => selectedGraphType;
            set
            {
                selectedGraphType = value;
                GenerateOutputValues();
                RaisePropertyChanged(nameof(SelectedGraphType));
                OnNodeModified();
            }
        }
        #endregion

        #region Output properties
        [JsonIgnore]
        public List<double> OutputValuesY
        {
            get => outputValuesY;
            set
            {
                outputValuesY = value;
                OnNodeModified();
            }
        }
        [JsonIgnore]
        public List<double> OutputValuesX
        {
            get => outputValuesX;
            set
            {
                outputValuesX = value;
                OnNodeModified();
            }
        }
        #endregion

        #region Linear Curve
        [JsonIgnore]
        public CurveMapperControlPoint PointLinearStart { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint PointLinearEnd { get; set; }
        [JsonIgnore]
        public LinearCurve LinearCurve { get; set; }
        [JsonIgnore]
        public CurveMapperControl CurveMapperControl { get; set; }
        #endregion

        #region Bezier Curve
        // represent fixed control points of a Bezier curve
        // likely the non-draggable control points of the curve that define ends or anchors
        [JsonIgnore]
        public CurveMapperControlPoint BezierControlPoint1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint BezierControlPoint2 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint BezierFixedPoint1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint BezierFixedPoint2 { get; set; }
        [JsonIgnore]
        public ControlLine CurveBezierControlLine1 { get; set; }
        [JsonIgnore]
        public ControlLine CurveBezierControlLine2 { get; set; }
        [JsonIgnore]
        public BezierCurve BezierCurve {  get; set; }
        #endregion

        #region Sine Curve
        private CurveMapperControlPoint controlPointSine1;
        private CurveMapperControlPoint controlPointSine2;

        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointSine1
        {
            get => controlPointSine1;
            set
            {
                controlPointSine1 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointSine2
        {
            get => controlPointSine2;
            set
            {
                controlPointSine2 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore]
        public SineCurve SineCurve { get; set; }
        #endregion

        #region Cosine Curve
        private CurveMapperControlPoint controlPointCosine1;
        private CurveMapperControlPoint controlPointCosine2;

        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointCosine1
        {
            get => controlPointCosine1;
            set
            {
                controlPointCosine1 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointCosine2
        {
            get => controlPointCosine2;
            set
            {
                controlPointCosine2 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore]
        public SineCurve CosineCurve { get; set; }
        #endregion

        #region Tangent Curve
        private CurveMapperControlPoint controlPointTangent1;
        private CurveMapperControlPoint controlPointTangent2;

        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointTangent1
        {
            get => controlPointTangent1;
            set
            {
                controlPointTangent1 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointTangent2
        {
            get => controlPointTangent2;
            set
            {
                controlPointTangent2 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore]
        public TangentCurve TangentCurve { get; set; }
        #endregion

        #region Parabolic Curve
        private CurveMapperControlPoint controlPointParabolic1;
        private CurveMapperControlPoint controlPointParabolic2;

        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointParabolic1
        {
            get => controlPointParabolic1;
            set
            {
                controlPointParabolic1 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointParabolic2
        {
            get => controlPointParabolic2;
            set
            {
                controlPointParabolic2 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore]
        public ParabolicCurve ParabolicCurve { get; set; }
        #endregion

        #region Perlin Curve
        private CurveMapperControlPoint fixedPointPerlin1;
        private CurveMapperControlPoint fixedPointPerlin2;
        private CurveMapperControlPoint controlPointPerlin;

        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint FixedPointPerlin1
        {
            get => fixedPointPerlin1;
            set
            {
                fixedPointPerlin1 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint FixedPointPerlin2
        {
            get => fixedPointPerlin2;
            set
            {
                fixedPointPerlin2 = value;
                OnNodeModified();
            }
        }
        [JsonIgnore] //[JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointPerlin
        {
            get => controlPointPerlin;
            set
            {
                controlPointPerlin = value;
                OnNodeModified();
            }
        }
        [JsonIgnore]
        public PerlinCurve PerlinCurve { get; set; }
        #endregion

        /// <summary>
        /// Triggers when port is connected or disconnected
        /// </summary>
        public event EventHandler PortUpdated;

        protected virtual void OnPortUpdated(EventArgs args)
        {
            PortUpdated?.Invoke(this, args);
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
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("y-Values",
                CoreNodeModelWpfResources.CurveMapperOutputDataPortToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("x-Values",
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




        internal void GenerateOutputValues()
        {           
            switch (SelectedGraphType)
            {
                case GraphTypes.Empty:
                    if (LinearCurve != null)
                    {
                        OutputValuesY = new List<double> { 5, 9, 1984};
                        OutputValuesY = new List<double> { 4, 8, 1984 };
                    }
                    break;


                case GraphTypes.Linear:
                    if (LinearCurve != null)
                    {
                        OutputValuesY = LinearCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                        OutputValuesX = LinearCurve.GetCurveYValues(minLimitX, maxLimitX, pointsCount);
                    }
                    break;
                case GraphTypes.SineWave:
                    if (SineCurve != null)
                    {
                        OutputValuesY = SineCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                        OutputValuesX = SineCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
                    }
                    break;
            }
        }












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
            var listValue = int.TryParse(inputs[4]?.ToString(), out var parsedCount) ? parsedCount : PointsCount;

            if (!InPorts[0].IsConnected && !InPorts[1].IsConnected && // do we need this id we are using default values?
                !InPorts[2].IsConnected && !InPorts[3].IsConnected)
            {
                return;
            }

            // Check port connectivity
            if (InPorts[0].IsConnected) MinLimitX = minValueX;
            if (InPorts[1].IsConnected) MaxLimitX = maxValueX;
            if (InPorts[2].IsConnected) MinLimitY = minValueY;
            if (InPorts[3].IsConnected) MaxLimitY = maxValueY;
            if (InPorts[4].IsConnected) PointsCount = listValue;

            // Notify property changes to update UI
            RaisePropertyChanged(nameof(MinLimitX));
            RaisePropertyChanged(nameof(MaxLimitX));
            RaisePropertyChanged(nameof(MinLimitY));
            RaisePropertyChanged(nameof(MaxLimitY));
            RaisePropertyChanged(nameof(PointsCount));


            // Trigger additional UI updates if necessary
            RaisePropertyChanged("DataUpdated");
        }

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //if (!InPorts[0].IsConnected || !InPorts[1].IsConnected ||
            //    !InPorts[2].IsConnected || !InPorts[3].IsConnected ||
            //    !InPorts[4].IsConnected)
            //{
            //    return new[]
            //    {
            //        AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()),
            //        //AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), AstFactory.BuildNullNode())
            //    };
            //}





            // Assign to output ports
            var xValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode());
            var yValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), AstFactory.BuildNullNode());
            
            if (OutputValuesY != null)
            {
                var doubListY = new List<AssociativeNode>();
                foreach (double dVal in OutputValuesY)
                {
                    doubListY.Add(AstFactory.BuildDoubleNode(dVal));
                }
                yValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildExprList(doubListY));

                var doubListX = new List<AssociativeNode>();
                foreach (double dVal in OutputValuesY)
                {
                    doubListX.Add(AstFactory.BuildDoubleNode(dVal));
                }
                xValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), AstFactory.BuildExprList(doubListY));
            }         






            // DataBridge call
            var dataBridgeCall = AstFactory.BuildAssignment(
                AstFactory.BuildIdentifier(AstIdentifierBase + "_dataBridge"),
                VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes))
            );

            return new[]
            {
                yValuesAssignment,
                xValuesAssignment,
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
