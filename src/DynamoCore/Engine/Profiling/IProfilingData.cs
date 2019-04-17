using System;
using Dynamo.Graph.Nodes;

namespace Dynamo.Engine.Profiling
{
    /// <summary>
    /// Returns information about time spent compiling and exectuing nodes.                                                 
    /// </summary>
    public interface IProfilingData
    {
        /// <summary>
        /// Returns the total amount of time spent compiling and executing nodes.
        /// </summary>
        TimeSpan? TotalExecutionTime { get; }

        /// <summary>
        /// Returns the amount of time spent compiling and executing a specific node.
        /// </summary>
        TimeSpan? NodeExecutionTime(NodeModel node);
    }
}
