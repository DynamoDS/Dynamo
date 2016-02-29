using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;

namespace Dynamo.Manipulation
{
    internal class ManipulatorDaemon
    {
        private readonly Dictionary<Type, IEnumerable<INodeManipulatorFactory>> registeredManipulatorCreators;
        private readonly Dictionary<Guid, IDisposable> activeManipulators = new Dictionary<Guid, IDisposable>();

        public IEnumerable<string> NodeNames { get; private set; }

        /// <summary>
        /// Checks if the given node has any manipulator attached with it.
        /// </summary>
        /// <param name="node">Input Node</param>
        /// <returns>true if node has any manipulator</returns>
        public bool HasNodeManipulator(NodeModel node)
        {
            IDisposable manipulator;
            return activeManipulators.TryGetValue(node.GUID, out manipulator);
        }

        private ManipulatorDaemon(Dictionary<Type, IEnumerable<INodeManipulatorFactory>> creators, IEnumerable<string> nodeNames)
        {
            registeredManipulatorCreators = creators;
            NodeNames = nodeNames;
        }

        internal static ManipulatorDaemon Create(INodeManipulatorFactoryLoader initializer)
        {
            var creators = initializer.Load();
            var names = creators.SelectMany(pair => pair.Value.SelectMany(x =>
            {
                var factory = x as ZeroTouchManipulatorFactory;
                return factory != null ? factory.NodeNames : null;
            }
            ));

            return new ManipulatorDaemon(creators, names);
        }

        internal void CreateManipulator(NodeModel model, DynamoManipulationExtension manipulatorContext)
        {
            var creators = registeredManipulatorCreators.Where(pair => pair.Key.IsInstanceOfType(model)).SelectMany(pair => pair.Value);
            activeManipulators[model.GUID] = new CompositeManipulator(
                creators.Select(x => x.Create(model, manipulatorContext)).
                Where(x => x != null).ToList());
        }

        internal void KillManipulators(NodeModel model)
        {
            IDisposable manipulator;
            if (activeManipulators.TryGetValue(model.GUID, out manipulator))
            {
                manipulator.Dispose();
                activeManipulators.Remove(model.GUID);
            }
        }

        internal void KillAll()
        {
            foreach (var manipulator in activeManipulators)
            {
                manipulator.Value.Dispose();
            }
            activeManipulators.Clear();
        }
    }
}
