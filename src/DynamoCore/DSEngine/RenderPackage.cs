using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.DSEngine
{
    class RenderPackage: IRenderPackage, IDisposable
    {
        private IntPtr nativeRenderPackage;
        private RenderDescription rd;

        public RenderPackage(RenderDescription rd)
        {
            nativeRenderPackage = DesignScriptStudio.Renderer.RenderPackageUtils.CreateNativeRenderPackage(this);
            this.rd = rd;
        }

        public IntPtr NativeRenderPackage
        {
            get 
            {
                return nativeRenderPackage;
            }
        }

        public void PushLineStripVertex(double x, double y, double z)
        {
            rd.Lines.Add(new System.Windows.Media.Media3D.Point3D(x, y, z));
        }

        public void PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
        }

        public void PushLineStripVertexCount(int n)
        {
            return;
        }

        public void PushPointVertex(double x, double y, double z)
        {
            rd.Points.Add(new System.Windows.Media.Media3D.Point3D(x, y, z));
        }

        public void PushPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            return;
        }

        public void PushTriangleVertex(double x, double y, double z)
        {
            return;
        }

        public void PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            return;
        }

        public void PushTriangleVertexNormal(double x, double y, double z)
        {
            return;
        }

        public void AddToRenderDescription(RenderDescription rd)
        {
        }

        public void Dispose()
        {
            DesignScriptStudio.Renderer.RenderPackageUtils.DestroyNativeRenderPackage(nativeRenderPackage);
        }
    }
}
