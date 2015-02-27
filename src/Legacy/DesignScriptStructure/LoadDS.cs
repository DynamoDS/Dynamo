using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autodesk.DesignScript.Structure
{
    public class LoadDS
    {
        public double X { get; protected set; }
        public double Y { get; protected set; }
        public double Z { get; protected set; }

        public LoadDS Copy(LoadDS load)
        {
            X=load.X;
            Y=load.Y;
            Z=load.Z;
            return this;
        }
    }

    

}

