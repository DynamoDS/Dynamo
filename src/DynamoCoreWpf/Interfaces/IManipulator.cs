using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;
using Dynamo.Wpf.Manipulation;

namespace Dynamo.Wpf.Interfaces
{
    public interface IManipulatorDaemonInitializer
    {
        Dictionary<Type, IEnumerable<INodeManipulatorCreator>> GetManipulatorCreators();
    }

    /// <summary>
    /// Creates a set of manipulators for the given node by inspecting its numeric inputs
    /// </summary>
    public interface INodeManipulatorCreator
    {
        IManipulator Create(NodeModel node, DynamoManipulatorContext manipulatorContext);
    }

    public interface IManipulator : IGraphicItem, IDisposable
    {
        
    }

}
