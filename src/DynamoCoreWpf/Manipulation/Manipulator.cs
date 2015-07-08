using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.Wpf.Manipulation
{
    public abstract class NodeManipulatorCreator : INodeManipulatorCreator
    {
        /// <summary>
        /// Create a set of manipulators for the given node by inspecting its numeric inputs
        /// </summary>
        /// <param name="node"></param>
        /// <param name="manipulatorContext"></param>
        /// <returns></returns>
        public IManipulator Create(Models.NodeModel node, DynamoManipulatorContext manipulatorContext)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Manipulator : IManipulator
    {
        public void Dispose()
        {
        }
    }

    internal class CompositeManipulator : IManipulator
    {
        private readonly List<IManipulator> subManipulators;
        public CompositeManipulator(List<IManipulator> manipulators)
        {
            subManipulators = manipulators;
        }

        public void Dispose()
        {
            foreach (var sub in subManipulators)
                sub.Dispose();
        }
    }
}
