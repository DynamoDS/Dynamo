using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;

namespace Dynamo.Manipulation
{
    /// <summary>
    /// Initializes a list of manipulator factories
    /// </summary>
    public interface INodeManipulatorFactoryLoader
    {
        Dictionary<Type, IEnumerable<INodeManipulatorFactory>> Load();
    }

    /// <summary>
    /// Creates a set of manipulators for the given node 
    /// </summary>
    public interface INodeManipulatorFactory
    {
        INodeManipulator Create(NodeModel node, DynamoManipulationExtension manipulatorContext);
    }

    /// <summary>
    /// Represents a manipulator for a given Node type
    /// </summary>
    public interface INodeManipulator : IDisposable
    {
        RenderPackageCache BuildRenderPackage();
    }
}
