using Autodesk.DesignScript.Runtime;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Dynamo.Core;
using CoreNodeModels.CurveMapper;
using Lucene.Net.QueryParsers.Surround.Query;
using CoreNodeModels.HigherOrder;

namespace CoreNodeModels
{
    [IsDesignScriptCompatible]
    [NodeName("Curve Mapper")]
    [NodeCategory("Math.Graph.Create")]
    [NodeDescription("CurveMapperNodeDescription")]
    [NodeSearchTags("CurveMapperSearchTags")]
    public class CurveMapperNodeModel : NodeModel
    {
        private double minLimitX;
        private double maxLimitX = 1;
        private double minLimitY;
        private double maxLimitY = 1;
        private int pointsCount = 10;
        private List<double> outputValuesY;
        private List<double> outputValuesX;
        private List<double> renderValuesY;
        private List<double> renderValuesX;
        private readonly IntNode minLimitXDefaultValue = new IntNode(0);
        private readonly IntNode maxLimitXDefaultValue = new IntNode(1);
        private readonly IntNode minLimitYDefaultValue = new IntNode(0);
        private readonly IntNode maxLimitYDefaultValue = new IntNode(1);
        private readonly IntNode pointsCountDefaultValue = new IntNode(10);
        private GraphTypes selectedGraphType;
        private const double defaultCanvasSize = 240;
        private const double defaultMinGridWidth = 310;
        private const double defaultMinGridHeight = 340;
        private double dynamicCanvasSize = defaultCanvasSize;
        private bool isLocked;

        #region Curves & point data

        // Linear curve
        public ControlPointData LinearCurveControlPointData1 { get; private set; }
        public ControlPointData LinearCurveControlPointData2 { get; private set; }
        private LinearCurve linearCurve;
        // Sine wave
        public ControlPointData SineWaveControlPointData1 { get; private set; }
        public ControlPointData SineWaveControlPointData2 { get; private set; }
        private SineWave sineWave;
        // Cosine wave
        public ControlPointData CosineWaveControlPointData1 { get; private set; }
        public ControlPointData CosineWaveControlPointData2 { get; private set; }
        private SineWave cosineWave;

        #endregion

        #region Inputs

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
        [JsonIgnore]
        public double MidValueX => (MaxLimitX + MinLimitX) * 0.5;
        [JsonIgnore]
        public double MidValueY => (MaxLimitY + MinLimitY) * 0.5;

        #endregion

        #region Outputs

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
        [JsonIgnore]
        public List<double> RenderValuesY
        {
            get => renderValuesY;
            set
            {
                renderValuesY = value;
                OnNodeModified();
            }
        }
        [JsonIgnore]
        public List<double> RenderValuesX
        {
            get => renderValuesX;
            set
            {
                renderValuesX = value;
                OnNodeModified();
            }
        }

        #endregion

        [JsonProperty]
        public double DynamicCanvasSize
        {
            get => dynamicCanvasSize;
            set
            {
                if (dynamicCanvasSize != value)
                {
                    double oldSize = dynamicCanvasSize;
                    dynamicCanvasSize = Math.Max(value, defaultCanvasSize);

                    // Scale control points when resizing the canvas
                    LinearCurveControlPointData1.ScaleToNewCanvasSize(oldSize, dynamicCanvasSize);
                    LinearCurveControlPointData2.ScaleToNewCanvasSize(oldSize, dynamicCanvasSize);
                    SineWaveControlPointData1.ScaleToNewCanvasSize(oldSize, dynamicCanvasSize);
                    SineWaveControlPointData2.ScaleToNewCanvasSize(oldSize, dynamicCanvasSize);
                    CosineWaveControlPointData1.ScaleToNewCanvasSize(oldSize, dynamicCanvasSize);
                    CosineWaveControlPointData2.ScaleToNewCanvasSize(oldSize, dynamicCanvasSize);

                    RaisePropertyChanged(nameof(DynamicCanvasSize));
                    OnNodeModified();
                    GenerateOutputValues();

                }
            }
        }

