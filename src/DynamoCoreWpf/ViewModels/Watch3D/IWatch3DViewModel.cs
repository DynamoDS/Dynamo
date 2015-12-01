using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Graph.Nodes;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// Interface to expose origin point and direction vector of any given Ray
    /// </summary>
    public interface IRay
    {
        Point3D Origin { get; }

        Vector3D Direction { get; }
    }

    /// <summary>
    /// An interface to expose API's on the Watch UI Viewmodel to extensions
    /// </summary>
    public interface IWatch3DViewModel
    {
        /// <summary>
        /// Returns a 3D ray from the camera to the given mouse location
        /// in world coordinates that can be used to perform a hit-test 
        /// on objects in the view
        /// </summary>
        /// <param name="args">mouse click location in screen coordinates</param>
        /// <returns></returns>
        IRay GetClickRay(MouseEventArgs args);

        /// <summary>
        /// Converts render packages into drawable geometry primitives 
        /// for display in the canvas
        /// </summary>
        /// <param name="packages">render packages to be drawn</param>
        void AddGeometryForRenderPackages(IEnumerable<IRenderPackage> packages);

        /// <summary>
        /// Finds a geometry corresponding to a string identifier
        /// and removes it from the collection of geometry objects to be drawn
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="requestUpdate"></param>
        void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true);

        /// <summary>
        /// Highlight geometry corresponding to their respective nodes 
        /// </summary>
        /// <param name="nodes"></param>
        void HighlightNodeGraphics(IEnumerable<NodeModel> nodes);

        /// <summary>
        /// Unhighlight geometry corresponding to their respective nodes 
        /// </summary>
        /// <param name="nodes"></param>
        void UnHighlightNodeGraphics(IEnumerable<NodeModel> nodes);

        #region Watch view Events to be handled by extensions

        /// <summary>
        /// Event to be handled for a mouse down event in the Watch view
        /// </summary>
        event Action<object, MouseButtonEventArgs> ViewMouseDown;

        /// <summary>
        /// Event to be handled for a mouse up event in the Watch view
        /// </summary>
        event Action<object, MouseButtonEventArgs> ViewMouseUp;

        /// <summary>
        /// Event to be handled for a mouse move event in the Watch view
        /// </summary>
        event Action<object, MouseEventArgs> ViewMouseMove;

        /// <summary>
        /// Event to be handled when the background preview is toggled on or off
        /// On/off state is passed using the bool parameter
        /// </summary>
        event Action<bool> CanNavigateBackgroundPropertyChanged;

        #endregion
    }
}
