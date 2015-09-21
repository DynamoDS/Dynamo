using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.DesignScript.Interfaces;
using HelixToolkit.Wpf.SharpDX;

namespace Dynamo.Wpf.Views.Preview
{
    public interface IWatch3DView
    {
        Viewport3DX View { get; } 

        void AddGeometryFromRenderPackages(IEnumerable<IRenderPackage> packages);

        void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true);

        Ray3D GetClickRay(MouseEventArgs mouseButtonEventArgs);
    }
}
