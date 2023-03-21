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
using Dynamo.Graph.Connectors;
using System.Linq;
using Newtonsoft.Json.Linq;
using Dynamo.ViewModels;

namespace CoreNodeModelsWpf.Charts
{
    [IsDesignScriptCompatible]
    [NodeName("Bar Chart")]
    [NodeCategory("Display.Charts.Create")]
    [NodeDescription("ChartsBarChartDescription", typeof(CoreNodeModelWpfResources))]
    [NodeSearchTags("ChartsBarChartSearchTags", typeof(CoreNodeModelWpfResources))]
    [InPortNames("labels", "values", "colors")]
    [InPortTypes("List<string>", "List<var>", "List<color>")]
    [InPortDescriptions(typeof(CoreNodeModelWpfResources),
        "ChartsBarChartLabelsDataPortToolTip",
        "ChartsBarChartValuesDataPortToolTip",
        "ChartsBarChartColorsDataPortToolTip")]
    [OutPortNames("labels:values")]
    [OutPortTypes("Dictionary<string, double>")]
    [OutPortDescriptions(typeof(CoreNodeModelWpfResources),
        "ChartsBarChartLabelsValuesDataPortToolTip")]
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
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public BarChartNodeModel()
        {
            RegisterAllPorts();

            PortConnected += BarChartNodeModel_PortConnected;
            PortDisconnected += BarChartNodeModel_PortDisconnected;

            ArgumentLacing = LacingStrategy.Disabled;
        }

        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        [JsonConstructor]
        public BarChartNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            PortConnected += BarChartNodeModel_PortConnected;
            PortDisconnected += BarChartNodeModel_PortDisconnected;
        }
        #endregion

        #region Events
        private void BarChartNodeModel_PortDisconnected(PortModel port)
        {
            OnPortUpdated(null);
            // Clear UI when a input port is disconnected
            if (port.PortType == PortType.Input)
            {
                Labels?.Clear();
                Values?.Clear();
                Colors?.Clear();

                RaisePropertyChanged("DataUpdated");
            }
        }
        private void BarChartNodeModel_PortConnected(PortModel port, ConnectorModel arg2)
        {
            // Reset an info states if any
            if (port.PortType == PortType.Input && InPorts[2].IsConnected && NodeInfos.Any(x => x.State.Equals(ElementState.Info)))
            {
                this.ClearInfoMessages();
            }

            OnPortUpdated(null);
            RaisePropertyChanged("DataUpdated");
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

            if (!InPorts[0].IsConnected && !InPorts[1].IsConnected && !InPorts[2].IsConnected)
            {
                return;
            }

            // Update chart properties
            Labels = new List<string>();
            Values = new List<List<double>>();
            Colors = new List<SolidColorBrush>();

            var anyNullData = labels == null || values == null;

            // Only continue if key/values match in length
            if (anyNullData || labels.Count != values.Count && labels.Count != (values[0] as ArrayList).Count || labels.Count == 0)
            {
                throw new Exception("Label and Values do not properly align in length.");
            }
            
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
                        if (InPorts[2].IsConnected) return;

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
                        if (InPorts[3].IsConnected) return;

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

            AssociativeNode inputNode;

            // If inputs are not connected return default input
            if (!InPorts[0].IsConnected && !InPorts[1].IsConnected)
            {
                inputNode = AstFactory.BuildFunctionCall(
                    new Func<List<string>, List<double>, List<DSCore.Color>, Dictionary<string, double>>(BarChartFunctions.GetNodeInput),
                    new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1], inputAstNodes[2] }
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
            else if (!InPorts[0].IsConnected || !InPorts[1].IsConnected)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()),
                };
            }
            else
            {
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

        }
        #endregion

        #region Dispose

        /// <summary>
        /// Finalize the usage of this Node
        /// </summary>
        public override void Dispose()
        {
            PortConnected -= BarChartNodeModel_PortConnected;
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
        private NodeView view;
        private BarChartNodeModel model;

        /// <summary>
        /// At run-time, this method is called during the node 
        /// creation. Add custom UI element to the node view.
        /// </summary>
        /// <param name="model">The NodeModel representing the node's core logic.</param>
        /// <param name="nodeView">The NodeView representing the node in the graph.</param>
        public void CustomizeView(BarChartNodeModel model, NodeView nodeView)
        {
            this.model = model;
            this.view = nodeView;
            barChartControl = new BarChartControl(model);
            nodeView.inputGrid.Children.Add(barChartControl);

            MenuItem exportImage = new MenuItem();
            exportImage.Header = "Export Chart as Image";
            exportImage.Click += ExportImage_Click;

            var contextMenu = (nodeView.Content as Grid).ContextMenu;
            contextMenu.Items.Add(exportImage);

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
                    !inPorts[1].IsConnected)
            {
                ((InPortViewModel)inPorts[0]).PortDefaultValueMarkerVisible = true;
                ((InPortViewModel)inPorts[1]).PortDefaultValueMarkerVisible = true;
            }
            else
            {
                ((InPortViewModel)inPorts[0]).PortDefaultValueMarkerVisible = false;
                ((InPortViewModel)inPorts[1]).PortDefaultValueMarkerVisible = false;
            }

            var allPortsConnected = inPorts[0].IsConnected && inPorts[1].IsConnected && model.State != ElementState.Warning;
            var noPortsConnected = !inPorts[0].IsConnected && !inPorts[1].IsConnected;

            // The color input uses default values if it's not connected
            if (!inPorts[2].IsConnected && (allPortsConnected || noPortsConnected))
            {
                ((InPortViewModel)inPorts[2]).PortDefaultValueMarkerVisible = true;
            }
            else
            {
                ((InPortViewModel)inPorts[2]).PortDefaultValueMarkerVisible = false;
            }
        }

        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            Export.ToPng(barChartControl.BarChart);
        }

        /// <summary>
        /// Here you can do any cleanup you require if you've assigned callbacks for particular 
        /// UI events on your node.
        /// </summary>
        public void Dispose()
        {
            model.PortUpdated -= ModelOnPortUpdated;
        }
    }
}
