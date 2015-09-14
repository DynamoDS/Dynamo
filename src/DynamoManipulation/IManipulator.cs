using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;

namespace DynamoManipulation
{
    public class NodeManipulatorContext
    {
        public IDisposable Context { get; private set; }

        public NodeManipulatorContext(IDisposable context)
        {
            Context = context;
        }
    }

    public interface IManipulatorDaemonInitializer
    {
        Dictionary<Type, IEnumerable<INodeManipulatorCreator>> GetManipulatorCreators();
    }

    /// <summary>
    /// Creates a set of manipulators for the given node by inspecting its numeric inputs
    /// </summary>
    public interface INodeManipulatorCreator
    {
        IManipulator Create(NodeModel node, NodeManipulatorContext manipulatorContext);
    }

    public interface IManipulator : IDisposable, IGraphicItem
    {
    }
}
