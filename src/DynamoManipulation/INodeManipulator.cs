using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;

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
        //NodeModel Node { get; }

        //Point Origin { get; }

        //IWatch3DViewModel BackgroundPreviewViewModel { get; }

        //IRenderPackageFactory RenderPackageFactory { get; }

        //Point3D? CameraPosition { get; }

        IEnumerable<IRenderPackage> BuildRenderPackage();
    }
}
