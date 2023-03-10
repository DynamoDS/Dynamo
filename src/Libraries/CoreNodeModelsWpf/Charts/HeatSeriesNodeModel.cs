using System;
using System.Collections;
using System.Collections.Generic;
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

namespace CoreNodeModelsWpf.Charts
{
    [IsDesignScriptCompatible]
    [NodeName("Heat Series Plot")]
    [NodeCategory("Display.Charts.Create")]    
    [NodeDescription("ChartsHeatSeriesDescription", typeof(CoreNodeModelWpfResources))]
    [NodeSearchTags("ChartsHeatSeriesSearchTags", typeof(CoreNodeModelWpfResources))]
    [InPortNames("x-labels", "y-labels", "values", "colors")]
    [InPortTypes("List<string>", "List<string>", "List<List<double>>", "List<color>")]
    [InPortDescriptions(typeof(CoreNodeModelWpfResources),
        "ChartsHeatSeriesXLabelsDataPortToolTip",
        "ChartsHeatSeriesYLabelsDataPortToolTip",
        "ChartsHeatSeriesValuesDataPortToolTip",
        "ChartsHeatSeriesColorsDataPortToolTip")]
    [OutPortNames("labels:values")]
    [OutPortTypes("Dictionary<string, Dictionary<string, double>>")]
    [OutPortDescriptions(typeof(CoreNodeModelWpfResources),
        "ChartsHeatSeriesLabelsValuesDataPortToolTip")]
    [AlsoKnownAs("CoreNodeModelsWpf.Charts.HeatSeriesPlot")]
    public class HeatSeriesNodeModel : NodeModel
    {
        #region Properties
        private Random rnd = new Random();

        /// <summary>
        /// A list of X-axis Labels.
        /// </summary>
        public List<string> XLabels { get; set; }

        /// <summary>
        /// A list of Y-axis Labels.
        /// </summary>
        public List<string> YLabels { get; set; }

        /// <summary>
        /// List of lists each containing double values representing items in a column.
        /// </summary>
        public List<List<double>> Values { get; set; }

        /// <summary>
        /// A list of color values, one for each plotted line.
        /// </summary>
        public List<Color> Colors { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public HeatSeriesNodeModel()
        {
            RegisterAllPorts();

            PortDisconnected += XYLineChartNodeModel_PortDisconnected;

            ArgumentLacing = LacingStrategy.Disabled;
        }

        [JsonConstructor]
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public HeatSeriesNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            PortDisconnected += XYLineChartNodeModel_PortDisconnected;
        }
        #endregion

        #region Events
        private void XYLineChartNodeModel_PortDisconnected(PortModel port)
        {
            // Clear UI when a input port is disconnected
            if (port.PortType == PortType.Input && this.State == ElementState.Active)
            {
                XLabels.Clear();
                YLabels.Clear();
                Values.Clear();
                Colors.Clear();

                RaisePropertyChanged("DataUpdated");
            }
        }
        #endregion

        #region Databridge
        // Use the VMDataBridge to safely retrieve our input values

