using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;

namespace Dynamo.Manipulation
{
    public interface IManipulatorDaemonInitializer
    {
        //ManipulatorDaemon CreateManipulatorDaemon();
        Dictionary<Type, IEnumerable<INodeManipulatorCreator>> GetManipulatorCreators();
    }

    /// <summary>
    /// Creates a set of manipulators for the given node by inspecting its numeric inputs
    /// </summary>
    public interface INodeManipulatorCreator
    {
        IManipulator Create(NodeModel node, NodeManipulatorContext manipulatorContext);
    }

    public interface IManipulator : IGraphicItem, IDisposable
    {

    }
}
