using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;

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
    /// These constants are defined to specify special render packages, so that
    /// they can be rendered differently.
    /// 
    /// If a render package description starts with any of the following constants
    /// that will be rendered differently.
    /// 
    /// We need to ensure that all the strings are of same length and starts with 
    /// an alphabet. Usually these are taken from the last part of a GUID.
    /// </summary>
    public struct RenderDescriptions
    {
        /// <summary>
        /// Draws line strip segment as a 3D arrow.
        /// </summary>
        public const string ManipulatorAxis   = "B0C5DE5EB5CA";

        /// <summary>
        /// Draws line strip segment as thin(0.3) line to represent axis line.
        /// </summary>
        public const string AxisLine          = "C4F6AC80953B";

        /// <summary>
        /// Draws a line strip segment as little thinner(0.7) line to represent a plane.
        /// </summary>
        public const string ManipulatorPlane  = "E75B2B0E31F1";
    }
    
    /// <summary>
    /// An interface to expose API's on the Watch UI ViewModel to extensions
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
        /// Represents the name of current IWatch3DViewModel which will be saved in preference settings
        /// </summary>
        string PreferenceWatchName { get; }

        /// <summary>
        /// Returns the current camera position of the 3D background preview
        /// Note: GetCameraInformation returns the camera position but without the correct
        /// transformation to model coordinates. This function takes care of that transformation
        /// TODO: Task to fix GetCameraInformation to return the correct camera position
        /// so that we can remove this API and simply use GetCameraInformation consistently
        /// </summary>
        /// <returns></returns>
        Point3D? GetCameraPosition();

        /// <summary>
        /// Returns information about camera position in background 3D preview
        /// </summary>
        /// <returns>Information about camera position</returns>
        CameraData GetCameraInformation();

        /// <summary>
        /// Converts render packages into drawable geometry primitives 
        /// for display in the canvas
        /// </summary>
        /// <param name="packages">render packages to be drawn</param>
        /// <param name="forceAsyncCall">set to 'true' if calling from UI thread and still need to queue 
        /// the creation of display geometry for asynchronous execution</param>
        void AddGeometryForRenderPackages(RenderPackageCache packages, bool forceAsyncCall = false);

        /// <summary>
        /// Finds a geometry corresponding to a string identifier
        /// and removes it from the collection of geometry objects to be drawn
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="requestUpdate"></param>
        void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true);

        /// <summary>
        /// Finds geometries corresponding to a node and remove
        /// them from the collection of geometry objects to be drawn.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="requestUpdate"></param>
        void DeleteGeometryForNode(NodeModel node, bool requestUpdate = true);

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

        /// <summary>
        /// Invoke an Action synchronously on the UI thread via the ViewModel's Dispatcher 
        /// </summary>
        /// <param name="action"></param>
        void Invoke(Action action);

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

        /// <summary>
        /// Camera changed events to be handled for zoom/pan/rotate events in watch view
        /// </summary>
        event Action<object, RoutedEventArgs> ViewCameraChanged;

        #endregion
    }
}
