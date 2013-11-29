using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes.References
{
    /// <summary>
    /// A Reference to a plane extracted from a Revit ELement
    /// </summary>
    public class DSPlaneReference : AbstractReference
    {
        internal DSPlaneReference(Autodesk.Revit.DB.Reference planeReference)
        {
            this.InternalReference = planeReference;
        }
    }
}
