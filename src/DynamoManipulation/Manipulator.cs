using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace DynamoManipulation
{
    public abstract class NodeManipulatorCreator : INodeManipulatorCreator
    {
        /// <summary>
        /// Create a set of manipulators for the given node by inspecting its numeric inputs
        /// </summary>
        /// <param name="node"></param>
        /// <param name="manipulatorContext"></param>
        /// <returns></returns>
        public IManipulator Create(Dynamo.Models.NodeModel node, NodeManipulatorContext manipulatorContext)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Manipulator : IManipulator
    {
        
        public abstract void Dispose();

        public abstract void Tessellate(IRenderPackage package, TessellationParameters parameters);

        protected void DrawManipulator(IEnumerable<IRenderPackage> packages)
        {
            
        }
    }

    public class CompositeManipulator : Manipulator
    {
        private readonly List<IManipulator> subManipulators;
        public CompositeManipulator(IEnumerable<IManipulator> manipulators)
        {
            subManipulators = manipulators.ToList();
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
