using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevitServices.Persistence;

namespace Revit.References
{
    /// <summary>
    /// A stable reference to a Revit Face, usually derived from a Revit Element
    /// </summary>
    /// See: http://revitapisearch.com.s3-website-us-east-1.amazonaws.com/html/f3d5d2fe-96bf-8528-4628-78d8d5e6705f.htm
    public class FaceReference : AbstractReference
    {
        internal FaceReference(Autodesk.Revit.DB.Face face)
        {
            if (face.Reference == null)
            {
                throw new Exception("A Face Reference can only be obtained "
                                    + "from an Element.");
            }
            this.InternalReference = face.Reference;
        }
    }

}
