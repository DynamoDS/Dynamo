using System;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace DSRevitNodes
{
    /// <summary>
    /// Superclass of all geometry
    /// </summary>
    public abstract class AbstractGeometry : IDisposable
    {
        public static Document Document
        {
            get { return DocumentManager.GetInstance().CurrentDBDocument; }
        }


        public abstract void Dispose();
    }
}