        /// <summary>
        /// Register the data bridge callback.
        /// </summary>
        protected override void OnBuilt()
        {
            base.OnBuilt();
            VMDataBridge.DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        /// <summary>
        /// Callback method for DataBridge mechanism.
        /// This callback only gets called when 
        ///     - The AST is executed
        ///     - After the BuildOutputAST function is executed 
        ///     - The AST is fully built
        /// </summary>
        /// <param name="data">The data passed through the data bridge.</param>
        private void DataBridgeCallback(object data)
        {
            // Reset an info states if any
            if (NodeInfos.Count > 0) this.ClearInfoMessages();

            // Grab input data which always returned as an ArrayList
            var inputs = data as ArrayList;

            // Each of the list inputs are also returned as ArrayLists
            var xLabels = inputs[0] as ArrayList;
            var yLabels = inputs[1] as ArrayList;
            var values = inputs[2] as ArrayList;
            var colors = inputs[3] as ArrayList;

            if (xLabels == null || yLabels == null || values == null)
                return;

            // TODO - is it worth/possible to display jagged data
            // If data is jagged throw warning
            if (xLabels.Count != values.Count || xLabels.Count == 0)
            {
                throw new Exception("Label and Values do not properly align in length.");
            }

            // Clear current chart values
            XLabels = new List<string>();
            YLabels = new List<string>();
            Values = new List<List<double>>();
            Colors = new List<Color>();

            // Iterate the x and y values separately as they may be different lengths
            for (var i = 0; i < xLabels.Count; i++)
            {
                XLabels.Add((string)xLabels[i]);
            }

            for (var i = 0; i < yLabels.Count; i++)
            {
                YLabels.Add((string)yLabels[i]);
            }

            // Iterate values (count should be x-labels length * y-lables length)
            for (var i = 0; i < values.Count; i++)
            {
                var unpackedValues = values[i] as ArrayList;
                var outputValues = new List<double>();

                for (int j = 0; j < unpackedValues.Count; j++)
                {
                    outputValues.Add(Convert.ToDouble(unpackedValues[j]));
                }

                Values.Add(outputValues);
            }

            // If colors is empty add 1 random color
            if (colors == null || colors.Count == 0)
            {
                // In case colors are not provided, we supply some from the default library of colors
                Info(Dynamo.Wpf.Properties.CoreNodeModelWpfResources.ProvideDefaultColorsWarningMessage);

                Color color = Utilities.Colors.GetColor();
                Colors.Add(color);

                Utilities.Colors.ResetColors();
            }


            // If provided with 1 color blend white to color
            // Else create color range from provided color
            else
            {
                for (var i = 0; i < colors.Count; i++)
                {
                    var dynColor = (DSCore.Color)colors[i];
                    var convertedColor = Color.FromArgb(dynColor.Alpha, dynColor.Red, dynColor.Green, dynColor.Blue);
                    Colors.Add(convertedColor);
                }
            }

            // TODO - Should this use Dynamo Scheduler to prevent timing issues with redundant calls?
            // Notify UI the data has been modified
            RaisePropertyChanged("DataUpdated");
        }
        #endregion

        #region Methods
        /// <summary>
        /// BuildOutputAst is where the outputs of this node are calculated.
        /// This method is used to do the work that a compiler usually does 
        /// by parsing the inputs List inputAstNodes into an abstract syntax tree.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // WARNING!!!
            // Do not throw an exception during AST creation.

            AssociativeNode inputNode;

            // If inputs are not connected return default input
            if (!InPorts[0].IsConnected ||
                !InPorts[1].IsConnected ||
                !InPorts[2].IsConnected)
            {
                inputNode = AstFactory.BuildFunctionCall(
                    new Func<List<string>, List<string>, List<List<double>>, List<DSCore.Color>, Dictionary<string, Dictionary<string, double>>>(HeatSeriesFunctions.GetNodeInput),
                    new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1], inputAstNodes[2], inputAstNodes[3] }
                );

                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputNode),
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                        VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes)
                        )
                    ),
                };
            }

            inputNode = AstFactory.BuildFunctionCall(
                new Func<List<string>, List<string>, List<List<double>>, List<DSCore.Color>, Dictionary<string, Dictionary<string, double>>>(HeatSeriesFunctions.GetNodeInput),
                new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1], inputAstNodes[2], inputAstNodes[3] }
            );

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputNode),
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                        VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes)
                    )
                ),
            };
        }
        #endregion

        #region Dispose

        /// <summary>
        /// Finalize the usage of this Node
        /// </summary>
        public override void Dispose()
        {
            PortDisconnected -= XYLineChartNodeModel_PortDisconnected;
            VMDataBridge.DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        #endregion
    }

    /// <summary>
    ///     View customizer for CustomNodeModel Node Model.
    /// </summary>
    public class HeatSeriesNodeView : INodeViewCustomization<HeatSeriesNodeModel>
    {
        private HeatSeriesControl heatSeriesControl;

        /// <summary>
        /// At run-time, this method is called during the node 
        /// creation. Add custom UI element to the node view.
        /// </summary>
        /// <param name="model">The NodeModel representing the node's core logic.</param>
        /// <param name="nodeView">The NodeView representing the node in the graph.</param>
        public void CustomizeView(HeatSeriesNodeModel model, NodeView nodeView)
        {
            heatSeriesControl = new HeatSeriesControl(model);
            nodeView.inputGrid.Children.Add(heatSeriesControl);

            MenuItem exportImage = new MenuItem();
            exportImage.Header = "Export Chart as Image";
            exportImage.Click += ExportImage_Click;

            var contextMenu = (nodeView.Content as Grid).ContextMenu;
            contextMenu.Items.Add(exportImage);
        }

        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            Export.ToPng(heatSeriesControl.HeatSeriesUI);
        }

        /// <summary>
        /// Here you can do any cleanup you require if you've assigned callbacks for particular 
        /// UI events on your node.
        /// </summary>
        public void Dispose() { }
    }
}
