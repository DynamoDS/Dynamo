using Autodesk.DesignScript.Runtime;
using CoreNodeModelsWpf.Charts.Controls;
using CoreNodeModelsWpf.Converters;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Controls.SubControls;
using Dynamo.Wpf.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace CoreNodeModelsWpf.Charts
{
    [IsDesignScriptCompatible]
    [NodeName("Curve Mapper")]
    [NodeCategory("Math.Graph.Create")]
    [NodeDescription("CurveMapperNodeDescription", typeof(CoreNodeModelWpfResources))]
    [NodeSearchTags("CurveMapperSearchTags", typeof(CoreNodeModelWpfResources))]
    public class CurveMapperNodeModel : NodeModel
    {
        [JsonIgnore]
        public CurveMapperControl CurveMapperControl { get; set; }
        public EngineController EngineController { get; set; }


        #region Input | Output

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

        // TODO: Should those properties be serialized?
        [JsonIgnore]  
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
        
        [JsonConverter(typeof(StringEnumConverter))]
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

        #region Curves

        // Linear curve
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointLinear1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointLinear2 { get; set; }
        [JsonIgnore]
        public LinearCurve LinearCurve { get; set; }

        // Bezier curve
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointBezier1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointBezier2 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint OrthoControlPointBezier1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint OrthoControlPointBezier2 { get; set; }
        [JsonIgnore]
        public ControlLine ControlLineBezier1 { get; set; }
        [JsonIgnore]
        public ControlLine ControlLineBezier2 { get; set; }
        [JsonIgnore]
        public BezierCurve BezierCurve {  get; set; }

        // Sine wave
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointSine1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointSine2 { get; set; }
        [JsonIgnore]
        public SineCurve SineWave { get; set; }

        //Cosine wave
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointCosine1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointCosine2 { get; set; }
        [JsonIgnore]
        public SineCurve CosineWave { get; set; }

        // Parabolic curve
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointParabolic1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointParabolic2 { get; set; }
        [JsonIgnore]
        public ParabolicCurve ParabolicCurve { get; set; }

        // Perlin noise
        [JsonIgnore]
        public CurveMapperControlPoint OrthoControlPointPerlin1 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint OrthoControlPointPerlin2 { get; set; }
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointPerlin { get; set; }
        [JsonIgnore]
        public PerlinCurve PerlinNoiseCurve { get; set; }

        // Power noise
        [JsonIgnore]
        public CurveMapperControlPoint ControlPointPower { get; set; }
        [JsonIgnore]
        public PowerCurve PowerCurve { get; set; }

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
            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }

        #endregion

        internal void GenerateOutputValues()
        {
            if (CurveMapperControl == null)
                return;

            // Ensure at least 2 points and a non-vertical curve; otherwise, display a warning.
            if (PointsCount < 2 ||
                (SelectedGraphType == GraphTypes.LinearCurve && ControlPointLinear1.Point.X == ControlPointLinear2.Point.X) ||
                (SelectedGraphType == GraphTypes.CosineWave && ControlPointCosine1.Point.X == ControlPointCosine2.Point.X) ||
                (SelectedGraphType == GraphTypes.SineWave && ControlPointSine1.Point.X == ControlPointSine2.Point.X) ||
                (SelectedGraphType == GraphTypes.ParabolicCurve && ControlPointParabolic1.Point.X == ControlPointParabolic2.Point.X) ||
                (SelectedGraphType == GraphTypes.PowerCurve && (ControlPointPower.Point.X == MinLimitX || ControlPointPower.Point.Y == MinLimitY))
            )
            {
                ClearErrorsAndWarnings();
                Warning(CoreNodeModelWpfResources.CurveMapperInputWarning, isPersistent: true);
            }
            else
            {
                ClearErrorsAndWarnings();
            }

            if (LinearCurve != null && SelectedGraphType == GraphTypes.LinearCurve)
            {
                OutputValuesY = LinearCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                OutputValuesX = LinearCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
            }
            else if (BezierCurve != null && SelectedGraphType == GraphTypes.BezierCurve)
            {
                OutputValuesY = BezierCurve.GetBezierCurveYValues(minLimitY, maxLimitY,
                    pointsCount, CurveMapperControl.DynamicCanvasSize);
                OutputValuesX = BezierCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
            }
            else if (SineWave != null && SelectedGraphType == GraphTypes.SineWave)
            {
                OutputValuesY = SineWave.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                OutputValuesX = SineWave.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
            }
            else if (CosineWave != null && SelectedGraphType == GraphTypes.CosineWave)
            {
                OutputValuesY = CosineWave.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                OutputValuesX = CosineWave.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
            }
            else if (ParabolicCurve != null && SelectedGraphType == GraphTypes.ParabolicCurve)
            {
                OutputValuesY = ParabolicCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                OutputValuesX = ParabolicCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
            }
            else if (PerlinNoiseCurve != null && SelectedGraphType == GraphTypes.PerlinNoiseCurve)
            {
                OutputValuesY = PerlinNoiseCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                OutputValuesX = PerlinNoiseCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
            }
            else if (PowerCurve != null && SelectedGraphType == GraphTypes.PowerCurve)
            {
                OutputValuesY = PowerCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                OutputValuesX = PowerCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
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
            // Ignore invalid inputs
            if (!(data is ArrayList inputs) || inputs.Count < 5)
                return;

            // Grab input data which always returned as an ArrayList
            inputs = data as ArrayList;

            var minValueX = double.TryParse(inputs[0]?.ToString(), out var minX) ? minX : MinLimitX;
            var maxValueX = double.TryParse(inputs[1]?.ToString(), out var maxX) ? maxX : MaxLimitX;
            var minValueY = double.TryParse(inputs[2]?.ToString(), out var minY) ? minY : MinLimitY;
            var maxValueY = double.TryParse(inputs[3]?.ToString(), out var maxY) ? maxY : MaxLimitY;
            var listValue = int.TryParse(inputs[4]?.ToString(), out var parsedCount) ? parsedCount : PointsCount;

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
            //// This requires the input nodes to be executed.
            //// If the value of input node changes, the graph will still need to execute twice,
            //// once for the new value to be registered and the second time for the value to be passes to
            //// the property
            //if (InPorts[0].IsConnected) MinLimitX = GetInputValueOrDefault(0, minLimitX);
            //if (InPorts[1].IsConnected) MaxLimitX = GetInputValueOrDefault(1, maxLimitX);
            //if (InPorts[2].IsConnected) MinLimitY = GetInputValueOrDefault(2, minLimitY);
            //if (InPorts[3].IsConnected) MaxLimitY = GetInputValueOrDefault(3, maxLimitY);
            //if (InPorts[4].IsConnected) PointsCount = GetInputValueOrDefault(4, pointsCount);

            // Assign to output ports
            var xValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode());
            var yValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), AstFactory.BuildNullNode());
            
            if (OutputValuesY != null)
            {
                var doubListY = new List<AssociativeNode>();
                if (OutputValuesY != null)
                {
                    foreach (double yVal in OutputValuesY)
                    {
                        doubListY.Add(AstFactory.BuildDoubleNode(yVal));
                    }
                    yValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildExprList(doubListY));
                }                

                var doubListX = new List<AssociativeNode>();
                if (OutputValuesX != null)
                {
                    foreach (double xVal in OutputValuesX)
                    {
                        doubListX.Add(AstFactory.BuildDoubleNode(xVal));
                    }
                    xValuesAssignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), AstFactory.BuildExprList(doubListX));
                }                
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
            OnNodeModified();
        }

        #endregion


        private double GetInputValueOrDefault(int portIndex, double defaultValue)
        {
            // Ensure the port index is valid
            if (portIndex < 0 || portIndex >= InPorts.Count || !InPorts[portIndex].IsConnected)
                return defaultValue;

            var connector = InPorts[portIndex].Connectors.FirstOrDefault();
            if (connector == null) return defaultValue;

            var inputNode = connector.Start.Owner as NodeModel;
            if (inputNode == null) return defaultValue;

            int inputNodeIndex = connector.Start.Index;

            // Ensure EngineController is not null
            if (this.EngineController == null) return defaultValue;

            var inputNodeId = inputNode.GetAstIdentifierForOutputIndex(inputNodeIndex).Name;

            var inputNodeMirror = this.EngineController.GetMirror(inputNodeId);
            if (inputNodeMirror == null || inputNodeMirror.GetData() == null)
                return defaultValue;

            object inputNodeObject;

            if (inputNodeMirror.GetData().IsCollection)
                inputNodeObject = inputNodeMirror.GetData().GetElements().Select(x => x.Data).FirstOrDefault();
            else
                inputNodeObject = inputNodeMirror.GetData().Data;

            if (inputNodeObject == null) return defaultValue;

            if (double.TryParse(inputNodeObject.ToString(), System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out double parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }
        private int GetInputValueOrDefault(int portIndex, int defaultValue)
        {
            // Ensure the port index is valid and connected
            if (portIndex < 0 || portIndex >= InPorts.Count || !InPorts[portIndex].IsConnected)
                return defaultValue;

            var connector = InPorts[portIndex].Connectors.FirstOrDefault();
            if (connector == null) return defaultValue;

            var inputNode = connector.Start.Owner as NodeModel;
            if (inputNode == null) return defaultValue;

            int inputNodeIndex = connector.Start.Index;

            // Ensure EngineController is not null
            if (this.EngineController == null) return defaultValue;

            var inputNodeId = inputNode.GetAstIdentifierForOutputIndex(inputNodeIndex).Name;

            var inputNodeMirror = this.EngineController.GetMirror(inputNodeId);
            if (inputNodeMirror == null || inputNodeMirror.GetData() == null)
                return defaultValue;

            object inputNodeObject;

            if (inputNodeMirror.GetData().IsCollection)
                inputNodeObject = inputNodeMirror.GetData().GetElements().Select(x => x.Data).FirstOrDefault();
            else
                inputNodeObject = inputNodeMirror.GetData().Data;

            if (inputNodeObject == null) return defaultValue;

            if (int.TryParse(inputNodeObject.ToString(), System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out int parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }


    }

    #region GraphTypes

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum GraphTypes
    {
        [Description("Select type")]
        Empty = 0,
        [Description("Linear Curve")]
        LinearCurve = 1,
        [Description("Bezier Curve")]
        BezierCurve = 2,
        [Description("Sine Wave")]
        SineWave = 3,
        [Description("Cosine Wave")]
        CosineWave = 4,
        [Description("Parabolic Curve")]
        ParabolicCurve = 5,
        [Description("Perlin Noise")]
        PerlinNoiseCurve = 6,
        [Description("Power Curve")]
        PowerCurve = 7
    }

    #endregion
}
