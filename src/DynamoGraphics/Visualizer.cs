using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Platform;
using Config = OpenTK.Configuration;
using Utilities = OpenTK.Platform.Utilities;
using OpenTK.Graphics.OpenGL4;

namespace Dynamo.Graphics
{
    class Visualizer
    {
        #region Class Data Members

        IWindowInfo windowInfo = null;
        IGraphicsContext context = null;

        #endregion

        #region Public Class Operational Methods

        internal Visualizer(IntPtr windowHandle)
        {
            windowInfo = Utilities.CreateWindowsWindowInfo(windowHandle);
            var mode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 0);

            context = new GraphicsContext(mode, windowInfo);
            context.MakeCurrent(windowInfo);

            // Finalize graphics context creation process.
            (context as IGraphicsContextInternal).LoadAll();
        }

        internal void Destroy()
        {
        }

        internal void Render(int width, int height)
        {
            context.MakeCurrent(windowInfo);
            GL.Viewport(0, 0, width, height);

            GL.ClearColor(0.117647f, 0.117647f, 0.117647f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit |
                ClearBufferMask.DepthBufferBit |
                ClearBufferMask.StencilBufferBit);

            context.SwapBuffers();
        }

        #endregion
    }
}