        [JsonIgnore]
        public List<string> GraphTypesList => Enum.GetValues(typeof(GraphTypes))
            .Cast<GraphTypes>()
            .Select(value => GetEnumDescription(value))
            .ToList();

        [JsonIgnore]
        public string SelectedGraphTypeDescription
        {
            get => GetEnumDescription(SelectedGraphType);
            set
            {
                SelectedGraphType = Enum.GetValues(typeof(GraphTypes))
                    .Cast<GraphTypes>()
                    .FirstOrDefault(e => GetEnumDescription(e) == value);

                RaisePropertyChanged(nameof(SelectedGraphType));
                RaisePropertyChanged(nameof(SelectedGraphTypeDescription));
                GenerateOutputValues();
                OnNodeModified();
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

        /// <summary>
        /// Gets the minimum allowed width for the grid in the view.
        /// </summary>
        public double MinGridWidth => defaultMinGridWidth;

        /// <summary>
        /// Gets the minimum allowed height for the grid in the view.
        /// </summary>
        public double MinGridHeight => defaultMinGridHeight;

        /// <summary>
        /// Gets the minimum allowed width/height for the canvas in the view.
        /// </summary>
        public double MinCanvasSize => defaultCanvasSize;

        [JsonIgnore]
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (isLocked != value)
                {
                    isLocked = value;
                    RaisePropertyChanged(nameof(IsLocked));
                }
            }
        }

        public CurveMapperNodeModel()
        {
            if (InPorts.Count == 0)
            {
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("x-MinLimit",
                    "add ToolTip",
                    minLimitXDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("x-MaxLimit",
                    "add ToolTip",
                    maxLimitXDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("y-MinLimit",
                    "add ToolTip",
                    minLimitYDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("y-MaxLimit",
                    "add ToolTip",
                    maxLimitYDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("count",
                    "add ToolTip",
                    pointsCountDefaultValue)));
            }
            if (OutPorts.Count == 0)
            {
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData("y-Values", "add ToolTip for y")));
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData("x-Values", "add ToolTip for x")));
            }

            RegisterAllPorts();

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            SelectedGraphType = GraphTypes.Empty;
            ArgumentLacing = LacingStrategy.Disabled;

