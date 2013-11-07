using System;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;

namespace DSRevitNodes
{
    class DSFace : AbstractGeometry
    {

        public Autodesk.Revit.DB.Face InternalFace
        {
            get; private set;
        }

    }
}
