using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.DesignScript.Runtime;
using CoreNodes.ChartHelpers;
using CoreNodeModelsWpf.Charts.Controls;
using CoreNodeModelsWpf.Charts.Utilities;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using DynamoServices;
using Dynamo.Wpf.Properties;
using Xceed.Wpf.Toolkit;
using Dynamo.Graph.Connectors;
using Dynamo.ViewModels;
using System.Collections.Generic;
using System;
using CoreNodeModelsWpf.Converters;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms.VisualStyles;

namespace CoreNodeModelsWpf.Charts
{
    [IsDesignScriptCompatible]
    [NodeName("Curve Mapper")]
    //[NodeCategory("Display.Charts.Create")]
    //[NodeDescription("ChartsCurveMapperDescription", typeof(CoreNodeModelWpfResources))]
    //[NodeSearchTags("ChartsCurveMapperSearchTags", typeof(CoreNodeModelWpfResources))]
    [InPortNames("x-MinLimit", "x-MaxLimit", "y-MinLimit", "y-MaxLimit", "list")]
    [InPortTypes("List<double>", "List<double>", "List<double>", "List<double>", "List<double>")]
    //[InPortDescriptions(typeof(CoreNodeModelWpfResources),
    //    "ChartsCurveMapperLabelsDataPortToolTip",
    //    "ChartsCurveMapperValuesDataPortToolTip",
    //    "ChartsCurveMapperColorsDataPortToolTip")]
    [OutPortNames("list")]
    [OutPortTypes("List<double>")]
    //[OutPortDescriptions(typeof(CoreNodeModelWpfResources),
    //    "ChartsCurveMapperLabelsValuesDataPortToolTip")]
    [AlsoKnownAs("CoreNodeModelsWpf.Charts.CurveMapper")]
    public class CurveMapperNodeModel : NodeModel
    {
        #region Properties
        private double minLimitX;
        private double maxLimitX;
        private double minLimitY;
        private double maxLimitY;

        [JsonProperty(PropertyName = "MinLimitX")]
        public double MinLimitX
        {
            get => minLimitX;
            set
            {
                minLimitX = value;
                this.RaisePropertyChanged(nameof(MinLimitX));
                OnNodeModified();
            }
        }

        [JsonProperty(PropertyName = "MaxLimitX")]
        public double MaxLimitX
        {
            get => maxLimitX;
            set
            {
                maxLimitX = value;
                this.RaisePropertyChanged(nameof(MaxLimitX));
                OnNodeModified();
            }
        }
        [JsonProperty(PropertyName = "MinLimitY")]
        public double MinLimitY
        {
            get => minLimitY;
            set
            {
                minLimitY = value;
                this.RaisePropertyChanged(nameof(MinLimitY));
                OnNodeModified();
            }
        }
        [JsonProperty(PropertyName = "MaxLimitY")]
        public double MaxLimitY
        {
            get => maxLimitY;
            set
            {
                maxLimitY = value;
                this.RaisePropertyChanged(nameof(MaxLimitY));
                OnNodeModified();
            }
        }
        public double MidValueX => (MaxLimitX + MinLimitX) * 0.5;
        public double MidValueY => (MaxLimitY + MinLimitY) * 0.5;

        public List<double> Values { get; set; }

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
            RegisterAllPorts();

            //PortConnected += CurveMapperNodeModel_PortConnected;
            //PortDisconnected += CurveMapperNodeModel_PortDisconnected;
            //this.PropertyChanged += ColorRange_PropertyChanged;

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            SelectedGraphType = GraphTypes.Linear;
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
        //private void GraphMapNodeModel_PortDisconnected(PortModel obj)
        //{
        //    if (obj.PortType == PortType.Input && this.State == ElementState.Active)
        //    {
        //        RaisePropertyChanged("DataUpdated");
        //    }
        //}

        private void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnNodeModified(); // This will ensure the node is re-executed
        }
        //private void UpdateGraph()
        //{
        //    // Logic to redraw grid or handle input updates
        //    OnNodeModified(); // This will ensure the node is re-executed
        //}





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

            UpdateDefaultInPortValues();

            model.PortUpdated += ModelOnPortUpdated;
        }

        private void ModelOnPortUpdated(object sender, EventArgs e)
        {
            UpdateDefaultInPortValues();
        }

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
                    !inPorts[3].IsConnected)
            {
                ((InPortViewModel)inPorts[0]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[1]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[2]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[3]).PortDefaultValueMarkerVisible = true;
            }
            else
            {
                ((InPortViewModel)inPorts[0]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[1]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[2]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[3]).PortDefaultValueMarkerVisible = false;
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
        [Description("Linear Curve")]
        Linear = 0,
        [Description("Bezier Curve")]
        Bezier = 1,
        [Description("Sine Wave")]
        SineWave = 2,
        [Description("Cosine Wave")]
        CosineWave = 3,
        [Description("Tangent Wave")]
        TangentWave = 4,
        [Description("Gaussian Wave")]
        GaussianWave = 5,
        [Description("Parabolic Curve")]
        Parabola = 6,
        [Description("Perlin Noise")]
        PerlinNoise = 7
    }

    #endregion
}
