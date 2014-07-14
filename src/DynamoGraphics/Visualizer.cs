using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Platform;
using Config = OpenTK.Configuration;
using Utilities = OpenTK.Platform.Utilities;

namespace Dynamo.Graphics
{
    class Visualizer
    {
        #region Public Class Operational Methods

        internal void Create()
        {
            var info = Utilities.CreateWindowsWindowInfo(IntPtr.Zero);
        }

        internal void Destroy()
        {
        }

        #endregion
    }
}
