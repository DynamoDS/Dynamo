using System.Collections.Generic;

namespace Dynamo
{
    public interface IDrawable
    {
        /// <summary>
        /// Register a node for visualization with the visualization manager.
        /// </summary>
        void RegisterForVisualization();

        /// <summary>
        /// Unregister a node from visualization with the visualization manager.
        /// </summary>
        void UnregisterFromVisualization();

        /// <summary>
        /// Returns the collection of geometry stored in the visualization manager for this node.
        /// </summary>
        List<object> VisualizationGeometry { get; }
    }
}
