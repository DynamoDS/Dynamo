using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;

namespace Dynamo.Manipulation
{
    public class ManipulatorDaemon
    {
        private readonly Dictionary<Type, IEnumerable<INodeManipulatorCreator>> registeredManipulatorCreators;
        private readonly Dictionary<Guid, IDisposable> activeManipulators = new Dictionary<Guid, IDisposable>();
        private readonly INodeManipulatorContext manipulatorContext;

        private ManipulatorDaemon(Dictionary<Type, IEnumerable<INodeManipulatorCreator>> creators, INodeManipulatorContext manipulatorContext)
        {
            registeredManipulatorCreators = creators;
            this.manipulatorContext = manipulatorContext;
        }

        public static ManipulatorDaemon Create(IManipulatorDaemonInitializer initializer)
        {
            return new ManipulatorDaemon(initializer.GetManipulatorCreators(), initializer.ManipulatorContext);
        }

        public void CreateManipulator(NodeModel model)
        {
            var creators = registeredManipulatorCreators.Where(pair => pair.Key.IsInstanceOfType(model)).SelectMany(pair => pair.Value);
            activeManipulators[model.GUID] = new CompositeManipulator(
                creators.Select(x => x.Create(model, manipulatorContext)).
                Where(x => x != null).ToList());
        }

        public void KillManipulators(NodeModel model)
        {
            IDisposable manipulator;
            if (activeManipulators.TryGetValue(model.GUID, out manipulator))
            {
                manipulator.Dispose();
                activeManipulators.Remove(model.GUID);
            }
        }

        public void KillAll()
        {
            foreach (var manipulator in activeManipulators)
            {
                manipulator.Value.Dispose();
            }
            activeManipulators.Clear();
        }
    }
}
