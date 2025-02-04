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
using System.Runtime.Serialization;
using System.Windows;

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
        public EngineController EngineController { get; set; }

        private bool isLocked = false;
        [JsonProperty]
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                isLocked = value;
                RaisePropertyChanged(nameof(IsLocked));
            }

        }

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
        private const double defaultCanvasSize = 240;
        private const double defaultMinGridWidth = 310;
        private const double defaultMinGridHeight = 340;
        private double dynamicCanvasSize = defaultCanvasSize;
        private double mainGridWidth = 310;
        private double mainGridHeight = 340;

        [JsonProperty]
        public double DynamicCanvasSize
        {
            get => dynamicCanvasSize;
            set
            {
                if (dynamicCanvasSize != value)
                {
                    dynamicCanvasSize = Math.Max(value, defaultCanvasSize);
                    MainGridWidth = dynamicCanvasSize + 70;
                    MainGridHeight = dynamicCanvasSize + 100;
                    RaisePropertyChanged(nameof(DynamicCanvasSize));
                    OnNodeModified();
                }
            }
        }

        [JsonProperty]
        public double MainGridWidth
        {
            get => mainGridWidth;
            set
            {
                if (mainGridWidth != value)
                {
                    mainGridWidth = Math.Max(value, defaultMinGridWidth);
                    RaisePropertyChanged(nameof(MainGridWidth));
                    OnNodeModified();
                }
            }
        }

        [JsonProperty]
        public double MainGridHeight
        {
            get => mainGridHeight;
            set
            {
                if (mainGridHeight != value)
                {
                    mainGridHeight = Math.Max(value, defaultMinGridHeight);
                    RaisePropertyChanged(nameof(MainGridHeight));
                    OnNodeModified();
                }
            }
        }

        public double MinGridWidth => defaultMinGridWidth;

        public double MinGridHeight => defaultMinGridHeight;

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
        //[JsonIgnore]
        public GraphTypes SelectedGraphType
        {
            get => selectedGraphType;
            set
            {
                selectedGraphType = value;
                GenerateOutputValues();
                RaisePropertyChanged(nameof(SelectedGraphType));
                OnNodeModified();
                UpdateGaussianControlPointsVisibility();
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

        #region Curve/Point properties

        // Linear curve
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointLinear1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointLinear2 { get; set; }

        [JsonIgnore]
        public LinearCurve LinearCurve { get; set; }

        // Bezier curve
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointBezier1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointBezier2 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointBezier1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointBezier2 { get; set; }

        [JsonIgnore]
        public ControlLine ControlLineBezier1 { get; set; }

        [JsonIgnore]
        public ControlLine ControlLineBezier2 { get; set; }

        [JsonIgnore]
        public BezierCurve BezierCurve { get; set; }

        // Sine wave
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointSine1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointSine2 { get; set; }

        [JsonIgnore]
        public SineCurve SineWave { get; set; }

        //Cosine wave
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointCosine1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointCosine2 { get; set; }

        [JsonIgnore]
        public SineCurve CosineWave { get; set; }

        // Parabolic curve
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointParabolic1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointParabolic2 { get; set; }

        [JsonIgnore]
        public ParabolicCurve ParabolicCurve { get; set; }

        // Perlin noise
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointPerlin1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointPerlin2 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointPerlin { get; set; }

        [JsonIgnore]
        public PerlinCurve PerlinNoiseCurve { get; set; }

        // Power noise
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointPower { get; set; }

        [JsonIgnore]
        public PowerCurve PowerCurve { get; set; }

        // SquareRoot curve
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointSquareRoot1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint ControlPointSquareRoot2 { get; set; }

        [JsonIgnore]
        public SquareRootCurve SquareRootCurve { get; set; }

        // Gaussian curve
        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointGaussian1 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointGaussian2 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointGaussian3 { get; set; }

        [JsonConverter(typeof(StringToPointThumbConverter))]
        public CurveMapperControlPoint OrthoControlPointGaussian4 { get; set; }

        [JsonIgnore]
        public GaussianCurve GaussianCurve { get; set; }

        #endregion

        #region Constructors

        public CurveMapperNodeModel()
        {
            if (InPorts.Count == 0)
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
            }
            if (OutPorts.Count == 0)
            {
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData("y-Values",
                    CoreNodeModelWpfResources.CurveMapperOutputDataPortToolTip)));
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData("x-Values",
                    CoreNodeModelWpfResources.CurveMapperOutputDataPortToolTip)));
            }

            RegisterAllPorts();

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            SelectedGraphType = GraphTypes.Empty;
            ArgumentLacing = LacingStrategy.Disabled;

            #region Create Curves/Points

            // Linear curve
            ControlPointLinear1 = new CurveMapperControlPoint(
                   new Point(0, DynamicCanvasSize),
                   DynamicCanvasSize, DynamicCanvasSize,
                   MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            ControlPointLinear2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize, 0),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            LinearCurve = new LinearCurve(
                ControlPointLinear1, ControlPointLinear2,
                DynamicCanvasSize, DynamicCanvasSize
            );
            ControlPointLinear1.CurveLinear = LinearCurve;
            ControlPointLinear2.CurveLinear = LinearCurve;

            // Bezier curve
            ControlPointBezier1 = new CurveMapperControlPoint(
                    new Point(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2),
                    DynamicCanvasSize, DynamicCanvasSize,
                    MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            ControlPointBezier2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            OrthoControlPointBezier1 = new CurveMapperControlPoint(
                new Point(0, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                true, true
            );
            OrthoControlPointBezier2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                true, true
            );
            ControlLineBezier1 = new ControlLine(
                    ControlPointBezier1.Point,
                    OrthoControlPointBezier1.Point
            );
            ControlLineBezier2 = new ControlLine(
                ControlPointBezier2.Point,
                OrthoControlPointBezier2.Point
            );
            BezierCurve = new BezierCurve(
                    OrthoControlPointBezier1,
                    OrthoControlPointBezier2,
                    ControlPointBezier1,
                    ControlPointBezier2,
                    DynamicCanvasSize,
                    DynamicCanvasSize
            );
            OrthoControlPointBezier1.CurveBezier = BezierCurve;
            OrthoControlPointBezier2.CurveBezier = BezierCurve;
            OrthoControlPointBezier1.ControlLineBezier = ControlLineBezier1;
            OrthoControlPointBezier2.ControlLineBezier = ControlLineBezier2;
            ControlPointBezier1.CurveBezier = BezierCurve;
            ControlPointBezier2.CurveBezier = BezierCurve;
            ControlPointBezier1.ControlLineBezier = ControlLineBezier1;
            ControlPointBezier2.ControlLineBezier = ControlLineBezier2;

            // Sine wave
            ControlPointSine1 = new CurveMapperControlPoint(
                    new Point(DynamicCanvasSize * 0.25, 0),
                    DynamicCanvasSize, DynamicCanvasSize,
                    MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            ControlPointSine2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.75, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            SineWave = new SineCurve(
                ControlPointSine1,
                ControlPointSine2,
                DynamicCanvasSize,
                DynamicCanvasSize
            );
            ControlPointSine1.CurveSine = SineWave;
            ControlPointSine2.CurveSine = SineWave;

            // Cosine wave
            ControlPointCosine1 = new CurveMapperControlPoint(
                    new Point(0, 0),
                    DynamicCanvasSize, DynamicCanvasSize,
                    MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            ControlPointCosine2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            CosineWave = new SineCurve(
                ControlPointCosine1,
                ControlPointCosine2,
                DynamicCanvasSize,
                DynamicCanvasSize
            );
            ControlPointCosine1.CurveCosine = CosineWave;
            ControlPointCosine2.CurveCosine = CosineWave;

            // Parabolic curve
            ControlPointParabolic1 = new CurveMapperControlPoint(
                    new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1),
                    DynamicCanvasSize, DynamicCanvasSize,
                    MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            ControlPointParabolic2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            ParabolicCurve = new ParabolicCurve(
                ControlPointParabolic1,
                ControlPointParabolic2,
                DynamicCanvasSize,
                DynamicCanvasSize
            );
            ControlPointParabolic1.CurveParabolic = ParabolicCurve;
            ControlPointParabolic2.CurveParabolic = ParabolicCurve;

            // Perlin noise
            OrthoControlPointPerlin1 = new CurveMapperControlPoint(
                    new Point(DynamicCanvasSize * 0.5, 0),
                    DynamicCanvasSize, DynamicCanvasSize,
                    MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                    true, false
            );
            OrthoControlPointPerlin2 = new CurveMapperControlPoint(
                new Point(0, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                true, true
            );
            ControlPointPerlin = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            PerlinNoiseCurve = new PerlinCurve(
                OrthoControlPointPerlin1,
                OrthoControlPointPerlin2,
                ControlPointPerlin, 1,
                DynamicCanvasSize,
                DynamicCanvasSize
            );
            OrthoControlPointPerlin1.CurvePerlin = PerlinNoiseCurve;
            OrthoControlPointPerlin2.CurvePerlin = PerlinNoiseCurve;
            ControlPointPerlin.CurvePerlin = PerlinNoiseCurve;

            // Power curve
            ControlPointPower = new CurveMapperControlPoint(
                    new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5),
                    DynamicCanvasSize, DynamicCanvasSize,
                    MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            PowerCurve = new PowerCurve(
                   ControlPointPower,
                   DynamicCanvasSize,
                   DynamicCanvasSize
            );
            ControlPointPower.CurvePower = PowerCurve;

            // Square Root curve
            ControlPointSquareRoot1 = new CurveMapperControlPoint(
                new Point(0, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            ControlPointSquareRoot2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize
            );
            SquareRootCurve = new SquareRootCurve(
                ControlPointSquareRoot1,
                ControlPointSquareRoot2,
                DynamicCanvasSize,
                DynamicCanvasSize
            );
            ControlPointSquareRoot1.SquareRootCurve = SquareRootCurve;
            ControlPointSquareRoot2.SquareRootCurve = SquareRootCurve;

            // Gaussian curve
            OrthoControlPointGaussian1 = new CurveMapperControlPoint(
                new Point(0, DynamicCanvasSize * 0.8),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                true, true
            );
            OrthoControlPointGaussian2 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                true, false
            );
            OrthoControlPointGaussian3 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.4, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                true, false
            );
            OrthoControlPointGaussian4 = new CurveMapperControlPoint(
                new Point(DynamicCanvasSize * 0.6, DynamicCanvasSize),
                DynamicCanvasSize, DynamicCanvasSize,
                MinLimitX, MaxLimitX, MinLimitY, MaxLimitY, DynamicCanvasSize,
                true, false
            );
            GaussianCurve = new GaussianCurve(
                OrthoControlPointGaussian1,
                OrthoControlPointGaussian2,
                OrthoControlPointGaussian3,
                OrthoControlPointGaussian4,
                DynamicCanvasSize,
                DynamicCanvasSize
            );
            OrthoControlPointGaussian1.GaussianCurve = GaussianCurve;
            OrthoControlPointGaussian2.GaussianCurve = GaussianCurve;
            OrthoControlPointGaussian3.GaussianCurve = GaussianCurve;
            OrthoControlPointGaussian4.GaussianCurve = GaussianCurve;

            #endregion
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


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Linear Curve
            if (LinearCurve == null && ControlPointLinear1 != null && ControlPointLinear2 != null)
            {
                LinearCurve = new LinearCurve(
                    ControlPointLinear1,
                    ControlPointLinear2,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                ControlPointLinear1.CurveLinear = LinearCurve;
                ControlPointLinear2.CurveLinear = LinearCurve;
            }

            // Bezier Curve
            if (BezierCurve == null && ControlPointBezier1 != null && ControlPointBezier2 != null)
            {
                BezierCurve = new BezierCurve(
                    OrthoControlPointBezier1,
                    OrthoControlPointBezier2,
                    ControlPointBezier1,
                    ControlPointBezier2,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                ControlLineBezier1 = new ControlLine(
                    ControlPointBezier1.Point,
                    OrthoControlPointBezier1.Point
                    );
                ControlLineBezier2 = new ControlLine(
                    ControlPointBezier1.Point,
                    OrthoControlPointBezier1.Point
                    );
                OrthoControlPointBezier1.CurveBezier = BezierCurve;
                OrthoControlPointBezier2.CurveBezier = BezierCurve;
                OrthoControlPointBezier1.ControlLineBezier = ControlLineBezier1;
                OrthoControlPointBezier2.ControlLineBezier = ControlLineBezier2;
                ControlPointBezier1.CurveBezier = BezierCurve;
                ControlPointBezier2.CurveBezier = BezierCurve;
                ControlPointBezier1.ControlLineBezier = ControlLineBezier1;
                ControlPointBezier2.ControlLineBezier = ControlLineBezier2;
            }

            // Ensure SineWave exists
            if (SineWave == null && ControlPointSine1 != null && ControlPointSine2 != null)
            {
                SineWave = new SineCurve(
                    ControlPointSine1,
                    ControlPointSine2,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                ControlPointSine1.CurveSine = SineWave;
                ControlPointSine2.CurveSine = SineWave;
            }

            // Ensure CosineWave exists
            if (CosineWave == null && ControlPointCosine1 != null && ControlPointCosine2 != null)
            {
                CosineWave = new SineCurve(
                    ControlPointCosine1,
                    ControlPointCosine2,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                ControlPointCosine1.CurveCosine = CosineWave;
                ControlPointCosine2.CurveCosine = CosineWave;
            }

            // Ensure ParabolicCurve exists
            if (ParabolicCurve == null && ControlPointParabolic1 != null && ControlPointParabolic2 != null)
            {
                ParabolicCurve = new ParabolicCurve(
                    ControlPointParabolic1,
                    ControlPointParabolic2,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                ControlPointParabolic1.CurveParabolic = ParabolicCurve;
                ControlPointParabolic2.CurveParabolic = ParabolicCurve;
            }

            // Ensure PerlinNoiseCurve exists
            if (PerlinNoiseCurve == null && OrthoControlPointPerlin1 != null && OrthoControlPointPerlin2 != null && ControlPointPerlin != null)
            {
                PerlinNoiseCurve = new PerlinCurve(
                    OrthoControlPointPerlin1,
                    OrthoControlPointPerlin2,
                    ControlPointPerlin,
                    1,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                OrthoControlPointPerlin1.CurvePerlin = PerlinNoiseCurve;
                OrthoControlPointPerlin2.CurvePerlin = PerlinNoiseCurve;
                ControlPointPerlin.CurvePerlin = PerlinNoiseCurve;
            }

            // Ensure PowerCurve exists
            if (PowerCurve == null && ControlPointPower != null)
            {
                PowerCurve = new PowerCurve(
                    ControlPointPower,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                ControlPointPower.CurvePower = PowerCurve;
            }

            // Ensure SquareRootCurve exists
            if (SquareRootCurve == null && ControlPointSquareRoot1 != null && ControlPointSquareRoot2 != null)
            {
                SquareRootCurve = new SquareRootCurve(
                    ControlPointSquareRoot1,
                    ControlPointSquareRoot2,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                ControlPointSquareRoot1.SquareRootCurve = SquareRootCurve;
                ControlPointSquareRoot2.SquareRootCurve = SquareRootCurve;
            }

            // Ensure GaussianCurve exists
            if (GaussianCurve == null && OrthoControlPointGaussian1 != null && OrthoControlPointGaussian2 != null && OrthoControlPointGaussian3 != null && OrthoControlPointGaussian4 != null)
            {
                GaussianCurve = new GaussianCurve(
                    OrthoControlPointGaussian1,
                    OrthoControlPointGaussian2,
                    OrthoControlPointGaussian3,
                    OrthoControlPointGaussian4,
                    DynamicCanvasSize,
                    DynamicCanvasSize
                );
                OrthoControlPointGaussian1.GaussianCurve = GaussianCurve;
                OrthoControlPointGaussian2.GaussianCurve = GaussianCurve;
                OrthoControlPointGaussian3.GaussianCurve = GaussianCurve;
                OrthoControlPointGaussian4.GaussianCurve = GaussianCurve;
            }

            RestoreCurveConnections();
            UpdateGaussianControlPointsVisibility();
        }

        private void RestoreCurveConnections()
        {
            if (ControlPointLinear1 != null && ControlPointLinear2 != null && LinearCurve != null)
            {
                ControlPointLinear1.CurveLinear = LinearCurve;
                ControlPointLinear2.CurveLinear = LinearCurve;
            }
            if (BezierCurve != null && ControlPointBezier1 != null && ControlPointBezier2 != null)
            {
                ControlPointBezier1.CurveBezier = BezierCurve;
                ControlPointBezier2.CurveBezier = BezierCurve;
            }
            if (SineWave != null && ControlPointSine1 != null && ControlPointSine2 != null)
            {
                ControlPointSine1.CurveSine = SineWave;
                ControlPointSine2.CurveSine = SineWave;
            }
            if (CosineWave != null && ControlPointCosine1 != null && ControlPointCosine2 != null)
            {
                ControlPointCosine1.CurveCosine = CosineWave;
                ControlPointCosine2.CurveCosine = CosineWave;
            }
            if (ParabolicCurve != null && ControlPointParabolic1 != null && ControlPointParabolic2 != null)
            {
                ControlPointParabolic1.CurveParabolic = ParabolicCurve;
                ControlPointParabolic2.CurveParabolic = ParabolicCurve;
            }
            if (PerlinNoiseCurve != null && OrthoControlPointPerlin1 != null && OrthoControlPointPerlin2 != null && ControlPointPerlin != null)
            {
                OrthoControlPointPerlin1.CurvePerlin = PerlinNoiseCurve;
                OrthoControlPointPerlin2.CurvePerlin = PerlinNoiseCurve;
                ControlPointPerlin.CurvePerlin = PerlinNoiseCurve;
            }
            if (PowerCurve != null && ControlPointPower != null)
            {
                ControlPointPower.CurvePower = PowerCurve;
            }
            if (SquareRootCurve != null && ControlPointSquareRoot1 != null && ControlPointSquareRoot2 != null)
            {
                ControlPointSquareRoot1.SquareRootCurve = SquareRootCurve;
                ControlPointSquareRoot2.SquareRootCurve = SquareRootCurve;
            }
            if (GaussianCurve != null && OrthoControlPointGaussian1 != null && OrthoControlPointGaussian2 != null && OrthoControlPointGaussian3 != null && OrthoControlPointGaussian4 != null)
            {
                OrthoControlPointGaussian1.GaussianCurve = GaussianCurve;
                OrthoControlPointGaussian2.GaussianCurve = GaussianCurve;
                OrthoControlPointGaussian3.GaussianCurve = GaussianCurve;
                OrthoControlPointGaussian4.GaussianCurve = GaussianCurve;
            }
        }

        internal void GenerateOutputValues()
        {
            // Ensure at least 2 points and a non-vertical curve; otherwise, display a warning.
            if (PointsCount < 2
                || (MinLimitX == MaxLimitX || MinLimitY == MaxLimitY)
                || (SelectedGraphType == GraphTypes.LinearCurve
                    && ControlPointLinear1 != null
                    && ControlPointLinear2 != null
                    && ControlPointLinear1.Point.X == ControlPointLinear2.Point.X)
                || (SelectedGraphType == GraphTypes.CosineWave
                    && ControlPointCosine1 != null
                    && ControlPointCosine2 != null
                    && ControlPointCosine1.Point.X == ControlPointCosine2.Point.X)
                || (SelectedGraphType == GraphTypes.SineWave
                    && ControlPointSine1 != null
                    && ControlPointSine2 != null
                    && ControlPointSine1.Point.X == ControlPointSine2.Point.X)
                || (SelectedGraphType == GraphTypes.ParabolicCurve
                    && ControlPointParabolic1 != null
                    && ControlPointParabolic2 != null
                    && ControlPointParabolic1.Point.X == ControlPointParabolic2.Point.X)
                || (SelectedGraphType == GraphTypes.PowerCurve
                    && ControlPointPower != null
                    && (ControlPointPower.Point.X == MinLimitX || ControlPointPower.Point.Y == MinLimitY))
            //// Review below
            //(SelectedGraphType == GraphTypes.SquareRootCurve && ControlPointSquareRoot1.Point.X == ControlPointSquareRoot2.Point.X) ||
            //(SelectedGraphType == GraphTypes.GaussianCurve && ControlPointGaussian1.Point.X == ControlPointGaussian2.Point.X)
            )
            {
                ClearErrorsAndWarnings();
                Warning(CoreNodeModelWpfResources.CurveMapperInputWarning, isPersistent: true);

                OutputValuesY = null;
                OutputValuesX = null;
            }
            else
            {
                ClearErrorsAndWarnings();

                if (LinearCurve != null && SelectedGraphType == GraphTypes.LinearCurve)
                {
                    OutputValuesY = LinearCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                    OutputValuesX = LinearCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
                }
                else if (BezierCurve != null && SelectedGraphType == GraphTypes.BezierCurve)
                {
                    OutputValuesY = BezierCurve.GetBezierCurveYValues(minLimitY, maxLimitY,
                        pointsCount, DynamicCanvasSize);
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
                else if (SquareRootCurve != null && SelectedGraphType == GraphTypes.SquareRootCurve)
                {
                    OutputValuesY = SquareRootCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                    OutputValuesX = SquareRootCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
                }
                else if (GaussianCurve != null && SelectedGraphType == GraphTypes.GaussianCurve)
                {
                    OutputValuesY = GaussianCurve.GetCurveYValues(minLimitY, maxLimitY, pointsCount);
                    OutputValuesX = GaussianCurve.GetCurveXValues(minLimitX, maxLimitX, pointsCount);
                }
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

        private void UpdateGaussianControlPointsVisibility()
        {
            if (selectedGraphType == GraphTypes.GaussianCurve && OrthoControlPointGaussian3 != null && OrthoControlPointGaussian4 != null)
            {
                OrthoControlPointGaussian3.IsWithinBounds = OrthoControlPointGaussian3.Point.X >= 0 &&
                                                            OrthoControlPointGaussian3.Point.X <= OrthoControlPointGaussian3.LimitWidth;

                OrthoControlPointGaussian4.IsWithinBounds = OrthoControlPointGaussian4.Point.X >= 0 &&
                                                            OrthoControlPointGaussian4.Point.X <= OrthoControlPointGaussian4.LimitWidth;

                RaisePropertyChanged(nameof(OrthoControlPointGaussian3.IsWithinBounds));
                RaisePropertyChanged(nameof(OrthoControlPointGaussian4.IsWithinBounds));
            }
        }


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



        public void ResetCurves()
        {
            if (isLocked) return;

            // Reset the control points to their original positions and regenerate curves
            switch (SelectedGraphType)
            {
                case GraphTypes.LinearCurve:
                    ControlPointLinear1.Point = new Point(0, DynamicCanvasSize);
                    ControlPointLinear2.Point = new Point(DynamicCanvasSize, 0);
                    LinearCurve?.Regenerate();
                    break;
                case GraphTypes.BezierCurve:
                    ControlPointBezier1.Point = new Point(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2);
                    ControlPointBezier2.Point = new Point(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2);
                    OrthoControlPointBezier1.Point = new Point(0, DynamicCanvasSize);
                    OrthoControlPointBezier2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                    ControlLineBezier1?.Regenerate(ControlPointBezier1, OrthoControlPointBezier1);
                    ControlLineBezier2?.Regenerate(ControlPointBezier2, OrthoControlPointBezier2);
                    BezierCurve?.Regenerate();
                    break;
                case GraphTypes.SineWave:
                    ControlPointSine1.Point = new Point(DynamicCanvasSize * 0.25, 0);
                    ControlPointSine2.Point = new Point(DynamicCanvasSize * 0.75, DynamicCanvasSize);
                    SineWave?.Regenerate();
                    break;
                case GraphTypes.CosineWave:
                    ControlPointCosine1.Point = new Point(0, 0);
                    ControlPointCosine2.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize);
                    CosineWave?.Regenerate();
                    break;
                case GraphTypes.ParabolicCurve:
                    ControlPointParabolic1.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1);
                    ControlPointParabolic2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                    ParabolicCurve?.Regenerate();
                    break;
                case GraphTypes.PerlinNoiseCurve:
                    OrthoControlPointPerlin1.Point = new Point(DynamicCanvasSize * 0.5, 0);
                    OrthoControlPointPerlin2.Point = new Point(0, DynamicCanvasSize);
                    ControlPointPerlin.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                    PerlinNoiseCurve?.Regenerate();
                    break;
                case GraphTypes.PowerCurve:
                    ControlPointPower.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                    PowerCurve?.Regenerate();
                    break;
                case GraphTypes.SquareRootCurve:
                    ControlPointSquareRoot1.Point = new Point(0, DynamicCanvasSize);
                    ControlPointSquareRoot2.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                    SquareRootCurve?.Regenerate();
                    break;
                case GraphTypes.GaussianCurve:
                    OrthoControlPointGaussian1.Point = new Point(0, DynamicCanvasSize * 0.8);
                    OrthoControlPointGaussian2.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                    OrthoControlPointGaussian3.Point = new Point(DynamicCanvasSize * 0.4, DynamicCanvasSize);
                    OrthoControlPointGaussian4.Point = new Point(DynamicCanvasSize * 0.6, DynamicCanvasSize);
                    GaussianCurve?.Regenerate();
                    break;
                    // Add mode curves here
            }

            GenerateOutputValues();
            OnNodeModified();
        }
    }
    /// <summary>
    /// Represents the different types of graph curves available in the Curve Mapper.
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
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
    /// JSON converter for serializing and deserializing CurveMapperControlPoint objects.
    /// </summary>
    class StringToPointThumbConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CurveMapperControlPoint);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || string.IsNullOrWhiteSpace(reader.Value.ToString()))
                return null;

            string[] pointData = reader.Value.ToString().Split(',');

            if (pointData.Length < 11)
            {
                throw new JsonSerializationException("Invalid data format for CurveMapperControlPoint");
            }

            if (!double.TryParse(pointData[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) ||
            !double.TryParse(pointData[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double y) ||
            !double.TryParse(pointData[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double limitWidth) ||
            !double.TryParse(pointData[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double limitHeight) ||
            !double.TryParse(pointData[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double minLimitX) ||
            !double.TryParse(pointData[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double maxLimitX) ||
            !double.TryParse(pointData[6], NumberStyles.Any, CultureInfo.InvariantCulture, out double minLimitY) ||
            !double.TryParse(pointData[7], NumberStyles.Any, CultureInfo.InvariantCulture, out double maxLimitY) ||
            !double.TryParse(pointData[8], NumberStyles.Any, CultureInfo.InvariantCulture, out double canvasSize) ||
            !bool.TryParse(pointData[9], out bool isOrthogonal) ||
            !bool.TryParse(pointData[10], out bool isVertical))
            {
                throw new JsonSerializationException("Error parsing CurveMapperControlPoint values");
            }

            return new CurveMapperControlPoint(
                new Point(x, y),
                limitWidth, limitHeight,
                minLimitX, maxLimitX,
                minLimitY, maxLimitY,
                canvasSize,
                isOrthogonal, isVertical
            );
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is CurveMapperControlPoint controlPoint)
            {
                writer.WriteValue(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                controlPoint.Point.X, controlPoint.Point.Y,
                controlPoint.LimitWidth, controlPoint.LimitHeight,
                controlPoint.MinLimitX, controlPoint.MaxLimitX,
                controlPoint.MinLimitY, controlPoint.MaxLimitY,
                controlPoint.CanvasSize, controlPoint.IsOrthogonal,
                controlPoint.IsVertical
            ));
            }
            else
            {
                throw new JsonSerializationException("Expected CurveMapperControlPoint object value.");
            }
        }
    }
}
