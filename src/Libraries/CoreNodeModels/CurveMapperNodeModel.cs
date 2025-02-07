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
        private bool isLocked = false;



        public ControlPointData ControlPoint1 { get; private set; }
        public ControlPointData ControlPoint2 { get; private set; }



        [JsonIgnore]
        public double MinLimitX
        {
            get => minLimitX;
            set
            {
                if (minLimitX != value)
                {
                    minLimitX = value;
                    //GenerateOutputValues();
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
                    //GenerateOutputValues();
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
                    //GenerateOutputValues();
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
                    //GenerateOutputValues();
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
                    //GenerateOutputValues();
                    this.RaisePropertyChanged(nameof(PointsCount));
                    OnNodeModified();
                }
            }
        }
        [JsonIgnore]
        public double MidValueX => (MaxLimitX + MinLimitX) * 0.5;
        [JsonIgnore]
        public double MidValueY => (MaxLimitY + MinLimitY) * 0.5;

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

        [JsonProperty]
        public double DynamicCanvasSize
        {
            get => dynamicCanvasSize;
            set
            {
                if (dynamicCanvasSize != value)
                {
                    dynamicCanvasSize = Math.Max(value, defaultCanvasSize);
                    //MainGridWidth = dynamicCanvasSize + 70;
                    //MainGridHeight = dynamicCanvasSize + 100;
                    RaisePropertyChanged(nameof(DynamicCanvasSize));
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
                //GenerateOutputValues();
                RaisePropertyChanged(nameof(SelectedGraphType));
                OnNodeModified();
                //UpdateGaussianControlPointsVisibility();
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
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData("y-Values",
                    "add ToolTip")));
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData("x-Values",
                    "add ToolTip")));
            }

            RegisterAllPorts();

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            SelectedGraphType = GraphTypes.Empty;
            ArgumentLacing = LacingStrategy.Disabled;

            // Setup control points and instantiate corresponding curves
            //InitializeControlPointsAndCurves();


            // Create control points
            ControlPoint1 = new ControlPointData(dynamicCanvasSize * 0.3, DynamicCanvasSize * 0.3);
            ControlPoint2 = new ControlPointData(DynamicCanvasSize * 0.7, DynamicCanvasSize * 0.7);
        }

        private void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnNodeModified();
        }

        public void PointUpdated(ControlPointData point, double newX, double newY)
        {
            // executes when point is moved
            point.X = newX;
            point.Y = newY;
            GenerateOutputValues();
            RaisePropertyChanged(nameof(ControlPoint1)); // Notify that ControlPoint1 has changed
            RaisePropertyChanged(nameof(ControlPoint2)); // Notify that ControlPoint2 has changed
        }

        public void GenerateOutputValues()
        {
            OutputValuesX = new List<double> { ControlPoint1.X, ControlPoint2.X };
            OutputValuesY = new List<double> { ControlPoint1.Y, ControlPoint2.Y };
            RaisePropertyChanged(nameof(OutputValuesX));
            RaisePropertyChanged(nameof(OutputValuesY));
        }





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
        LinearCurve
        //    ,
        //[Description("Bezier Curve")]
        //BezierCurve,
        //[Description("Sine Wave")]
        //SineWave,
        //[Description("Cosine Wave")]
        //CosineWave,
        //[Description("Parabolic Curve")]
        //ParabolicCurve,
        //[Description("Perlin Noise")]
        //PerlinNoiseCurve,
        //[Description("Power Curve")]
        //PowerCurve,
        //[Description("Square Root Curve")]
        //SquareRootCurve,
        //[Description("Gaussian Curve")]
        //GaussianCurve
    }



    public class ControlPointData
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ControlPointData(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
