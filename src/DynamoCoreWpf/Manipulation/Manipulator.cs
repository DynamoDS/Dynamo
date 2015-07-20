using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
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
        public abstract void Dispose();

        public abstract void Tessellate(IRenderPackage package, TessellationParameters parameters);
    }

    public class CompositeManipulator : Manipulator
    {
        private readonly List<IManipulator> subManipulators;
        public CompositeManipulator(List<IManipulator> manipulators)
        {
            subManipulators = manipulators;
        }

        public override void Dispose()
        {
            subManipulators.ForEach(x => x.Dispose());
        }

        public override void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            subManipulators.ForEach(x => x.Tessellate(package, parameters));
        }
    }
}
