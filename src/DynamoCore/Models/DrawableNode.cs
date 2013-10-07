using System.Collections.Generic;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Models
{
    public abstract class DrawableNode: NodeModel
    {
        /// <summary>
        /// Returns the collection of geometry stored in the visualization manager for this node.
        /// </summary>
        public List<object> VisualizationGeometry
        {
            get
            {
                var viz = dynSettings.Controller.VisualizationManager;

                if (viz.Visualizations.ContainsKey(this.GUID.ToString()))
                {
                    return viz.Visualizations[this.GUID.ToString()].Geometry;
                }

                return null;
            }
        }

        protected override void __eval_internal(Microsoft.FSharp.Collections.FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            dynSettings.Controller.VisualizationManager.MarkForUpdate(this);

            base.__eval_internal(args, outPuts);
        }
    }

    public abstract class DrawableNodeWithOneOutput : DrawableNode
    {
        /// <summary>
        /// Returns the collection of geometry stored in the visualization manager for this node.
        /// </summary>
        public List<object> VisualizationGeometry
        {
            get
            {
                var viz = dynSettings.Controller.VisualizationManager;

                if (viz.Visualizations.ContainsKey(this.GUID.ToString()))
                {
                    return viz.Visualizations[this.GUID.ToString()].Geometry;
                }

                return null;
            }
        }

        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public abstract FScheme.Value Evaluate(FSharpList<FScheme.Value> args);

        protected override void __eval_internal(Microsoft.FSharp.Collections.FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            dynSettings.Controller.VisualizationManager.MarkForUpdate(this);

            base.__eval_internal(args, outPuts);
        }
    }
}