            // Create control points
            LinearCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.1, DynamicCanvasSize * 0.9);
            LinearCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.9, DynamicCanvasSize * 0.1);
            SineWaveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.25, 0);
            SineWaveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.75, DynamicCanvasSize);
            CosineWaveControlPointData1 = new ControlPointData(0, 0);
            CosineWaveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize);

            GenerateOutputValues();
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

        private void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnNodeModified();
        }        

        public void GenerateOutputValues()
        {
            if (SelectedGraphType == GraphTypes.Empty)
            {
                OutputValuesX = null;
                OutputValuesY = null;
            }
            else if (SelectedGraphType == GraphTypes.LinearCurve)
            {
                linearCurve = new LinearCurve(
                    LinearCurveControlPointData1.X, (DynamicCanvasSize - LinearCurveControlPointData1.Y),
                    LinearCurveControlPointData2.X, (DynamicCanvasSize - LinearCurveControlPointData2.Y),
                    DynamicCanvasSize
                );

                RenderValuesX = linearCurve.GetCurveXValues(PointsCount, true);
                RenderValuesY = linearCurve.GetCurveYValues(PointsCount, true);
                OutputValuesX = MapValues(linearCurve.GetCurveXValues(PointsCount), MinLimitX, MaxLimitX);
                OutputValuesY = MapValues(linearCurve.GetCurveYValues(PointsCount), MinLimitY, MaxLimitY);
            }
            else if (SelectedGraphType == GraphTypes.SineWave)
            {
                sineWave = new SineWave(
                    SineWaveControlPointData1.X, (DynamicCanvasSize - SineWaveControlPointData1.Y),
                    SineWaveControlPointData2.X, (DynamicCanvasSize - SineWaveControlPointData2.Y),
                    DynamicCanvasSize
                );

                RenderValuesX = sineWave.GetCurveXValues(PointsCount, true);
                RenderValuesY = sineWave.GetCurveYValues(PointsCount, true);
                OutputValuesX = MapValues(sineWave.GetCurveXValues(PointsCount), MinLimitX, MaxLimitX); // Review
                OutputValuesY = MapValues(sineWave.GetCurveYValues(PointsCount), MinLimitY, MaxLimitY); // Review
            }
            else if (SelectedGraphType == GraphTypes.CosineWave)
            {
                cosineWave = new SineWave(
                    CosineWaveControlPointData1.X, (DynamicCanvasSize - CosineWaveControlPointData1.Y),
                    CosineWaveControlPointData2.X, (DynamicCanvasSize - CosineWaveControlPointData2.Y),
                    DynamicCanvasSize
                );

                RenderValuesX = cosineWave.GetCurveXValues(PointsCount, true);
                RenderValuesY = cosineWave.GetCurveYValues(PointsCount, true);
                OutputValuesX = MapValues(cosineWave.GetCurveXValues(PointsCount), MinLimitX, MaxLimitX); // Review
                OutputValuesY = MapValues(cosineWave.GetCurveYValues(PointsCount), MinLimitY, MaxLimitY); // Review
            }


            RaisePropertyChanged(nameof(OutputValuesX));
            RaisePropertyChanged(nameof(OutputValuesY));
        }

        // Helper
        private List<double> MapValues(List<double> rawValues, double minLimit, double maxLimit)
        {
            var mappedValues = new List<double>();

            foreach(var value in rawValues)
            {                
                mappedValues.Add(minLimit + value / DynamicCanvasSize * (maxLimit - minLimit));
            }
            return mappedValues;
        }

        public void ResetCurves()
        {
            if (SelectedGraphType == GraphTypes.LinearCurve)
            {
                LinearCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.1, DynamicCanvasSize * 0.9);
                LinearCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.9, DynamicCanvasSize * 0.1);
                RaisePropertyChanged(nameof(LinearCurveControlPointData1));
                RaisePropertyChanged(nameof(LinearCurveControlPointData2));
            }
            else if (SelectedGraphType == GraphTypes.SineWave)
            {
                SineWaveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.25, 0);
                SineWaveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.75, DynamicCanvasSize);
                RaisePropertyChanged(nameof(SineWaveControlPointData1));
                RaisePropertyChanged(nameof(SineWaveControlPointData2));
            }
            else if (SelectedGraphType == GraphTypes.CosineWave)
            {
                CosineWaveControlPointData1 = new ControlPointData(0, 0);
                CosineWaveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize);
                RaisePropertyChanged(nameof(CosineWaveControlPointData1));
                RaisePropertyChanged(nameof(CosineWaveControlPointData2));
            }

            GenerateOutputValues();
        }

        // Helper method to extract descriptions from enum values
        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute != null ? attribute.Description : value.ToString();
        }

        #region BuildAst

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
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
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

            return new[] { yValuesAssignment, xValuesAssignment, dataBridgeCall };
        }

        #endregion
    }

    /// <summary>
    /// Represents the different types of graph curves available in the Curve Mapper.
    /// </summary>
    //[TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum GraphTypes
    {
        [Description("Select type")]
        Empty,
        [Description("Linear Curve")]
        LinearCurve,
        [Description("Bezier Curve")]
        BezierCurve,
        [Description("Sine Wave")]
        SineWave,
        [Description("Cosine Wave")]
        CosineWave,
        [Description("Parabolic Curve")]
        ParabolicCurve,
        [Description("Perlin Noise")]
        PerlinNoiseCurve,
        [Description("Power Curve")]
        PowerCurve,
        [Description("Square Root Curve")]
        SquareRootCurve,
        [Description("Gaussian Curve")]
        GaussianCurve
    }

    /// <summary>
    /// Represents a control point with X and Y coordinates, supporting scaling to a new canvas size.
    /// </summary>
    public class ControlPointData
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ControlPointData(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void ScaleToNewCanvasSize(double oldCanvasSize, double newCanvasSize)
        {
            X = (X / oldCanvasSize) * newCanvasSize;
            Y = newCanvasSize - ((oldCanvasSize - Y) / oldCanvasSize) * newCanvasSize;
        }
    }
}
