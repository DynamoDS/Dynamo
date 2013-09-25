using System.Collections.Generic;

namespace Dynamo
{
    public interface IDrawable
    {
        void RegisterForVisualization();


        void UnregisterFromVisualization();

        /// <summary>
        /// Returns the collection of geometry stored in the visualization manager for this node.
        /// </summary>
        List<object> VisualizationGeometry { get; }
    }
}
