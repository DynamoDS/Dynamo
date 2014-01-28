using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit.References
{
    /// <summary>
    /// A Reference to a plane extracted from a Revit ELement
    /// </summary>
    public class PlaneReference : AbstractReference
    {
        internal PlaneReference(Autodesk.Revit.DB.Reference planeReference)
        {
            this.InternalReference = planeReference;
        }
    }
}
