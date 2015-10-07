﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Autodesk.DesignScript.Interfaces;
using HelixToolkit.Wpf.SharpDX;


namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// An interface to expose API's on the Watch UI Viewmodel to extensions
    /// </summary>
    public interface IWatch3DViewModel
    {
        /// <summary>
        /// Returns a 3D ray from the camera to the given a mouse location
        /// in world coordinates that can be used to perform a hit-test 
        /// on objects in the view
        /// </summary>
        /// <param name="args">mouse click location in screen coordinates</param>
        /// <returns></returns>
        Ray3D GetClickRay(MouseEventArgs args);

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
        /// Event to be raised for a mouse down event in the Watch view
        /// </summary>
        event Action<object, MouseButtonEventArgs> ViewMouseDown;

        /// <summary>
        /// Event to be raised for a mouse up event in the Watch view
        /// </summary>
        event Action<object, MouseButtonEventArgs> ViewMouseUp;

        /// <summary>
        /// Event to be raised for a mouse move event in the Watch view
        /// </summary>
        event Action<object, MouseEventArgs> ViewMouseMove;
    }
}
