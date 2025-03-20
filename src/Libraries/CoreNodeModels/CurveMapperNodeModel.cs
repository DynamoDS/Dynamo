using DSCore.CurveMapper;
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
    [NodeDescription("CurveMapperDescription", typeof(Properties.Resources))]
    [NodeSearchTags("CurveMapperSearchTags", typeof(Properties.Resources))]
    public class CurveMapperNodeModel : NodeModel
    {
        private double minLimitX = 0;
        private double maxLimitX = 1;
        private double minLimitY = 0;
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

        private const string gaussianCurveControlPointData2Tag = "GaussianCurveControlPointData2";
        private const string gaussianCurveControlPointData3Tag = "GaussianCurveControlPointData3";
        private const string gaussianCurveControlPointData4Tag = "GaussianCurveControlPointData4";

        private ControlPointData DefaultLinearCurvePoint1 => new ControlPointData(0, DynamicCanvasSize);
        private ControlPointData DefaultLinearCurvePoint2 => new ControlPointData(DynamicCanvasSize, 0);
        private ControlPointData DefaultBezierCurvePoint1 => new ControlPointData(0, DynamicCanvasSize);
        private ControlPointData DefaultBezierCurvePoint2 => new ControlPointData(DynamicCanvasSize, DynamicCanvasSize);
        private ControlPointData DefaultBezierCurvePoint3 => new ControlPointData(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2);
        private ControlPointData DefaultBezierCurvePoint4 => new ControlPointData(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2);
        private ControlPointData DefaultSineWavePoint1 => new ControlPointData(DynamicCanvasSize * 0.25, 0);
        private ControlPointData DefaultSineWavePoint2 => new ControlPointData(DynamicCanvasSize * 0.75, DynamicCanvasSize);
        private ControlPointData DefaultCosineWavePoint1 => new ControlPointData(0, 0);
        private ControlPointData DefaultCosineWavePoint2 => new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize);
        private ControlPointData DefaultParabolicCurvePoint1 => new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1);
        private ControlPointData DefaultParabolicCurvePoint2 => new ControlPointData(DynamicCanvasSize, DynamicCanvasSize);
        private ControlPointData DefaultPerlinNoiseCurvePoint1 => new ControlPointData(DynamicCanvasSize * 0.5, 0);
        private ControlPointData DefaultPerlinNoiseCurvePoint2 => new ControlPointData(0, DynamicCanvasSize);
        private ControlPointData DefaultPerlinNoiseCurvePoint3 => new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
        private ControlPointData DefaultPowerCurvePoint1 => new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
        private ControlPointData DefaultSquareRootCurvePoint1 => new ControlPointData(0, DynamicCanvasSize);
        private ControlPointData DefaultSquareRootCurvePoint2 => new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
        private ControlPointData DefaultGaussianCurvePoint1 => new ControlPointData(0, DynamicCanvasSize * 0.8);
        private ControlPointData DefaultGaussianCurvePoint2 => new ControlPointData(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5, gaussianCurveControlPointData2Tag);
        private ControlPointData DefaultGaussianCurvePoint3 => new ControlPointData(DynamicCanvasSize * 0.4, DynamicCanvasSize, gaussianCurveControlPointData3Tag);
        private ControlPointData DefaultGaussianCurvePoint4 => new ControlPointData(DynamicCanvasSize * 0.6, DynamicCanvasSize, gaussianCurveControlPointData4Tag);


        private GraphTypes selectedGraphType;
        private const double defaultCanvasSize = 240;
        private double dynamicCanvasSize = defaultCanvasSize;
        private bool isLocked;

        #region Curves & Point Data
        
        /// <summary> Point data for the 1st control point of the linear curve. </summary>
        [JsonProperty]
        public ControlPointData LinearCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the linear curve. </summary>
        [JsonProperty]
        public ControlPointData LinearCurveControlPointData2 { get; private set; }
        private LinearCurve linearCurve;

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

        /// <summary> Point data for the 1st control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData SineWaveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the sine wave. </summary>
        [JsonProperty]
        public ControlPointData SineWaveControlPointData2 { get; private set; }
        private SineWave sineWave;

        /// <summary> Point data for the 1st control point of the cosine wave. </summary>
        [JsonProperty]
        public ControlPointData CosineWaveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the cosine wave. </summary>
        [JsonProperty]
        public ControlPointData CosineWaveControlPointData2 { get; private set; }
        private SineWave cosineWave;

        /// <summary> Point data for the 1st control point of the parabolic curve. </summary>
        [JsonProperty]
        public ControlPointData ParabolicCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the parabolic curve. </summary>
        [JsonProperty]
        public ControlPointData ParabolicCurveControlPointData2 { get; private set; }
        private ParabolicCurve parabolicCurve;

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

        /// <summary> Point data for the power curve control point. </summary>
        [JsonProperty]
        public ControlPointData PowerCurveControlPointData1 { get; private set; }
        private PowerCurve powerCurve;

        /// <summary> Point data for the 1st control point of the square root curve. </summary>
        [JsonProperty]
        public ControlPointData SquareRootCurveControlPointData1 { get; private set; }
        /// <summary> Point data for the 2nd control point of the square root curve. </summary>
        [JsonProperty]
        public ControlPointData SquareRootCurveControlPointData2 { get; private set; }
        private SquareRootCurve squareRootCurve;

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
        [JsonProperty]
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

        /// <summary> Gets or sets the maximum X limit for the curve. </summary>
        [JsonProperty]
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

        /// <summary> Gets or sets the minimum Y limit for the curve. </summary>
        [JsonProperty]
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

        /// <summary> Gets or sets the maximum Y limit for the curve. </summary>
        [JsonProperty]
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

        /// <summary> Gets or sets the number of points used to compute the curve. </summary>
        [JsonProperty]
        public int PointsCount
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

        /// <summary> Gets the midpoint value of the X range. </summary>
        [JsonIgnore]
        public double MidValueX => (MaxLimitX + MinLimitX) * 0.5;

        /// <summary> Gets the midpoint value of the Y range. </summary>
        [JsonIgnore]
        public double MidValueY => (MaxLimitY + MinLimitY) * 0.5;

        #endregion

        #region Output

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
                    dynamicCanvasSize = System.Math.Max(value, defaultCanvasSize);

                    ScaleAllControlPoints(oldSize, dynamicCanvasSize);
                    RaisePropertyChanged(nameof(DynamicCanvasSize));
                    OnNodeModified();
                    GenerateRenderValues();
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
                GenerateRenderValues();
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

        #region Constructors

        public CurveMapperNodeModel()
        {
            if (InPorts.Count == 0)
            {
                InPorts.Add(new PortModel(PortType.Input, this, new PortData(
                    Properties.Resources.CurveMapperXMinLimitInputPortName,
                    Properties.Resources.CurveMapperXMinLimitInputPortToolTip,
                    minLimitXDefaultValue
                    )));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData(
                    Properties.Resources.CurveMapperXMaxLimitInputPortName,
                    Properties.Resources.CurveMapperXMaxLimitInputPortToolTip,
                    maxLimitXDefaultValue
                    )));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData(
                    Properties.Resources.CurveMapperYMinLimitInputPortName,
                    Properties.Resources.CurveMapperYMinLimitInputPortToolTip,
                    minLimitYDefaultValue
                    )));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData(
                    Properties.Resources.CurveMapperYMaxLimitInputPortName,
                    Properties.Resources.CurveMapperYMaxLimitInputPortToolTip,
                    maxLimitYDefaultValue
                    )));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData(
                    Properties.Resources.CurveMapperCountInputPortName,
                    Properties.Resources.CurveMapperCountInputPortToolTip,
                    pointsCountDefaultValue
                    )));
            }
            if (OutPorts.Count == 0)
            {
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData(
                    Properties.Resources.CurveMapperYValuesOutputPortName,
                    Properties.Resources.CurveMapperYValuesOutputPortToolTip
                    )));
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData(
                    Properties.Resources.CurveMapperXValuesOutputPortName,
                    Properties.Resources.CurveMapperXValuesOutputPortToolTip
                    )));
            }

            RegisterAllPorts();

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            SelectedGraphType = GraphTypes.Empty;
            ArgumentLacing = LacingStrategy.Disabled;

            InitiateControlPointData();
            GenerateRenderValues();
        }

        [JsonConstructor]
        public CurveMapperNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts,
            double dynamicCanvasSize = defaultCanvasSize) : base(inPorts, outPorts)
        {
            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            DynamicCanvasSize = dynamicCanvasSize;
            ArgumentLacing = LacingStrategy.Disabled;
        }

        #endregion

        #region Event Handers

        private void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnNodeModified();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates and updates the X and Y render values based on the selected graph type and control points, or displays a warning if the curve is invalid.
        /// </summary>
        public void GenerateRenderValues()
        {
            ClearErrorsAndWarnings();

            if (SelectedGraphType == GraphTypes.Empty)
            {
                RenderValuesX = RenderValuesY = null;
                return;
            }
            if (!IsValidCurve())
            {
                ClearErrorsAndWarnings();
                Warning(Properties.Resources.CurveMapperWarningMessage, isPersistent: true);

                RenderValuesX = RenderValuesY = null;
                return;
            }
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
                dynamic dynamicCurve = curve;
                RenderValuesX = dynamicCurve.GetCurveXValues(PointsCount, true);
                RenderValuesY = dynamicCurve.GetCurveYValues(PointsCount, true);
            }
        }

        /// <summary>
        /// Resets the curves to their original state
        /// </summary>
        public void ResetControlPointData()
        {
            var propertyNames = new List<string>();

            if (SelectedGraphType == GraphTypes.LinearCurve)
            {
                LinearCurveControlPointData1 = DefaultLinearCurvePoint1;
                LinearCurveControlPointData2 = DefaultLinearCurvePoint2;
                propertyNames = new List<string>() { nameof(LinearCurveControlPointData1), nameof(LinearCurveControlPointData2) };
            }
            else if (SelectedGraphType == GraphTypes.BezierCurve)
            {
                BezierCurveControlPointData1 = DefaultBezierCurvePoint1;
                BezierCurveControlPointData2 = DefaultBezierCurvePoint2;
                BezierCurveControlPointData3 = DefaultBezierCurvePoint3;
                BezierCurveControlPointData4 = DefaultBezierCurvePoint4;
                propertyNames = new List<string>()
                {
                    nameof(BezierCurveControlPointData1), nameof(BezierCurveControlPointData2),
                    nameof(BezierCurveControlPointData3), nameof(BezierCurveControlPointData4)
                };
            }
            else if (SelectedGraphType == GraphTypes.SineWave)
            {
                SineWaveControlPointData1 = DefaultSineWavePoint1;
                SineWaveControlPointData2 = DefaultSineWavePoint2;
                propertyNames = new List<string>() { nameof(SineWaveControlPointData1), nameof(SineWaveControlPointData2) };
            }
            else if (SelectedGraphType == GraphTypes.CosineWave)
            {
                CosineWaveControlPointData1 = DefaultCosineWavePoint1;
                CosineWaveControlPointData2 = DefaultCosineWavePoint2;
                propertyNames = new List<string>() { nameof(CosineWaveControlPointData1), nameof(CosineWaveControlPointData2) };
            }
            else if (SelectedGraphType == GraphTypes.ParabolicCurve)
            {
                ParabolicCurveControlPointData1 = DefaultParabolicCurvePoint1;
                ParabolicCurveControlPointData2 = DefaultParabolicCurvePoint2;
                propertyNames = new List<string>() { nameof(ParabolicCurveControlPointData1), nameof(ParabolicCurveControlPointData2) };
            }
            else if (SelectedGraphType == GraphTypes.PerlinNoiseCurve)
            {
                PerlinNoiseControlPointData1 = DefaultPerlinNoiseCurvePoint1;
                PerlinNoiseControlPointData2 = DefaultPerlinNoiseCurvePoint2;
                PerlinNoiseControlPointData3 = DefaultPerlinNoiseCurvePoint3;
                propertyNames = new List<string>()
                {
                    nameof(PerlinNoiseControlPointData1), nameof(PerlinNoiseControlPointData2),
                    nameof(PerlinNoiseControlPointData3)
                };
            }
            else if (SelectedGraphType == GraphTypes.PowerCurve)
            {
                PowerCurveControlPointData1 = DefaultPowerCurvePoint1;
                propertyNames = new List<string>() { nameof(PowerCurveControlPointData1) };
            }
            else if (SelectedGraphType == GraphTypes.SquareRootCurve)
            {
                SquareRootCurveControlPointData1 = DefaultSquareRootCurvePoint1;
                SquareRootCurveControlPointData2 = DefaultSquareRootCurvePoint2;
                propertyNames = new List<string>() { nameof(SquareRootCurveControlPointData1), nameof(SquareRootCurveControlPointData2) };
            }
            else if (SelectedGraphType == GraphTypes.GaussianCurve)
            {
                GaussianCurveControlPointData1 = DefaultGaussianCurvePoint1;
                GaussianCurveControlPointData2 = DefaultGaussianCurvePoint2;
                GaussianCurveControlPointData3 = DefaultGaussianCurvePoint3;
                GaussianCurveControlPointData4 = DefaultGaussianCurvePoint4;
                propertyNames = new List<string>()
                {
                    nameof(GaussianCurveControlPointData1), nameof(GaussianCurveControlPointData2),
                    nameof(GaussianCurveControlPointData3), nameof(GaussianCurveControlPointData4)
                };
            }

            foreach (var propertyName in propertyNames)
            {
                RaisePropertyChanged(propertyName);
            }

            GenerateRenderValues();
        }

        /// <summary>
        /// Updates Gaussian control points positions while maintaining relative spacing and canvas boundaries.
        /// </summary>
        public void UpdateGaussianCurveControlPoints(double deltaX, string tag)
        {
            switch (tag)
            {
                case gaussianCurveControlPointData2Tag:
                    GaussianCurveControlPointData3 = new ControlPointData(
                        GaussianCurveControlPointData3.X + deltaX,
                        GaussianCurveControlPointData3.Y,
                        gaussianCurveControlPointData3Tag);
                    GaussianCurveControlPointData4 = new ControlPointData(
                        GaussianCurveControlPointData4.X + deltaX,
                        GaussianCurveControlPointData4.Y,
                        gaussianCurveControlPointData4Tag);
                    break;

                case gaussianCurveControlPointData3Tag:
                    GaussianCurveControlPointData3 = new ControlPointData(
                        GaussianCurveControlPointData3.X + deltaX,
                        GaussianCurveControlPointData3.Y,
                        gaussianCurveControlPointData3Tag);
                    GaussianCurveControlPointData4 = new ControlPointData(
                        GaussianCurveControlPointData4.X - deltaX,
                        GaussianCurveControlPointData4.Y,
                        gaussianCurveControlPointData4Tag);
                    break;

                case gaussianCurveControlPointData4Tag:
                    GaussianCurveControlPointData4 = new ControlPointData(
                        GaussianCurveControlPointData4.X + deltaX,
                        GaussianCurveControlPointData4.Y,
                        gaussianCurveControlPointData4Tag);
                    GaussianCurveControlPointData3 = new ControlPointData(
                        GaussianCurveControlPointData3.X - deltaX,
                        GaussianCurveControlPointData3.Y,
                        gaussianCurveControlPointData3Tag);

                    break;
            }

            RaisePropertyChanged(nameof(GaussianCurveControlPointData3));
            RaisePropertyChanged(nameof(GaussianCurveControlPointData4));
        }

        #endregion

        #region Private Methods

        private bool IsValidInput()
        {
            return PointsCount >= 2
                && MinLimitX != MaxLimitX
                && MinLimitY != MaxLimitY;
        }

        private bool IsValidCurve()
        {
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

            return controlPointChecks.TryGetValue(SelectedGraphType, out var validator) ? validator() : true;
        }

        private void InitiateControlPointData()
        {
            // Linear Curve
            LinearCurveControlPointData1 = DefaultLinearCurvePoint1;
            LinearCurveControlPointData2 = DefaultLinearCurvePoint2;
            // Bezier curve
            BezierCurveControlPointData1 = DefaultBezierCurvePoint1;
            BezierCurveControlPointData2 = DefaultBezierCurvePoint2;
            BezierCurveControlPointData3 = DefaultBezierCurvePoint3;
            BezierCurveControlPointData4 = DefaultBezierCurvePoint4;
            // Sine wave
            SineWaveControlPointData1 = DefaultSineWavePoint1;
            SineWaveControlPointData2 = DefaultSineWavePoint2;
            // Cosine wave
            CosineWaveControlPointData1 = DefaultCosineWavePoint1;
            CosineWaveControlPointData2 = DefaultCosineWavePoint2;
            // Parabolic curve
            ParabolicCurveControlPointData1 = DefaultParabolicCurvePoint1;
            ParabolicCurveControlPointData2 = DefaultParabolicCurvePoint2;
            // Perlin noise curve
            PerlinNoiseControlPointData1 = DefaultPerlinNoiseCurvePoint1;
            PerlinNoiseControlPointData2 = DefaultPerlinNoiseCurvePoint2;
            PerlinNoiseControlPointData3 = DefaultPerlinNoiseCurvePoint3;
            // Power curve
            PowerCurveControlPointData1 = DefaultPowerCurvePoint1;
            // Square root curve  
            SquareRootCurveControlPointData1 = DefaultSquareRootCurvePoint1;
            SquareRootCurveControlPointData2 = DefaultSquareRootCurvePoint2;
            // Gaussian curve
            GaussianCurveControlPointData1 = DefaultGaussianCurvePoint1;
            GaussianCurveControlPointData2 = DefaultGaussianCurvePoint2;
            GaussianCurveControlPointData3 = DefaultGaussianCurvePoint3;
            GaussianCurveControlPointData4 = DefaultGaussianCurvePoint4;
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
            }.Where(p => p != null).ToList();

            foreach (var point in controlPoints)
            {
                point?.ScaleToNewCanvasSize(oldSize, newSize);
            }
        }

        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute != null ? attribute.Description : value.ToString();
        }

        #endregion

        #region AST Methods

        protected override void OnBuilt()
        {
            base.OnBuilt();
            VMDataBridge.DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        private void DataBridgeCallback(object data)
        {
            ClearErrorsAndWarnings();

            // Ignore invalid inputs & grab input data
            if (!(data is ArrayList inputs) || inputs.Count < 5) return;

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
            foreach (var propertyName in new[] { nameof(MinLimitX), nameof(MaxLimitX), nameof(MinLimitY), nameof(MaxLimitY), nameof(PointsCount) })
            {
                RaisePropertyChanged(propertyName);
            }

            if (!IsValidInput())
            {
                Warning(Properties.Resources.CurveMapperWarningMessage, isPersistent: true);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // Return null outputs if GraphType is Empty
            if (SelectedGraphType == GraphTypes.Empty || !IsValidCurve())
            {
                return new[]
                {
                    AstFactory.BuildAssignment( GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()),
                    AstFactory.BuildAssignment( GetAstIdentifierForOutputIndex(1), AstFactory.BuildNullNode())
                };
            }

            // Map GraphType to corresponding control points
            var controlPointMap = new Dictionary<GraphTypes, List<(double X, double Y)>>()
            {
                [GraphTypes.LinearCurve] = new() {
                    (LinearCurveControlPointData1.X, LinearCurveControlPointData1.Y),
                    (LinearCurveControlPointData2.X, LinearCurveControlPointData2.Y)
                },
                [GraphTypes.BezierCurve] = new() {
                    (BezierCurveControlPointData1.X, BezierCurveControlPointData1.Y),
                    (BezierCurveControlPointData2.X, BezierCurveControlPointData2.Y),
                    (BezierCurveControlPointData3.X, BezierCurveControlPointData3.Y),
                    (BezierCurveControlPointData4.X, BezierCurveControlPointData4.Y)
                },
                [GraphTypes.SineWave] = new() {
                    (SineWaveControlPointData1.X, SineWaveControlPointData1.Y),
                    (SineWaveControlPointData2.X, SineWaveControlPointData2.Y)
                },
                [GraphTypes.CosineWave] = new() {
                    (CosineWaveControlPointData1.X, CosineWaveControlPointData1.Y),
                    (CosineWaveControlPointData2.X, CosineWaveControlPointData2.Y)
                },
                [GraphTypes.ParabolicCurve] = new() {
                    (ParabolicCurveControlPointData1.X, ParabolicCurveControlPointData1.Y),
                    (ParabolicCurveControlPointData2.X, ParabolicCurveControlPointData2.Y)
                },
                [GraphTypes.PerlinNoiseCurve] = new() {
                    (PerlinNoiseControlPointData1.X, PerlinNoiseControlPointData1.Y),
                    (PerlinNoiseControlPointData2.X, PerlinNoiseControlPointData2.Y),
                    (PerlinNoiseControlPointData3.X, PerlinNoiseControlPointData3.Y)
                },
                [GraphTypes.PowerCurve] = new() {
                    (PowerCurveControlPointData1.X,
                    PowerCurveControlPointData1.Y)
                },
                [GraphTypes.SquareRootCurve] = new() {
                    (SquareRootCurveControlPointData1.X, SquareRootCurveControlPointData1.Y),
                    (SquareRootCurveControlPointData2.X, SquareRootCurveControlPointData2.Y)
                },
                [GraphTypes.GaussianCurve] = new() {
                    (GaussianCurveControlPointData1.X, GaussianCurveControlPointData1.Y),
                    (GaussianCurveControlPointData2.X, GaussianCurveControlPointData2.Y),
                    (GaussianCurveControlPointData3.X, GaussianCurveControlPointData3.Y),
                    (GaussianCurveControlPointData4.X, GaussianCurveControlPointData4.Y)
                }
            };

            // Build controlPointsList dynamically
            var controlPointsList = AstFactory.BuildExprList(
                controlPointMap[SelectedGraphType]
                .SelectMany(cp => new AssociativeNode[]
                {
                    AstFactory.BuildDoubleNode(cp.X),
                    AstFactory.BuildDoubleNode(DynamicCanvasSize - cp.Y)
                })
                .Cast<AssociativeNode>()
                .ToList()
                );

            // Handle input values with fall-back defaults
            var inputValues = new List<AssociativeNode>
            {
                InPorts[0].IsConnected ? inputAstNodes[0] : minLimitXDefaultValue,
                InPorts[1].IsConnected ? inputAstNodes[1] : maxLimitXDefaultValue,
                InPorts[2].IsConnected ? inputAstNodes[2] : minLimitYDefaultValue,
                InPorts[3].IsConnected ? inputAstNodes[3] : maxLimitYDefaultValue,
                InPorts[4].IsConnected ? inputAstNodes[4] : pointsCountDefaultValue
            };

            var curveInputs = new List<AssociativeNode> {controlPointsList, AstFactory.BuildDoubleNode(DynamicCanvasSize)};
            curveInputs.AddRange(inputValues);
            curveInputs.Add(AstFactory.BuildStringNode(SelectedGraphType.ToString()));

            AssociativeNode buildResultNode =
                AstFactory.BuildFunctionCall(
                    new Func<List<double>, double, double, double, double, double, int, string, List<List<double>>>(
                        CurveMapperGenerator.CalculateValues),
                    curveInputs
                );

            // DataBridge call
            var dataBridgeCall = AstFactory.BuildAssignment(
                AstFactory.BuildIdentifier(AstIdentifierBase + "_dataBridge"),
                VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputValues))
            );

            // Assign outputs
            var xValuesAssignment = AstFactory.BuildAssignment(
                GetAstIdentifierForOutputIndex(0),
                AstFactory.BuildIndexExpression(buildResultNode, AstFactory.BuildIntNode(0))
            );

            var yValuesAssignment = AstFactory.BuildAssignment(
                GetAstIdentifierForOutputIndex(1),
                AstFactory.BuildIndexExpression(buildResultNode, AstFactory.BuildIntNode(1))
            );

            return new[] { xValuesAssignment, yValuesAssignment, dataBridgeCall };
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
