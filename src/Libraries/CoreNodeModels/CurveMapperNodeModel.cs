using CoreNodeModels.CurveMapper;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
        private const int rounding = 15;
        private const double defaultCanvasSize = 240;
        private double dynamicCanvasSize = defaultCanvasSize;
        private bool isLocked;

        #region Curves & point data

        // Linear curve
        /// <summary> Point data for the 1st control point of the linear curve. </summary>
        [JsonProperty]
        public ControlPointData LinearCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the linear curve. </summary>
        [JsonProperty]
        public ControlPointData LinearCurveControlPointData2 { get; private set; }
        private LinearCurve linearCurve;

        // Bezier curve
        /// <summary> Point data for the 1st control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData BezierCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData BezierCurveControlPointData2 { get; private set; }
        /// <summary> Point data for the 3rd control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData BezierCurveControlPointData3 { get; private set; }
        /// <summary> Point data for the 4th control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData BezierCurveControlPointData4 { get; private set; }
        private BezierCurve bezierCurve;

        // Sine wave
        /// <summary> Point data for the 1st control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData SineWaveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData SineWaveControlPointData2 { get; private set; }
        private SineWave sineWave;

        // Cosine wave
        /// <summary> Point data for the 1st control point of the cosine wave. </summary>
        [JsonProperty]
        public ControlPointData CosineWaveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the cosine wave. </summary>
        [JsonProperty]
        public ControlPointData CosineWaveControlPointData2 { get; private set; }
        private SineWave cosineWave;

        // Parabolic curve
        /// <summary> Point data for the 1st control point of the parabolic curve. </summary>
        [JsonProperty]
        public ControlPointData ParabolicCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the parabolic curve. </summary>
        [JsonProperty]
        public ControlPointData ParabolicCurveControlPointData2 { get; private set; }
        private ParabolicCurve parabolicCurve;

        // Perlin noise
        /// <summary> Point data for the 1st control point of the perlin noise curve. </summary>
        [JsonProperty]
        public ControlPointData PerlinNoiseControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the perlin noise curve. </summary>
        [JsonProperty]
        public ControlPointData PerlinNoiseControlPointData2 { get; private set; }
        /// <summary> Point data for the 3rd control point of the perlin noise curve. </summary>
        [JsonProperty]
        public ControlPointData PerlinNoiseControlPointData3 { get; private set; }
        private PerlinNoiseCurve perlinNoiseCurve;

        // Power curve
        /// <summary> Point data for the power curve control point. </summary>
        [JsonProperty]
        public ControlPointData PowerCurveControlPointData1 { get; private set; }
        private PowerCurve powerCurve;

        // Square root curve
        /// <summary> Point data for the 1st control point of the square root curve. </summary>
        [JsonProperty]
        public ControlPointData SquareRootCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the square root curve. </summary>
        [JsonProperty]
        public ControlPointData SquareRootCurveControlPointData2 { get; private set; }
        private SquareRootCurve squareRootCurve;

        // Gaussian curve
        /// <summary> Point data for the 1st control point of the Gaussian curve. </summary>
        [JsonProperty]
        public ControlPointData GaussianCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the Gaussian curve. </summary>
        [JsonProperty]
        public ControlPointData GaussianCurveControlPointData2 { get; private set; }
        /// <summary> Point data for the 3rd control point of the Gaussian curve. </summary>
        [JsonProperty]
        public ControlPointData GaussianCurveControlPointData3 { get; private set; }
        /// <summary> Point data for the 4th control point of the Gaussian curve. </summary>
        [JsonProperty]
        public ControlPointData GaussianCurveControlPointData4 { get; private set; }
        private GaussianCurve gaussianCurve;

        #endregion

        #region Inputs

        /// <summary> Gets or sets the minimum X limit for the curve. </summary>
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
        /// <summary> Gets or sets the maximum X limit for the curve. </summary>
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
        /// <summary> Gets or sets the minimum Y limit for the curve. </summary>
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
        /// <summary> Gets or sets the maximum Y limit for the curve. </summary>
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
        /// <summary> Gets or sets the number of points used to compute the curve. </summary>
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
        /// <summary> Gets the midpoint value of the X range. </summary>
        [JsonIgnore]
        public double MidValueX => (MaxLimitX + MinLimitX) * 0.5;
        /// <summary> Gets the midpoint value of the Y range. </summary>
        [JsonIgnore]
        public double MidValueY => (MaxLimitY + MinLimitY) * 0.5;

        #endregion

        #region Outputs

        /// <summary> Gets or sets the computed Y values of the curve for output. </summary>
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
        /// <summary> Gets or sets the computed X values of the curve for output. </summary>
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
        /// <summary> Gets or sets the Y values used for rendering the curve. </summary>
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
        /// <summary> Gets or sets the X values used for rendering the curve. </summary>
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

        /// <summary> Gets or sets the dynamic size of the canvas, scaling control points accordingly. </summary>
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

                    ScaleAllControlPoints(oldSize, dynamicCanvasSize);
                    RaisePropertyChanged(nameof(DynamicCanvasSize));
                    OnNodeModified();
                    GenerateOutputValues();
                }
            }
        }
        /// <summary> Gets a list of graph type descriptions for UI selection. </summary>
        [JsonIgnore]
        public List<string> GraphTypesList => Enum.GetValues(typeof(GraphTypes))
            .Cast<GraphTypes>()
            .Select(value => GetEnumDescription(value))
            .ToList();
        /// <summary> Gets or sets the selected graph type as a description for UI binding. </summary>
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
        /// <summary> Gets or sets the currently selected graph type. </summary>
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
        /// <summary> Gets or sets a value indicating whether the control points are locked. </summary>
        [JsonProperty]
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

            InitiateControlPointData();
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

        private void InitiateControlPointData()
        {
            // Linear Curve
            LinearCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.1, DynamicCanvasSize * 0.9);
            LinearCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.9, DynamicCanvasSize * 0.1);
            // Bezier curve
            BezierCurveControlPointData1 = new ControlPointData(0, DynamicCanvasSize);
            BezierCurveControlPointData2 = new ControlPointData(DynamicCanvasSize, DynamicCanvasSize);
            BezierCurveControlPointData3 = new ControlPointData(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2);
            BezierCurveControlPointData4 = new ControlPointData(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2);
            // Sine wave
            SineWaveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.25, 0);
            SineWaveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.75, DynamicCanvasSize);
            // Cosine wave
            CosineWaveControlPointData1 = new ControlPointData(0, 0);
            CosineWaveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize);
            // Parabolic curve
            ParabolicCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1);
            ParabolicCurveControlPointData2 = new ControlPointData(DynamicCanvasSize, DynamicCanvasSize);
            // Perlin noise curve
            PerlinNoiseControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.5, 0);
            PerlinNoiseControlPointData2 = new ControlPointData(0, DynamicCanvasSize);
            PerlinNoiseControlPointData3 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
            // Power curve
            PowerCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
            // Power curve  
            SquareRootCurveControlPointData1 = new ControlPointData(0, DynamicCanvasSize);
            SquareRootCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
            // Gaussian curve
            GaussianCurveControlPointData1 = new ControlPointData(0, DynamicCanvasSize * 0.8);
            GaussianCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5, "GaussianCurveControlPointData2");
            GaussianCurveControlPointData3 = new ControlPointData(DynamicCanvasSize * 0.4, DynamicCanvasSize, "GaussianCurveControlPointData3");
            GaussianCurveControlPointData4 = new ControlPointData(DynamicCanvasSize * 0.6, DynamicCanvasSize, "GaussianCurveControlPointData4");
        }

        public void GenerateOutputValues()
        {
            if (SelectedGraphType == GraphTypes.Empty)
            {
                RenderValuesX = RenderValuesY = null;
                OutputValuesX = OutputValuesY  = null;
            }
            else
            {
                if (!IsValidCurve())
                {
                    ClearErrorsAndWarnings();
                    Warning("The provided original values cannot be redistributed using the curve equation.", isPersistent: true); // TODO: add to resources

                    OutputValuesY = null;
                    OutputValuesX = null;
                    RenderValuesX = null;
                    RenderValuesY = null;
                    return;
                }

                ClearErrorsAndWarnings();

                object curve = null;

                switch (SelectedGraphType)
                {
                    case GraphTypes.LinearCurve:
                        curve = new LinearCurve(
                            LinearCurveControlPointData1.X, (DynamicCanvasSize - LinearCurveControlPointData1.Y),
                            LinearCurveControlPointData2.X, (DynamicCanvasSize - LinearCurveControlPointData2.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.BezierCurve:
                        curve = new BezierCurve(
                            BezierCurveControlPointData1.X, (DynamicCanvasSize - BezierCurveControlPointData1.Y),
                            BezierCurveControlPointData2.X, (DynamicCanvasSize - BezierCurveControlPointData2.Y),
                            BezierCurveControlPointData3.X, (DynamicCanvasSize - BezierCurveControlPointData3.Y),
                            BezierCurveControlPointData4.X, (DynamicCanvasSize - BezierCurveControlPointData4.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.SineWave:
                        curve = new SineWave(
                            SineWaveControlPointData1.X, (DynamicCanvasSize - SineWaveControlPointData1.Y),
                            SineWaveControlPointData2.X, (DynamicCanvasSize - SineWaveControlPointData2.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.CosineWave:
                        curve = new SineWave(
                            CosineWaveControlPointData1.X, (DynamicCanvasSize - CosineWaveControlPointData1.Y),
                            CosineWaveControlPointData2.X, (DynamicCanvasSize - CosineWaveControlPointData2.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.ParabolicCurve:
                        curve = new ParabolicCurve(
                            ParabolicCurveControlPointData1.X, (DynamicCanvasSize - ParabolicCurveControlPointData1.Y),
                            ParabolicCurveControlPointData2.X, (DynamicCanvasSize - ParabolicCurveControlPointData2.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.PerlinNoiseCurve:
                        curve = new PerlinNoiseCurve(
                            PerlinNoiseControlPointData1.X, (DynamicCanvasSize - PerlinNoiseControlPointData1.Y),
                            PerlinNoiseControlPointData2.X, (DynamicCanvasSize - PerlinNoiseControlPointData2.Y),
                            PerlinNoiseControlPointData3.X, (DynamicCanvasSize - PerlinNoiseControlPointData3.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.PowerCurve:
                        curve = new PowerCurve(
                            PowerCurveControlPointData1.X, (DynamicCanvasSize - PowerCurveControlPointData1.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.SquareRootCurve:
                        curve = new SquareRootCurve(
                            SquareRootCurveControlPointData1.X, (DynamicCanvasSize - SquareRootCurveControlPointData1.Y),
                            SquareRootCurveControlPointData2.X, (DynamicCanvasSize - SquareRootCurveControlPointData2.Y),
                            DynamicCanvasSize
                        );
                        break;
                    case GraphTypes.GaussianCurve:
                        curve = new GaussianCurve(
                            GaussianCurveControlPointData1.X, (DynamicCanvasSize - GaussianCurveControlPointData1.Y),
                            GaussianCurveControlPointData2.X, (DynamicCanvasSize - GaussianCurveControlPointData2.Y),
                            GaussianCurveControlPointData3.X, (DynamicCanvasSize - GaussianCurveControlPointData3.Y),
                            GaussianCurveControlPointData4.X, (DynamicCanvasSize - GaussianCurveControlPointData4.Y),
                            DynamicCanvasSize
                        );
                        break;
                }

                if (curve is not null)
                {
                    // Dynamic to call methods on different curve types
                    dynamic dynamicCurve = curve; 
                    RenderValuesX = dynamicCurve.GetCurveXValues(PointsCount, true);
                    RenderValuesY = dynamicCurve.GetCurveYValues(PointsCount, true);
                    OutputValuesX = MapValues(dynamicCurve.GetCurveXValues(PointsCount), MinLimitX, MaxLimitX);
                    OutputValuesY = MapValues(dynamicCurve.GetCurveYValues(PointsCount), MinLimitY, MaxLimitY);
                }
            }

            RaisePropertyChanged(nameof(OutputValuesX));
            RaisePropertyChanged(nameof(OutputValuesY));
        }

        private bool IsValidCurve() //
        {
            if (PointsCount < 2 || MinLimitX == MaxLimitX || MinLimitY == MaxLimitY)
                return false;

            // Dictionary mapping graph types to control point validation logic
            var controlPointChecks = new Dictionary<GraphTypes, Func<bool>>
            {
                { GraphTypes.LinearCurve, () => LinearCurveControlPointData1.X != LinearCurveControlPointData2.X },
                { GraphTypes.SineWave, () => SineWaveControlPointData1.X != SineWaveControlPointData2.X },
                { GraphTypes.CosineWave, () => CosineWaveControlPointData1.X != CosineWaveControlPointData2.X },
                { GraphTypes.ParabolicCurve, () => ParabolicCurveControlPointData1.X != ParabolicCurveControlPointData2.X },
                { GraphTypes.PowerCurve, () => PowerCurveControlPointData1.X > 0 &&
                PowerCurveControlPointData1.Y > 0 &&
                PowerCurveControlPointData1.X < DynamicCanvasSize &&
                PowerCurveControlPointData1.Y < DynamicCanvasSize }
            };

            // Validate the selected graph type if it exists in the dictionary
            return controlPointChecks.TryGetValue(SelectedGraphType, out var validator) ? validator() : true;
        }

        // Helper
        private List<double> MapValues(List<double> rawValues, double minLimit, double maxLimit) //
        {
            var mappedValues = new List<double>();

            foreach(var value in rawValues)
            {                
                mappedValues.Add(Math.Round(minLimit + value / DynamicCanvasSize * (maxLimit - minLimit), rounding));
            }
            return mappedValues;
        }

        private void ScaleAllControlPoints(double oldSize, double newSize)
        {
            var controlPoints = new List<ControlPointData>
            {
                LinearCurveControlPointData1, LinearCurveControlPointData2,
                BezierCurveControlPointData1, BezierCurveControlPointData2,
                BezierCurveControlPointData3, BezierCurveControlPointData4,
                SineWaveControlPointData1, SineWaveControlPointData2,
                CosineWaveControlPointData1, CosineWaveControlPointData2,
                ParabolicCurveControlPointData1, ParabolicCurveControlPointData2,
                PerlinNoiseControlPointData1, PerlinNoiseControlPointData2, PerlinNoiseControlPointData3,
                PowerCurveControlPointData1,
                SquareRootCurveControlPointData1, SquareRootCurveControlPointData2,
                GaussianCurveControlPointData1, GaussianCurveControlPointData2,
                GaussianCurveControlPointData3, GaussianCurveControlPointData4
            };

            foreach (var point in controlPoints)
            {
                point.ScaleToNewCanvasSize(oldSize, newSize);
            }
        }

        /// <summary>
        /// Resets the curves to their original state
        /// </summary>
        public void ResetControlPointData() //
        {
            if (SelectedGraphType == GraphTypes.LinearCurve)
            {
                LinearCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.1, DynamicCanvasSize * 0.9);
                LinearCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.9, DynamicCanvasSize * 0.1);
                RaisePropertyChanged(nameof(LinearCurveControlPointData1));
                RaisePropertyChanged(nameof(LinearCurveControlPointData2));
            }
            else if (SelectedGraphType == GraphTypes.BezierCurve)
            {
                BezierCurveControlPointData1 = new ControlPointData(0, DynamicCanvasSize);
                BezierCurveControlPointData2 = new ControlPointData(DynamicCanvasSize, DynamicCanvasSize);
                BezierCurveControlPointData3 = new ControlPointData(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2);
                BezierCurveControlPointData4 = new ControlPointData(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2);
                RaisePropertyChanged(nameof(BezierCurveControlPointData1));
                RaisePropertyChanged(nameof(BezierCurveControlPointData2));
                RaisePropertyChanged(nameof(BezierCurveControlPointData3));
                RaisePropertyChanged(nameof(BezierCurveControlPointData4));
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
            else if (SelectedGraphType == GraphTypes.ParabolicCurve)
            {
                ParabolicCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1);
                ParabolicCurveControlPointData2 = new ControlPointData(DynamicCanvasSize, DynamicCanvasSize);
                RaisePropertyChanged(nameof(ParabolicCurveControlPointData1));
                RaisePropertyChanged(nameof(ParabolicCurveControlPointData2));
            }
            else if (SelectedGraphType == GraphTypes.PerlinNoiseCurve)
            {
                PerlinNoiseControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.5, 0);
                PerlinNoiseControlPointData2 = new ControlPointData(0, DynamicCanvasSize);
                PerlinNoiseControlPointData3 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                RaisePropertyChanged(nameof(PerlinNoiseControlPointData1));
                RaisePropertyChanged(nameof(PerlinNoiseControlPointData2));
                RaisePropertyChanged(nameof(PerlinNoiseControlPointData3));
            }
            else if (SelectedGraphType == GraphTypes.PowerCurve)
            {
                PowerCurveControlPointData1 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                RaisePropertyChanged(nameof(PowerCurveControlPointData1));
            }
            else if (SelectedGraphType == GraphTypes.SquareRootCurve)
            {
                SquareRootCurveControlPointData1 = new ControlPointData(0, DynamicCanvasSize);
                SquareRootCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                RaisePropertyChanged(nameof(SquareRootCurveControlPointData1));
                RaisePropertyChanged(nameof(SquareRootCurveControlPointData2));
            }
            else if (SelectedGraphType == GraphTypes.GaussianCurve)
            {
                GaussianCurveControlPointData1 = new ControlPointData(0, DynamicCanvasSize * 0.8);
                GaussianCurveControlPointData2 = new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5, "GaussianCurveControlPointData2");
                GaussianCurveControlPointData3 = new ControlPointData(DynamicCanvasSize * 0.4, DynamicCanvasSize, "GaussianCurveControlPointData3");
                GaussianCurveControlPointData4 = new ControlPointData(DynamicCanvasSize * 0.6, DynamicCanvasSize, "GaussianCurveControlPointData4");
                RaisePropertyChanged(nameof(GaussianCurveControlPointData1));
                RaisePropertyChanged(nameof(GaussianCurveControlPointData2));
                RaisePropertyChanged(nameof(GaussianCurveControlPointData3));
                RaisePropertyChanged(nameof(GaussianCurveControlPointData4));
            }

            GenerateOutputValues();
        }

        // Helper method to extract descriptions from enum values
        private string GetEnumDescription(Enum value) //
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute != null ? attribute.Description : value.ToString();
        }

        /// <summary>
        /// Updates Gaussian control points positions while maintaining relative spacing and canvas boundaries.
        /// </summary>
        public void UpdateGaussianCurveControlPoints(double deltaX, string tag) //
        {
            switch (tag)
            {
                case "GaussianCurveControlPointData2":
                    GaussianCurveControlPointData3 = new ControlPointData(
                        GaussianCurveControlPointData3.X + deltaX,
                        GaussianCurveControlPointData3.Y,
                        "GaussianCurveControlPointData3");
                    GaussianCurveControlPointData4 = new ControlPointData(
                        GaussianCurveControlPointData4.X + deltaX,
                        GaussianCurveControlPointData4.Y,
                        "GaussianCurveControlPointData4");
                    break;

                case "GaussianCurveControlPointData3":
                    GaussianCurveControlPointData3 = new ControlPointData(
                        GaussianCurveControlPointData3.X + deltaX,
                        GaussianCurveControlPointData3.Y,
                        "GaussianCurveControlPointData3");
                    GaussianCurveControlPointData4 = new ControlPointData(
                        GaussianCurveControlPointData4.X - deltaX,
                        GaussianCurveControlPointData4.Y,
                        "GaussianCurveControlPointData4");
                    break;

                case "GaussianCurveControlPointData4":
                    GaussianCurveControlPointData4 = new ControlPointData(
                        GaussianCurveControlPointData4.X + deltaX,
                        GaussianCurveControlPointData4.Y,
                        "GaussianCurveControlPointData4");
                    GaussianCurveControlPointData3 = new ControlPointData(
                        GaussianCurveControlPointData3.X - deltaX,
                        GaussianCurveControlPointData3.Y,
                        "GaussianCurveControlPointData3");

                    break;
            }

            RaisePropertyChanged(nameof(GaussianCurveControlPointData3));
            RaisePropertyChanged(nameof(GaussianCurveControlPointData4));
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
        public string Tag { get; set; }

        public ControlPointData(double x, double y, string tag = "")
        {
            X = x;
            Y = y;
            Tag = tag;
        }

        public void ScaleToNewCanvasSize(double oldCanvasSize, double newCanvasSize)
        {
            X = (X / oldCanvasSize) * newCanvasSize;
            Y = newCanvasSize - ((oldCanvasSize - Y) / oldCanvasSize) * newCanvasSize;
        }
    }


}
