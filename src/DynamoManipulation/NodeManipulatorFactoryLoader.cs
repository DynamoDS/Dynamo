using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes.ZeroTouch;

namespace Dynamo.Manipulation
{
    public class NodeManipulatorFactoryLoader : INodeManipulatorFactoryLoader
    {
        public Dictionary<Type, IEnumerable<INodeManipulatorFactory>> Load()
        {
            return new Dictionary<Type, IEnumerable<INodeManipulatorFactory>>
            {
                { typeof(DSFunction), new[] { new ZeroTouchManipulatorFactory() } }
            };
        }
    }
}
