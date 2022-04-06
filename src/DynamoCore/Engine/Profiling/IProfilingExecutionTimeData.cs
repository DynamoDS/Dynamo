using System;
using Dynamo.Graph.Nodes;

namespace Dynamo.Engine.Profiling
{
    /// <summary>
    /// Returns information about time spent compiling and executing nodes.                                                 
    /// </summary>
    public interface IProfilingExecutionTimeData
    {
        /// <summary>
        /// Returns the total amount of time spent compiling and executing nodes during the most recent graph run.
        /// </summary>
        TimeSpan? TotalExecutionTime { get; }

        /// <summary>
        /// Returns the amount of time spent compiling and executing a specific node.
        /// Returns null if the node was not executed during the most recent graph run.
        /// </summary>
        TimeSpan? NodeExecutionTime(NodeModel node);
    }
}
