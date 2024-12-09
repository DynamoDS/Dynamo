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
        public List<double> MinLimitX { get; set; }
        public List<double> MaxLimitX { get; set; }
        public List<double> MinLimitY { get; set; }
        public List<double> MaxLimitY { get; set; }
        public List<double> Values { get; set; }
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

        public CurveMapperNodeModel()
        {
            RegisterAllPorts();

            PortConnected += CurveMapperNodeModel_PortConnected;
            PortDisconnected += CurveMapperNodeModel_PortDisconnected;
        }

        #endregion

        #region Events
        // ip comment: modify this
        private void CurveMapperNodeModel_PortConnected(PortModel port, ConnectorModel arg2)
        {
            // Reset an info states if any
            //if (port.PortType == PortType.Input && InPorts[2].IsConnected && NodeInfos.Any(x => x.State.Equals(ElementState.PersistentInfo)))
            //{
            //    this.ClearInfoMessages();
            //}

            OnPortUpdated(null);
            RaisePropertyChanged("DataUpdated");
        }
        private void CurveMapperNodeModel_PortDisconnected(PortModel port)
        {
            OnPortUpdated(null);
            // Clear UI when a input port is disconnected
            if (port.PortType == PortType.Input)
            {
                MinLimitX?.Clear();
                MaxLimitX?.Clear();
                MinLimitY?.Clear();
                MaxLimitY?.Clear();
                Values?.Clear();

                RaisePropertyChanged("DataUpdated");
            }
        }

        #endregion
    }

    public class CurveMapperNodeView : INodeViewCustomization<PieChartNodeModel>
    {
        public void CustomizeView(PieChartNodeModel model, NodeView nodeView)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
