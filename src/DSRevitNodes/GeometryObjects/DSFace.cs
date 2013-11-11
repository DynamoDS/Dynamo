using System;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;

namespace DSRevitNodes
{
    public class DSFace
    {
        internal Autodesk.Revit.DB.Face InternalFace
        {
            get; private set;
        }

        internal DSFace(Autodesk.Revit.DB.Face face)
        {
            InternalSetFace(face);
        }

        private void InternalSetFace(Autodesk.Revit.DB.Face face)
        {
            this.InternalFace = face;
        }

        public bool IsTwoSided
        {
            get
            {
                return InternalFace.IsTwoSided;
            }
        }
    }
}
