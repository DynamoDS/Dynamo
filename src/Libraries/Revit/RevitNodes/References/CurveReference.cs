using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.References
{
    /// <summary>
    /// A stable reference to a Revit curve, usually derived from a Revit Element
    /// </summary>
    /// See: http://revitapisearch.com.s3-website-us-east-1.amazonaws.com/html/d5e10517-24fa-4627-43be-8981746d30c8.htm
    public class CurveReference : AbstractReference
    {
        internal CurveReference(Autodesk.Revit.DB.Curve curve)
        {
            if ( curve.Reference == null )
            {
                throw new Exception("A Curve Reference can only be obtained "
                                    + "from an Element.");
            }
            this.InternalReference = curve.Reference;
        }

        // do NOT expose constructors for these types that are not using 
        // Revit elements

    }
}
