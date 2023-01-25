using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.DesignScript.Runtime;
using CoreNodes.ChartHelpers;
using CoreNodeModelsWpf.Charts.Controls;
using CoreNodeModelsWpf.Charts.Utilities;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;
using LiveCharts.Wpf;
using Dynamo.UI;
using DynamoServices;
using Dynamo.Wpf.Properties;

namespace CoreNodeModelsWpf.Charts
{
    [IsDesignScriptCompatible]
    [NodeName("Bar Chart")]
    [NodeCategory("Display.Charts.Create")]
    [NodeDescription("ChartsBarChartDescription", typeof(CoreNodeModelWpfResources))]
    [NodeSearchTags("ChartsBarChartSearchTags", typeof(CoreNodeModelWpfResources))]

    [InPortTypes("List<string>", "List<var>", "List<color>")]
    [OutPortTypes("Dictionary<Label, Value>")]
    [AlsoKnownAs("CoreNodeModelsWpf.Charts.BarChart")]
    public class BarChartNodeModel : NodeModel
    {
        #region Properties
        private Random rnd = new Random();
        private bool isNestedList;

        /// <summary>
        /// Bar chart labels.
        /// </summary>
        public List<string> Labels { get; set; }

        /// <summary>
        /// Bar chart values.
        /// </summary>
        public List<List<double>> Values { get; set; }

        /// <summary>
        /// Bar chart color values.
        /// </summary>
        public List<SolidColorBrush> Colors { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public BarChartNodeModel()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("labels", "A list of labels for the bar chart categories.")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("values", "A list (of lists) to supply values for the bars in each category.")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("colors", "A list of colors for each bar chart category.")));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("labels:values", "Dictionary containing label:value key-pairs")));

            RegisterAllPorts();

            PortDisconnected += BarChartNodeModel_PortDisconnected;

            ArgumentLacing = LacingStrategy.Disabled;
        }

        [JsonConstructor]
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public BarChartNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            PortDisconnected += BarChartNodeModel_PortDisconnected;
        }
        #endregion

        #region Events
        private void BarChartNodeModel_PortDisconnected(PortModel port)
        {
            // Clear UI when a input port is disconnected
            if (port.PortType == PortType.Input && this.State == ElementState.Active)
            {
                Labels.Clear();
                Values.Clear();
                Colors.Clear();

                RaisePropertyChanged("DataUpdated");
            }
        }
        #endregion

        #region databridge
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
        /// Callback method for DataBridge mechanism.PortDisconnected 
        /// This callback only gets called when 
        ///     - The AST is executed
        ///     - After the BuildOutputAST function is executed 
        ///     - The AST is fully built
        /// </summary>
        /// <param name="data">The data passed through the data bridge.</param>
        private void DataBridgeCallback(object data)
        {
            // Grab input data which always returned as an ArrayList
            var inputs = data as ArrayList;

            // Each of the list inputs are also returned as ArrayLists
            var labels = inputs[0] as ArrayList;
            var values = inputs[1] as ArrayList;
            var colors = inputs[2] as ArrayList;

            // Only continue if key/values match in length
            if (labels.Count != values.Count && labels.Count != (values[0] as ArrayList).Count)
            {
                throw new Exception("Label and Values do not properly align in length.");
            }

            // Update chart properties
            Labels = new List<string>();
            Values = new List<List<double>>();
            Colors = new List<SolidColorBrush>();

            // If the bar chart contains nested lists
            if (values[0] is ArrayList)
            {
                isNestedList = true;

                for (var i = 0; i < labels.Count; i++)
                {
                    Labels.Add((string)labels[i]);

                    var unpackedValues = values[i] as ArrayList;
                    var labelValues = new List<double>();

                    for (var j = 0; j < unpackedValues.Count; j++)
                    {
                        labelValues.Add(Convert.ToDouble(unpackedValues[j]));
                    }

                    Values.Add(labelValues);

                    Color color;
                    if (colors == null || colors.Count == 0 || colors.Count != labels.Count)
                    {
                        // In case colors are not provided, we supply some from the default library of colors
                        Info(Dynamo.Wpf.Properties.CoreNodeModelWpfResources.ProvideDefaultColorsWarningMessage);

                        color = Utilities.Colors.GetColor();
                    }
                    else
                    {

                        var dynColor = (DSCore.Color)colors[i];
                        color = Color.FromArgb(dynColor.Alpha, dynColor.Red, dynColor.Green, dynColor.Blue);
                    }

                    SolidColorBrush brush = new SolidColorBrush(color);
                    brush.Freeze();
                    Colors.Add(brush);
                }
            }
            else
            {
                isNestedList = false;

                for (var i = 0; i < labels.Count; i++)
                {
                    Labels.Add((string)labels[i]);
                    
                    var labelValues = new List<double>();
                    
                    Values.Add(new List<double>{Convert.ToDouble(values[i])} );

                    Color color;
                    if (colors == null || colors.Count == 0 || colors.Count != labels.Count)
                    {
                        color = Utilities.Colors.GetColor();
                    }
                    else
                    {
                        var dynColor = (DSCore.Color)colors[i];
                        color = Color.FromArgb(dynColor.Alpha, dynColor.Red, dynColor.Green, dynColor.Blue);
                    }

                    SolidColorBrush brush = new SolidColorBrush(color);
                    brush.Freeze();
                    Colors.Add(brush);
                }
            }

            Utilities.Colors.ResetColors();

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

            // If inputs are not connected return null
            if (!InPorts[0].IsConnected ||
                !InPorts[1].IsConnected)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()),
                };
            }

            AssociativeNode inputNode;
            if (isNestedList)
            {
                inputNode = AstFactory.BuildFunctionCall(
                    new Func<List<string>, List<List<double>>, List<DSCore.Color>, Dictionary<string, List<double>>>(BarChartFunctions.GetNodeInput),
                    new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1], inputAstNodes[2] }
                );
            }
            else
            {
                inputNode = AstFactory.BuildFunctionCall(
                    new Func<List<string>, List<double>, List<DSCore.Color>, Dictionary<string, double>>(BarChartFunctions.GetNodeInput),
                    new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1], inputAstNodes[2] }
                );
            }

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
            PortDisconnected -= BarChartNodeModel_PortDisconnected;
            VMDataBridge.DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        #endregion
    }

    /// <summary>
    ///     View customizer for CustomNodeModel Node Model.
    /// </summary>
    public class BarChartNodeView : INodeViewCustomization<BarChartNodeModel>
    {
        private BarChartControl barChartControl;

        /// <summary>
        /// At run-time, this method is called during the node 
        /// creation. Add custom UI element to the node view.
        /// </summary>
        /// <param name="model">The NodeModel representing the node's core logic.</param>
        /// <param name="nodeView">The NodeView representing the node in the graph.</param>
        public void CustomizeView(BarChartNodeModel model, NodeView nodeView)
        {
            barChartControl = new BarChartControl(model);
            nodeView.inputGrid.Children.Add(barChartControl);

            MenuItem exportImage = new MenuItem();
            exportImage.Header = "Export Chart as Image";
            exportImage.Click += ExportImage_Click;

            var contextMenu = (nodeView.Content as Grid).ContextMenu;
            contextMenu.Items.Add(exportImage);
        }

        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            Export.ToPng(barChartControl.BarChart);
        }

        /// <summary>
        /// Here you can do any cleanup you require if you've assigned callbacks for particular 
        /// UI events on your node.
        /// </summary>
        public void Dispose() { }
    }
}
