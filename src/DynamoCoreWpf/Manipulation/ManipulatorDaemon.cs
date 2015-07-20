using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.Wpf.Manipulation
{
    internal class ManipulatorDaemon
    {
        private readonly Dictionary<Type, IEnumerable<INodeManipulatorCreator>> registeredManipulatorCreators;
        private readonly Dictionary<Guid, IDisposable> activeManipulators = new Dictionary<Guid, IDisposable>();

        private ManipulatorDaemon(Dictionary<Type, IEnumerable<INodeManipulatorCreator>> creators)
        {
            registeredManipulatorCreators = creators;
        }

        public static ManipulatorDaemon Create(IManipulatorDaemonInitializer initializer)
        {
            return new ManipulatorDaemon(initializer.GetManipulatorCreators());
        }

        public void CreateManipulator(NodeModel model, DynamoView view)
        {
            var creators = registeredManipulatorCreators.Where(pair => pair.Key.IsInstanceOfType(model)).SelectMany(pair => pair.Value);
            activeManipulators[model.GUID] = new CompositeManipulator(
                creators.Select(x => x.Create(model, new DynamoManipulatorContext { View = view })).
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
