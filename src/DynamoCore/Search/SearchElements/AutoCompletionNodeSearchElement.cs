using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///      Class for all Auto-Completion Node search elements.
    ///      This class will contain the info related to Auto-completion functionality of the matching node elements.
    /// </summary>
    public abstract class AutoCompletionNodeSearchElement
    {
        internal int PortToConnect { get; set; }
    }
}
