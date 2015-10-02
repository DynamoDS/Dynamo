using System;
using System.Collections.Generic;
using System.Windows.Input;
using Autodesk.DesignScript.Interfaces;
using HelixToolkit.Wpf.SharpDX;


namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public interface IWatch3DViewModel
    {
        Ray3D GetClickRay(MouseEventArgs args);

        void AddGeometryForRenderPackages(IEnumerable<IRenderPackage> packages);

        void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true);

        event Action<object, MouseButtonEventArgs> ViewMouseDown;

        event Action<object, MouseButtonEventArgs> ViewMouseUp;

        event Action<object, MouseEventArgs> ViewMouseMove;
    }
}
