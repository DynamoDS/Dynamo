using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;

namespace Dynamo.Manipulation
{
    internal class ZeroTouchManipulatorFactory : INodeManipulatorFactory
    {
        private readonly Dictionary<string, IEnumerable<INodeManipulatorFactory>> manipulatorCreators;


        public IEnumerable<string> NodeNames
        {
            get { return manipulatorCreators.Select(pair => pair.Key); }
        }

        internal ZeroTouchManipulatorFactory()
        {
            manipulatorCreators = new Dictionary<string, IEnumerable<INodeManipulatorFactory>>
            {
                {
                    "Autodesk.DesignScript.Geometry.Point.ByCoordinates@double,double,double",
                    new INodeManipulatorFactory[] { new MousePointManipulatorFactory() }
                },
                {
                    "Autodesk.DesignScript.Geometry.Point.ByCoordinates@double,double",
                    new INodeManipulatorFactory[] { new MousePointManipulatorFactory() }
                },
                {
                    "Autodesk.DesignScript.Geometry.Curve.PointAtParameter@double",
                    new INodeManipulatorFactory[] { new PointOnCurveManipulatorFactory() }
                }
            };
        }

        public INodeManipulator Create(NodeModel node, DynamoManipulationExtension manipulatorContext)
        {
            var dsFunction = node as DSFunction;
            if (dsFunction == null) return null;

            var name = dsFunction.CreationName;

            IEnumerable<INodeManipulatorFactory> creators;
            if (manipulatorCreators.TryGetValue(name, out creators))
            {
                return new CompositeManipulator(creators.Select(x => x.Create(node, manipulatorContext)).ToList());
            }
            return null;
        }

    }
}
