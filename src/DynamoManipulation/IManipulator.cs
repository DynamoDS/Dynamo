using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;

namespace Dynamo.Manipulation
{
    //public interface INodeManipulatorContext
    //{
    //}

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

    public interface IManipulator : IDisposable
    {

    }
}
