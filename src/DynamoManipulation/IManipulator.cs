using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;

namespace Dynamo.Manipulation
{
    public interface INodeManipulatorContext
    {
    }

    public interface IManipulatorDaemonInitializer
    {
        INodeManipulatorContext ManipulatorContext { get; }    
        Dictionary<Type, IEnumerable<INodeManipulatorCreator>> GetManipulatorCreators();
    }

    /// <summary>
    /// Creates a set of manipulators for the given node by inspecting its numeric inputs
    /// </summary>
    public interface INodeManipulatorCreator
    {
        IManipulator Create(NodeModel node, INodeManipulatorContext manipulatorContext);
    }

    public interface IManipulator : IGraphicItem, IDisposable
    {

    }
}
