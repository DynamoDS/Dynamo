using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Interfaces;

using Dynamo.Interfaces;

namespace Dynamo.Wpf
{
    /// <summary>
    /// A Helix-specific IRenderPackageFactory implementation.
    /// </summary>
    internal class HelixRenderPackageFactory : IRenderPackageFactory
    {
        public HelixRenderPackageFactory(){}

        public IRenderPackage CreateRenderPackage()
        {
            return new HelixRenderPackage();
        }
    }

    /// <summary>
    /// A Helix-specifc IRenderPackage implementation.
    /// </summary>
    internal class HelixRenderPackage : IRenderPackage
    {
        public void PushPointVertex(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public void PushPointVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            throw new NotImplementedException();
        }

        public void PushTriangleVertex(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public void PushTriangleVertexNormal(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public void PushTriangleVertexUV(double u, double v)
        {
            throw new NotImplementedException();
        }

        public void PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            throw new NotImplementedException();
        }

        public void PushLineStripVertex(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public void PushLineStripVertexCount(int n)
        {
            throw new NotImplementedException();
        }

        public void PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public List<double> LineStripVertices
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> PointVertices
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> TriangleVertices
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> TriangleNormals
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<double> TriangleUVs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<byte> PointVertexColors
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<byte> LineStripVertexColors
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<byte> TriangleVertexColors
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<int> LineStripVertexCounts
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Tag
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IntPtr NativeRenderPackage
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
